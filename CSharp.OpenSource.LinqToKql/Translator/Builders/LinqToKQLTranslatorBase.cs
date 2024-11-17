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
    {
        if (expression is UnaryExpression unaryExpression)
        {
            expression = unaryExpression.Operand;
        }
        if (expression is LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
        }

        throw new NotSupportedException($"{GetType().Name} - Unsupported expression type.");
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
