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
        var props = Build(lambda.Body);
        return $"project {props}";
    }

    private string Build(Expression expression)
    {
        return expression switch
        {
            MemberInitExpression memberInitExpression => Build(memberInitExpression.NewExpression),
            NewExpression newExpr => string.Join(", ", newExpr.Members!.Select(m => m.Name)),
            MemberExpression member => member.Member.Name,
            _ => throw new NotSupportedException($"{GetType().Name} - Expression type {expression.GetType()} is not supported, expression={expression}."),
        };
    }
}