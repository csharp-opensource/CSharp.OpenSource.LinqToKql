using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Builders;

public class WhereLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public WhereLinqToKQLTranslator() : base(new() { nameof(Enumerable.Where) })
    {
    }

    public override string Handle(MethodCallExpression methodCall)
    {
        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var condition = Build(lambda.Body);
        return $"where {condition}";
    }

    private string Build(Expression expression)
        => expression switch
        {
            BinaryExpression binary => $"{Build(binary.Left)} {GetOperator(binary.NodeType)} {Build(binary.Right)}",
            MemberExpression member => member.Member.Name!,
            ConstantExpression constant => constant.Value!.ToString()!,
            _ => throw new NotSupportedException($"Expression type {expression.GetType()} is not supported."),
        };

    private string GetOperator(ExpressionType nodeType)
        => nodeType switch
        {
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Operator {nodeType} is not supported.")
        };
}