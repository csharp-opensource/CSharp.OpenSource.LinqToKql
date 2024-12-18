using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class DistinctLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public DistinctLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(config, new() { nameof(Enumerable.Distinct) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        return "distinct *";
    }
}
