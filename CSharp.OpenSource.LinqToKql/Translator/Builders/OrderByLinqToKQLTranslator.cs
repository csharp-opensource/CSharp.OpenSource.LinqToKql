using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class OrderByLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public OrderByLinqToKQLTranslator() : base(new() { nameof(Enumerable.OrderBy), nameof(Enumerable.OrderByDescending) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
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
}