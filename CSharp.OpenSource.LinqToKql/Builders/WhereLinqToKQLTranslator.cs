using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Builders;

public class WhereLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public WhereLinqToKQLTranslator() : base(new() { nameof(Enumerable.Where) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
        var condition = Build(lambda.Body);
        return $"where {condition}";
    }

    private string Build(Expression expression)
        => expression switch
        {
            BinaryExpression binary => BuildBinaryOperation(binary),
            MemberExpression member when member.Expression is ConstantExpression c => GetValue(c, member),
            MemberExpression member => member.Member.Name!,
            NewExpression newExpression => GetValue(Expression.Lambda(newExpression).Compile().DynamicInvoke()),
            ConstantExpression constant => GetValue(constant.Value),
            _ => throw new NotSupportedException($"Expression type {expression.GetType()} is not supported."),
        };

    private string BuildBinaryOperation(BinaryExpression binary)
    {
        var left = Build(binary.Left);
        var right = Build(binary.Right);
        var op = GetOperator(binary.NodeType);
        return $"{left} {op} {right}";
    }

    private string GetValue(ConstantExpression constant, MemberExpression? member)
    {
        if (member == null) { return GetValue(constant.Value); }
        var field = constant.Type.GetField(member.Member.Name)!;
        return GetValue(field.GetValue(constant.Value));
    }

    private string GetValue(NewExpression newExpression)
    {
        var compiledValue = Expression.Lambda(newExpression).Compile().DynamicInvoke();
        return GetValue(compiledValue);
    }

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