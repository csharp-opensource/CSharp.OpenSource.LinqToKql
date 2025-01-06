using System.Linq.Expressions;
using System.Reflection;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class SelectLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    private readonly string[] _extendOps = new[]
    {
        "bag_pack(",
        "iff("
    };
    public SelectLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(config, new() { nameof(Enumerable.Select), nameof(Enumerable.DistinctBy), nameof(Enumerable.Distinct) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        if (methodCall.Method.Name == nameof(Enumerable.Distinct))
        {
            var elementType = methodCall.Arguments[0].Type.GetGenericArguments().First();
            var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propNames = properties.Select(p => p.Name).ToArray();
            return $"distinct {string.Join(", ", propNames)}";
        }

        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var isAfterGroupBy = (SkipCast(methodCall.Arguments[0]) as MethodCallExpression)?.Method.Name == "GroupBy";
        var props = SelectMembers(lambda.Body, isAfterGroupBy);
        if (string.IsNullOrEmpty(props)) { return ""; }
        var action = methodCall.Method.Name == nameof(Enumerable.DistinctBy) ? "distinct" : (_extendOps.Any(props.Contains) ? "extend" : "project");
        return $"{action} {props}";
    }
}
