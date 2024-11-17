using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Builders;

public class GroupByLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public GroupByLinqToKQLTranslator() : base(new() { nameof(Enumerable.GroupBy) })
    {
    }

    public override string Handle(MethodCallExpression methodCall)
    {
        var keySelector = methodCall.Arguments[1];
        var key = GetMemberName(keySelector);
        return $"summarize Count=count() by {key}";
    }

    private string GetMemberName(Expression expression)
    {
        if (expression is LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
        }
        throw new NotSupportedException("Unsupported expression type for GroupBy key selector.");
    }
}