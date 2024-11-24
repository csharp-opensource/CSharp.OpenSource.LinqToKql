using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class SelectLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public SelectLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(config, new() { nameof(Enumerable.Select) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var isAfterGroupBy = (methodCall.Arguments[0] as MethodCallExpression)?.Method.Name == "GroupBy";
        var props = SelectMembers(lambda.Body, isAfterGroupBy);
        if (string.IsNullOrEmpty(props)) { return ""; }
        var action = props.Contains("dynamic(") ? "extend" : "project";
        return $"{action} {props}";
    }
}