using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public abstract class LinqToKQLTranslatorBase
{
    public HashSet<string> LinqMethods { get; private set; }

    protected LinqToKQLTranslatorBase(HashSet<string> linqMethods)
    {
        LinqMethods = linqMethods;
    }

    public abstract string Handle(MethodCallExpression methodCall, Expression? parent);

    protected string GetMemberName(Expression expression)
        => SelectMembers(expression);

    protected string SelectMembers(Expression expression, bool isAfterGroupBy = false)
    {
        return expression switch
        {
            MethodCallExpression methodCallExpression => GetArgMethod(methodCallExpression),
            UnaryExpression unary => SelectMembers(unary.Operand),
            LambdaExpression lambda => SelectMembers(lambda.Body),
            MemberInitExpression memberInitExpression => SelectInitMembers(memberInitExpression, isAfterGroupBy),
            NewExpression newExpr => string.Join(", ", newExpr.Members!.Select(m => m.Name)),
            MemberExpression member => member.Member.Name,
            _ => throw new NotSupportedException($"{GetType().Name} - Expression type {expression.GetType()} is not supported, expression={expression}."),
        };
    }

    private string SelectInitMembers(MemberInitExpression memberInitExpression, bool isAfterGroupBy)
    {
        var projections = new List<string>();
        foreach (var binding in memberInitExpression.Bindings)
        {
            var name = binding.Member.Name;
            var value = isAfterGroupBy 
                ? name
                : SelectMembers(((MemberAssignment)binding).Expression);
            projections.Add($"{name}={value}");
        }
        return string.Join(", ", projections);
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
