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
            throw new InvalidOperationException($"{GetType().Name} - Method call must have at least two arguments (source, key selector). Check the LINQ expression.");
        }

        var keySelector = methodCall.Arguments[1];
        var key = SelectMembers(keySelector);
        if (key == null)
        {
            throw new InvalidOperationException($"{GetType().Name} - Key selector expression is invalid or not supported.");
        }

        var aggregation = methodCall.Arguments.Count == 2
            ? AggrigationWithTwoArgs(methodCall, parent, key)
            : GetAggregation((methodCall.Arguments[2] as LambdaExpression)!, key);

        return string.IsNullOrEmpty(aggregation)
            ? $"summarize by {key}"
            : $"summarize {aggregation} by {key}";
    }

    private string AggrigationWithTwoArgs(MethodCallExpression methodCall, Expression? parent, string key)
    {
        string aggregation;
        Expression? expression = null;
        if (methodCall.Object is MethodCallExpression innerSelectCall && innerSelectCall.Method.Name == nameof(Enumerable.Select))
        {
            expression = innerSelectCall.Arguments[1];
        }
        expression ??= (parent as MethodCallExpression)?.Arguments[1];
        if (expression == null)
        {
            return "";
        }
        aggregation = GetAggregation(expression, key);
        return aggregation;
    }

    private string GetAggregation(Expression expression, string keyName)
    {
        if (expression is UnaryExpression unary)
        {
            expression = unary.Operand;
        }
        if (expression is LambdaExpression lambda)
        {
            expression = lambda.Body;
        }
        if (expression is MemberExpression mb1)
        {
            return mb1.Member.Name;
        }
        if (expression is NewExpression newExpression)
        {
            var aggregations = new List<string>();
            for (int i = 0; i < newExpression.Arguments.Count; i++)
            {
                var arg = newExpression.Arguments[i];
                var argName = newExpression.Members![i].Name;
                var argValue = GetArgMethod(arg);
                if (argValue == "Key") { continue; }
                aggregations.Add($"{argName}={argValue}");
            }
            return string.Join(", ", aggregations);
        }
        else if (expression is MemberInitExpression memberInit)
        {
            var aggregations = new List<string>();
            foreach (var binding in memberInit.Bindings)
            {
                var argName = binding.Member.Name;
                var expresion = ((MemberAssignment)binding).Expression;
                var argValue = GetArgMethod(expresion);
                if (argValue == "Key") { continue; }
                aggregations.Add($"{argName}={argValue}");
            }
            return string.Join(", ", aggregations);
        }
        else if (expression is MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;
            if (methodCall.Arguments.Count > 0 && methodCall.Arguments[0] is MemberExpression mb3)
            {
                return $"{methodName}({mb3.Member.Name})";
            }
            else
            {
                throw new InvalidOperationException($"{GetType().Name} - Aggregation method must operate on a member expression.");
            }
        }
        throw new InvalidOperationException($"{GetType().Name} - Invalid aggregation selector expression");
    }
}
