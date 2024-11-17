using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders;

public class GroupByLinqToKQLTranslator : LinqToKQLTranslatorBase
{
    public GroupByLinqToKQLTranslator() : base(new() { nameof(Enumerable.GroupBy) })
    {
    }

    public override string Handle(MethodCallExpression methodCall, Expression? parent)
    {
        if (methodCall.Arguments.Count < 2)
        {
            throw new InvalidOperationException("Method call must have at least two arguments (source, key selector). Check the LINQ expression.");
        }

        var keySelector = methodCall.Arguments[1];
        var key = GetMemberName(keySelector);
        if (key == null)
        {
            throw new InvalidOperationException("Key selector expression is invalid or not supported.");
        }

        string aggregation;
        if (methodCall.Arguments.Count == 2)
        {
            Expression? expression = null;
            if (methodCall.Object is MethodCallExpression innerSelectCall && innerSelectCall.Method.Name == nameof(Enumerable.Select))
            {
                expression = innerSelectCall.Arguments[1] as LambdaExpression;
            }
            expression ??= (parent as MethodCallExpression)?.Arguments[1];
            if (expression is UnaryExpression unaryExpression)
            {
                expression = unaryExpression.Operand as LambdaExpression;
            }

            if (expression is not LambdaExpression lambda)
            {
                throw new InvalidOperationException("Expected an inner Select method to derive the aggregation.");
            }

            aggregation = GetAggregation(lambda, key);
        }
        else
        {
            var aggregationSelector = methodCall.Arguments[2];
            aggregation = GetAggregation((aggregationSelector as LambdaExpression)!, key);
        }

        return $"summarize {aggregation} by {key}";
    }

    private string GetAggregation(Expression expression, string keyName)
    {
        if (expression is LambdaExpression lambda)
        {
            if (lambda.Body is NewExpression newExpression)
            {
                var aggregations = new List<string>();
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var arg = newExpression.Arguments[i];
                    if (arg is MethodCallExpression methodCall)
                    {
                        var methodName = methodCall.Method.Name;
                        var kql = "";
                        if (methodName == "Count")
                        {
                            kql = $"{newExpression.Members![i].Name}=count()";
                        }
                        if (string.IsNullOrEmpty(kql))
                        {
                            throw new InvalidOperationException("fail to translate");
                        }
                        aggregations.Add(kql);
                    }
                    else if (arg is MemberExpression member)
                    {
                        var propName = member.Member.Name;
                        if (propName == "Key") { continue; }
                        aggregations.Add($"{newExpression.Members![i].Name}={propName}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported expression type for aggregation.");
                    }
                }
                return string.Join(", ", aggregations);
            }
            else if (lambda.Body is MethodCallExpression methodCall)
            {
                var methodName = methodCall.Method.Name;
                if (methodCall.Arguments.Count > 0 && methodCall.Arguments[0] is MemberExpression member)
                {
                    return $"{methodName}({member.Member.Name})";
                }
                else
                {
                    throw new InvalidOperationException("Aggregation method must operate on a member expression.");
                }
            }
            else if (lambda.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
        }
        throw new InvalidOperationException("Invalid aggregation selector expression");
    }
}
