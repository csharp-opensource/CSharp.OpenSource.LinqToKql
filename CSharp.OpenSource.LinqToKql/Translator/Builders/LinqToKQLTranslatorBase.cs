using CSharp.OpenSource.LinqToKql.Translator.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public abstract class LinqToKQLTranslatorBase
{
    public HashSet<string> LinqMethods { get; private set; }
    protected LinqToKQLQueryTranslatorConfig Config { get; private set; }

    protected LinqToKQLTranslatorBase(LinqToKQLQueryTranslatorConfig config, HashSet<string> linqMethods)
    {
        LinqMethods = linqMethods;
        Config = config;
    }

    public abstract string Handle(MethodCallExpression methodCall, Expression? parent);

    protected string SelectMembers(Expression expression, bool isAfterGroupBy = false)
    {
        return expression switch
        {
            MethodCallExpression methodCallExpression => GetArgMethod(methodCallExpression),
            UnaryExpression unary => SelectMembers(unary.Operand),
            LambdaExpression lambda => SelectMembers(lambda.Body),
            MemberInitExpression memberInitExpression => SelectInitMembers(memberInitExpression, isAfterGroupBy),
            NewExpression newExpr => isAfterGroupBy ? "" : SelectNewExpression(newExpr),
            MemberExpression member => member.Expression == null || member.Expression is ParameterExpression ? member.Member.Name : $"{SelectMembers(member.Expression)}.{member.Member.Name}",
            ConstantExpression constant => GetValue(constant.Value),
            _ => throw new NotSupportedException($"{GetType().Name} - Expression type {expression.GetType()} is not supported, expression={expression}."),
        };
    }

    private string SelectNewExpression(NewExpression newExpr)
    {
        var membersWithArgs = newExpr.Arguments.Select((arg, index) => new
        {
            arg,
            argName = (arg as MemberExpression)?.Member.Name,
            member = newExpr.Members?[index],
            memberName = newExpr.Members?[index]?.Name,
        });
        var res = new List<string>();
        foreach (var item in membersWithArgs)
        {
            var arg = item.arg;
            var member = item.member;
            if (arg.NodeType == ExpressionType.MemberAccess || Config.DisableNestedProjection)
            {
                res.Add(item.memberName == item.argName || arg.NodeType != ExpressionType.MemberAccess ? item.memberName! : $"{item.memberName}={item.argName}");
                continue;
            }
            if (arg is ConditionalExpression conditionalExpression)
            {
                var conditionKQL = BuildFilter(conditionalExpression.Test);
                var ifTrueKQL = SelectMembers(conditionalExpression.IfTrue);
                var ifFalseKQL = SelectMembers(conditionalExpression.IfFalse);
                res.Add($"{item.memberName}=iff(({conditionKQL}),{ifTrueKQL},{ifFalseKQL})");
                continue;
            }
            var members = arg.NodeType switch
            {
                ExpressionType.MemberInit => SelectInitMembersModels(arg as MemberInitExpression, false),
                ExpressionType.New => SelectNewMembersModels(arg as NewExpression),
                _ => throw new NotImplementedException(arg.NodeType.ToString()),
            };
            var dynamicBlock = $"{item.memberName}=dynamic({{";
            foreach (var m in members)
            {
                dynamicBlock += $"\"{m.Name}\":{m.Value}";
            }
            dynamicBlock += "})";
            res.Add(dynamicBlock);
        }
        return string.Join(", ", res);
    }

    private List<SelectMemeberModel> SelectNewMembersModels(NewExpression newExpr)
    {
        var res = new List<SelectMemeberModel>();
        foreach (var (arg, index) in newExpr.Arguments.Select((arg, index) => (arg, index)))
        {
            var member = newExpr.Members[index];
            var value = SelectMembers(arg);
            res.Add(new() { Name = member.Name, Value = value, });
        }
        return res;
    }

    private string SelectInitMembers(MemberInitExpression memberInitExpression, bool isAfterGroupBy)
    {
        var members = SelectInitMembersModels(memberInitExpression, isAfterGroupBy);
        var isAllValuesEq = members.All(x => x.Name == x.Value);
        return isAllValuesEq ? "" : string.Join(", ", members.Select(x => $"{x.Name}={x.Value}"));
    }

    private List<SelectMemeberModel> SelectInitMembersModels(MemberInitExpression memberInitExpression, bool isAfterGroupBy)
    {
        var res = new List<SelectMemeberModel>();
        foreach (var binding in memberInitExpression.Bindings)
        {
            var name = binding.Member.Name;
            var value = isAfterGroupBy
                ? name
                : SelectMembers(((MemberAssignment)binding).Expression);
            res.Add(new() { Name = name, Value = value });
        }
        return res;
    }

    protected string GetArgMethod(Expression arg)
    {
        if (arg is UnaryExpression unaryExpression)
        {
            arg = unaryExpression.Operand;
        }
        if (arg is MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;
            if (methodName == "Count")
            {
                return "count()";
            }
            throw new InvalidOperationException($"{GetType().Name} - fail to translate");
        }
        else if (arg is MemberExpression mb2)
        {
            var propName = mb2.Member.Name;
            return propName;
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} - Unsupported expression type for aggregation.");
        }
    }

    protected string GetValue(object? value)
        => value switch
        {
            bool boolean => boolean.ToString().ToLower(),
            TimeOnly time => $"timespan({time:HH:mm:ss.f})",
            TimeSpan timeSpan => $"timespan({timeSpan.Days}.{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2})",
            DateTime dateTime => $"datetime({dateTime:yyyy-MM-dd HH:mm:ss.f})",
            DateOnly date => $"datetime({date:yyyy-MM-dd})",
            string str => $"'{str}'",
            _ => value?.ToString() ?? "null",
        };

    protected string BuildFilter(Expression expression)
            => expression switch
            {
                MethodCallExpression methodCall when methodCall.Method.Name == nameof(string.Contains) => $"{BuildFilter(methodCall.Arguments[1])} has {BuildFilter(methodCall.Arguments[0])}",
                UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Not => $"!({BuildFilter(unaryExpression.Operand)})",
                BinaryExpression binary => BuildBinaryOperation(binary),
                MemberExpression member when member.Expression is ConstantExpression c => GetValue(c, member),
                MemberExpression member => SelectMembers(member),
                NewArrayExpression newArrayExpression => $"({string.Join(", ", newArrayExpression.Expressions.Select(BuildFilter))})",
                NewExpression newExpression => GetValue(Expression.Lambda(newExpression).Compile().DynamicInvoke()),
                ConstantExpression constant => GetValue(constant.Value),
                _ => throw new NotSupportedException($"Expression type {expression.GetType()} is not supported."),
            };

    protected string BuildBinaryOperation(BinaryExpression binary)
    {
        var left = BuildFilter(binary.Left);
        var right = BuildFilter(binary.Right);
        var op = GetOperator(binary.NodeType);
        return $"{left} {op} {right}";
    }

    protected string GetValue(ConstantExpression constant, MemberExpression? member)
    {
        if (member == null) { return GetValue(constant.Value); }
        if (member.Member is FieldInfo fieldInfo)
        {
            return GetValue(fieldInfo.GetValue(constant.Value));
        }
        if (member.Member is PropertyInfo propertyInfo)
        {
            return GetValue(propertyInfo.GetValue(constant.Value));
        }
        throw new NotSupportedException($"Member type {member.Member.GetType()} is not supported.");
    }

    protected string GetValue(NewExpression newExpression)
    {
        var compiledValue = Expression.Lambda(newExpression).Compile().DynamicInvoke();
        return GetValue(compiledValue);
    }

    protected string GetOperator(ExpressionType nodeType)
        => nodeType switch
        {
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "and",
            ExpressionType.OrElse => "or",
            ExpressionType.Negate => "!",
            _ => throw new NotSupportedException($"Operator {nodeType} is not supported.")
        };
}
