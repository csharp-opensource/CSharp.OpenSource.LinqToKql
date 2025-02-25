﻿using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Translator.Models;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
            MemberExpression member => member.Expression == null || member.Expression is ParameterExpression ? GetMemberName(member.Member) : $"{SelectMembers(member.Expression)}.{GetMemberName(member.Member)}",
            ConstantExpression constant => constant.Value.GetKQLValue(),
            _ => throw new NotSupportedException($"{GetType().Name} - Expression type {expression.GetType()} is not supported, expression={expression}."),
        };
    }

    private string GetMemberName(MemberInfo memberInfo)
    {
        var attributes = memberInfo.GetCustomAttributes(true);
        var dmAttr = attributes.OfType<DataMemberAttribute>().FirstOrDefault();
        if (dmAttr != null) { return dmAttr.Name; }
        var jpnAttr = attributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault();
        if (jpnAttr != null) { return jpnAttr.Name; }
        var jpAttr = attributes.FirstOrDefault(x => x.GetType().FullName == "Newtonsoft.Json.JsonPropertyAttribute");
        if (jpAttr != null)
        {
            return jpAttr.GetType().GetProperty("PropertyName")!.GetValue(jpAttr)!.ToString()!;
        }
        return memberInfo.Name;
    }

    private string SelectNewExpression(NewExpression newExpr)
    {
        var membersWithArgs = newExpr.Arguments.Select((arg, index) => new
        {
            arg,
            argName = (arg as MemberExpression)?.Member == null ? null : GetMemberName((arg as MemberExpression)!.Member),
            member = newExpr.Members?[index],
            memberName = newExpr.Members?[index] == null ? null : GetMemberName(newExpr.Members[index]),
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
            var dynamicBlock = $"{item.memberName}=bag_pack(";
            foreach (var m in members)
            {
                dynamicBlock += $"\"{m.Name}\",{m.Value}";
            }
            dynamicBlock += ")";
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
            res.Add(new() { Name = GetMemberName(member), Value = value, });
        }
        return res;
    }

    private string SelectInitMembers(MemberInitExpression memberInitExpression, bool isAfterGroupBy)
    {
        var members = SelectInitMembersModels(memberInitExpression, isAfterGroupBy);
        var isAllValuesEq = members.All(x => x.Name == x.Value);
        return isAllValuesEq 
            ? isAfterGroupBy ? "" : string.Join(", ", members.Select(x => x.Name))
            : string.Join(", ", members.Select(x => $"{x.Name}={x.Value}"));
    }

    private List<SelectMemeberModel> SelectInitMembersModels(MemberInitExpression memberInitExpression, bool isAfterGroupBy)
    {
        var res = new List<SelectMemeberModel>();
        foreach (var binding in memberInitExpression.Bindings)
        {
            var name = GetMemberName(binding.Member);
            var value = isAfterGroupBy || Config.DisableNestedProjection
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
            return methodName switch
            {
                "Count" => "count()",
                "Sum" => $"sum({SelectMembers(methodCall.Arguments[1])})",
                "Average" => $"avg({SelectMembers(methodCall.Arguments[1])})",
                "Min" => $"min({SelectMembers(methodCall.Arguments[1])})",
                "Max" => $"max({SelectMembers(methodCall.Arguments[1])})",
                _ => throw new InvalidOperationException($"{GetType().Name} - Method {methodName} is not supported."),
            };
        }
        else if (arg is MemberExpression mb2)
        {
            var propName = GetMemberName(mb2.Member);
            return propName;
        }
        else if (arg is ConstantExpression constant)
        {
            return constant.Value.GetKQLValue();
        }
        else
        {
            throw new InvalidOperationException($"{GetType().Name} - Unsupported expression type for aggregation.");
        }
    }

    protected string BuildFilter(Expression expression)
            => expression switch
            {
                MethodCallExpression methodCall => BuildFilterCustomMethodCall(methodCall),
                UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Not => $"not({BuildFilter(unaryExpression.Operand)})",
                UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Convert => BuildFilter(unaryExpression.Operand),
                BinaryExpression binary => BuildBinaryOperation(binary),
                MemberExpression member => BuildMemberExpression(member),
                NewArrayExpression newArrayExpression => $"({string.Join(", ", newArrayExpression.Expressions.Select(BuildFilter))})",
                NewExpression newExpression => Expression.Lambda(newExpression).Compile().DynamicInvoke().GetKQLValue(),
                ConstantExpression constant => constant.Value.GetKQLValue(),
                InvocationExpression invocationExpression => BuildFilter(invocationExpression.Expression),
                LambdaExpression lambdaExpression => BuildFilter(lambdaExpression.Body),
                _ => throw new NotSupportedException($"Expression type {expression.GetType()}, {expression.NodeType} is not supported."),
            };

    private string BuildFilterCustomMethodCall(MethodCallExpression methodCall)
    {
        var methodName = methodCall.Method.Name;
        if (methodName == "KqlLike") { return HandleKqlLike(methodCall); }
        var leftSideKql = BuildFilter(methodCall.Arguments.Count > 1 ? methodCall.Arguments[1] : methodCall.Object!);
        if (methodName == "Like") { return HandleLike(methodCall, leftSideKql); }

        var rightSideKql = BuildFilter(methodCall.Arguments[0]);
        if (leftSideKql.StartsWith('(') && leftSideKql.EndsWith(')'))
        {
            (rightSideKql, leftSideKql) = (leftSideKql, rightSideKql);
        }
        return methodName switch
        {
            nameof(string.Contains) when methodCall.Method.DeclaringType == typeof(string) => $"{leftSideKql} has_cs {rightSideKql}",
            nameof(Enumerable.Contains) => $"{leftSideKql} has_any ({rightSideKql.TrimStart('(').TrimEnd(')')})",
            nameof(string.StartsWith) => $"{leftSideKql} startswith_cs {rightSideKql}",
            nameof(string.EndsWith) => $"{leftSideKql} endswith_cs {rightSideKql}",
            nameof(string.Equals) => $"{leftSideKql} == {rightSideKql}",
            _ => throw new NotSupportedException($"{nameof(BuildFilterCustomMethodCall)} - Method {methodCall.Method.Name} is not supported."),
        };

        string HandleLike(MethodCallExpression methodCall, string leftSideKql)
        {
            var likeValueConst = methodCall.Arguments
                .Where(x => x.NodeType == ExpressionType.Constant)
                .Select(x => x as ConstantExpression)
                .Where(x => !x!.Type.Name.Contains("DbFunctions"))
                .OrderBy(x => x!.Type == typeof(string) ? 0 : 1)
                .First();
            if (likeValueConst == null) { throw new NotSupportedException($"{nameof(HandleLike)} - likeValueConst is null."); }
            var likeValue = likeValueConst.Value!.ToString()!;
            var filter = likeValue.Trim('%').GetKQLValue();
            if (likeValue.StartsWith("%") && likeValue.EndsWith("%"))
            {
                return $"{leftSideKql} has_cs {filter}";
            }
            if (likeValue.StartsWith("%"))
            {
                return $"{leftSideKql} endswith_cs {filter}";
            }
            if (likeValue.EndsWith("%"))
            {
                return $"{leftSideKql} startswith_cs {filter}";
            }
            return $"{leftSideKql} == {filter}";
        }

        string HandleKqlLike(MethodCallExpression methodCall)
        {
            var likeValue = GetValue(methodCall.Arguments[1], null, false).ToString()!;
            var wildCardSymbol = SelectMembers(methodCall.Arguments[2])[0];
            var filter = likeValue.Trim(wildCardSymbol).GetKQLValue();
            var leftSideKql = BuildFilter(methodCall.Arguments[0]);
            if (likeValue.StartsWith(wildCardSymbol) && likeValue.EndsWith(wildCardSymbol))
            {
                return $"{leftSideKql} has_cs {filter}";
            }
            if (likeValue.StartsWith(wildCardSymbol))
            {
                return $"{leftSideKql} endswith_cs {filter}";
            }
            if (likeValue.EndsWith(wildCardSymbol))
            {
                return $"{leftSideKql} startswith_cs {filter}";
            }
            return $"{leftSideKql} == {filter}";
        }
    }

    private string BuildMemberExpression(MemberExpression member)
    {
        var lastExpression = member as Expression;
        while ((lastExpression as MemberExpression) != null)
        {
            if (lastExpression is not MemberExpression innerMember) { break; }
            if (innerMember.Expression == null) { break; }
            lastExpression = innerMember.Expression;
        }
        if (lastExpression.NodeType == ExpressionType.Parameter)
        {
            return SelectMembers(member);
        }
        if (member.Expression != null)
        {
            return GetValue(member.Expression, member).ToString()!;
        }
        return GetValue(member, null).ToString()!;
    }

    protected string BuildBinaryOperation(BinaryExpression binary)
    {
        var left = BuildFilter(binary.Left);
        var op = GetOperator(binary.NodeType);
        if (binary.Right is ConstantExpression c && c.Value is null)
        {
            return op == "==" ? $"isnull({left})" : $"isnotnull({left})";
        }
        
        var right = BuildFilter(binary.Right);
        return $"{left} {op} {right}";
    }

    protected object GetValue(Expression expression, MemberExpression? parentExpression, bool getAsKQLValue = true)
    {
        if (expression is ConstantExpression constant)
        {
            var val = parentExpression == null
                ? constant.Value
                : GetValueFromParent(constant.Value, parentExpression);
            return getAsKQLValue ? val.GetKQLValue() : val;
        }
        if (expression is NewExpression newExpression)
        {
            var compiledValue = Expression.Lambda(newExpression).Compile().DynamicInvoke();
            return getAsKQLValue ? compiledValue.GetKQLValue() : compiledValue;
        }
        if (expression is MemberExpression member)
        {
            if (parentExpression == null)
            {
                return GetValue(member.Expression, member, getAsKQLValue);
            }
            var innerValue = GetValue(member.Expression, member, false);
            var pVal = GetValueFromParent(innerValue, parentExpression);
            return getAsKQLValue ? pVal.GetKQLValue() : pVal;
        }
        throw new NotSupportedException($"expression type = {expression.GetType()}, Member type {parentExpression.Member.GetType()} is not supported.");
    }

    private object GetValueFromParent(object value, MemberExpression parentExpression)
    {
        if (parentExpression.Member is FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(value);
        }
        else if (parentExpression.Member is PropertyInfo propertyInfo)
        {
            return propertyInfo.GetValue(value);
        }
        else
        {
            throw new NotSupportedException();
        }
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

    protected Expression SkipCast(Expression parent)
    {
        if (parent is not MethodCallExpression methodCall)
        {
            return parent;
        }
        if (methodCall.Arguments.Count > 0 && methodCall.Method.Name == nameof(Enumerable.Cast))
        {
            return methodCall.Arguments[0];
        }
        return parent;
    }
}
