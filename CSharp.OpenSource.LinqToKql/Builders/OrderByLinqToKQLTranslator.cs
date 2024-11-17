using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Builders;

public class OrderByLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public OrderByLinqToKQLTranslator() : base(new() { nameof(Enumerable.OrderBy), nameof(Enumerable.OrderByDescending) })
    {
    }

    public override string Handle(MethodCallExpression methodCall) 
    {
        return Handle(methodCall, methodCall.Method.Name == nameof(Enumerable.OrderByDescending));
    } 

    public string Handle(MethodCallExpression methodCall, bool descending)
    {
        var keySelector = methodCall.Arguments[1];
        var key = GetMemberName(keySelector);
        var direction = descending ? "desc" : "asc";
        return $"sort by {key} {direction}";
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
        throw new NotSupportedException("Unsupported expression type for OrderBy key selector.");
    }
}