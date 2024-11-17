using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class SelectLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public SelectLinqToKQLTranslator() : base(new() { nameof(Enumerable.Select) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var props = lambda.Body switch
        {
            NewExpression newExpr => string.Join(", ", newExpr.Members!.Select(m => m.Name)),
            MemberExpression member => member.Member.Name,
            _ => throw new NotSupportedException($"Expression type {lambda.Body.GetType()} is not supported."),
        };
        return $"project {props}";
    }
}