using CSharp.OpenSource.LinqToKql.Translator.Models;
using System.Linq.Expressions;

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
            var members = arg.NodeType switch
            {
                ExpressionType.MemberInit => SelectInitMembersModels(arg as MemberInitExpression, false),
                ExpressionType.New => SelectNewMembersModels(arg as NewExpression),
                _ => throw new NotImplementedException(""),
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
            var argMember = arg as MemberExpression;
            var value = argMember?.Expression == null || argMember?.Expression is ParameterExpression 
                ? argMember.Member.Name 
                : $"{SelectMembers(argMember.Expression)}.{argMember.Member.Name}";
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
}
