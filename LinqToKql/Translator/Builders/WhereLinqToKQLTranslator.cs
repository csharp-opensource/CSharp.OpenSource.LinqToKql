using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class WhereLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public WhereLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(
        config,
        new()
        {
            nameof(Enumerable.Where),
            nameof(Enumerable.Any),
            nameof(Enumerable.Single),
            nameof(Enumerable.SingleOrDefault),
            nameof(Enumerable.First),
            nameof(Enumerable.FirstOrDefault),
        })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var condition = BuildFilter(lambda.Body);
        var kql = $"where {condition}";
        if (methodCall.Method.Name is nameof(Enumerable.Where))
        {
            return kql;
        }
        if (methodCall.Method.Name is nameof(Enumerable.Any))
        {
            return $"{kql}{Config.PipeWithIndentation}summarize c=count(){Config.PipeWithIndentation}project res=iff(c > 0, true, false)";
        }
        return $"{kql}{Config.PipeWithIndentation}take 1";
    }
}