using CSharp.OpenSource.LinqToKql.Translator.Builders;
using System.Linq.Expressions;
using System.Text;

namespace CSharp.OpenSource.LinqToKql.Translator;

public class LinqToKQLQueryTranslator
{
    private List<LinqToKQLTranslatorBase> _translators;

    public LinqToKQLQueryTranslatorConfig Config { get; set; }

    public LinqToKQLQueryTranslator(LinqToKQLQueryTranslatorConfig? config = null)
    {
        Config = config ?? new();
        _translators = new()
        {
            new SelectLinqToKQLTranslator(Config),
            new WhereLinqToKQLTranslator(Config),
            new GroupByLinqToKQLTranslator(Config),
            new OrderByLinqToKQLTranslator(Config),
            new TaskLinqToKQLTranslator(Config),
        };
    }

    public string PipeWithIndentation => Config.PipeWithIndentation;

    public string Translate<T>(IQueryable<T> query, string tableOrKQL)
    {
        return Translate(query.Expression, tableOrKQL);
    }

    public string Translate(Expression expression, string tableOrKQL)
    {
        var kqlBuilder = new StringBuilder(tableOrKQL);
        TranslateExpression(expression, kqlBuilder, null);
        return kqlBuilder.ToString();
    }

    private void TranslateExpression(Expression expression, StringBuilder kqlBuilder, Expression? parent)
    {
        if (expression is not MethodCallExpression methodCall)
        {
            return;
        }

        // Recursively translate the inner expression
        var methodName = methodCall.Method.Name;
        var child = methodCall.Arguments[0];
        var isCast = methodName == nameof(Enumerable.Cast);
        var childParent = isCast ? parent : methodCall;
        TranslateExpression(child, kqlBuilder, childParent);

        // Handle the current method call
        if (isCast)
        {
            return;
        }
        var translator = _translators.FirstOrDefault(t => t.LinqMethods.Contains(methodName));
        if (translator is null)
        {
            throw new NotSupportedException($"{GetType().Name} - Method {methodName} is not supported.");
        }
        var kql = translator.Handle(methodCall, parent);
        if (!string.IsNullOrEmpty(kql))
        {
            kqlBuilder.Append($"{PipeWithIndentation}{kql}");
        }
    }
}
