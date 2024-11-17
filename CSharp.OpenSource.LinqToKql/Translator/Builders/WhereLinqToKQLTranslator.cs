using System.Linq.Expressions;
using System.Reflection;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

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
            MethodCallExpression methodCall when methodCall.Method.Name == nameof(string.Contains) => $"{Build(methodCall.Arguments[1])} has {Build(methodCall.Arguments[0])}",
            UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Not => $"!({Build(unaryExpression.Operand)})",
            BinaryExpression binary => BuildBinaryOperation(binary),
            MemberExpression member when member.Expression is ConstantExpression c => GetValue(c, member),
            MemberExpression member => member.Member.Name!,
            NewArrayExpression newArrayExpression => $"({string.Join(", ", newArrayExpression.Expressions.Select(Build))})",
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
        if (member.Member is FieldInfo fieldInfo)
        {
            return GetValue(fieldInfo.GetValue(constant.Value));
        }
        if (member.Member is PropertyInfo propertyInfo)
        {
            return GetValue(propertyInfo.GetValue(constant.Value));
        }
        throw new NotSupportedException($"Member type {member.Member.GetType()} is not supported.");
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
            ExpressionType.AndAlso => "and",
            ExpressionType.OrElse => "or",
            ExpressionType.Negate => "!",
            _ => throw new NotSupportedException($"Operator {nodeType} is not supported.")
        };
}