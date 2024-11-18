using CSharp.OpenSource.LinqToKql.Translator.Builders;
using System.Linq.Expressions;
using System.Text;

namespace CSharp.OpenSource.LinqToKql.Translator;

public class LinqToKQLQueryTranslator
{
    private List<LinqToKQLTranslatorBase> _translators = new List<LinqToKQLTranslatorBase> {
        new SelectLinqToKQLTranslator(),
        new WhereLinqToKQLTranslator(),
        new GroupByLinqToKQLTranslator(),
        new OrderByLinqToKQLTranslator(),
        new TaskLinqToKQLTranslator(),
    };

    public string PipeWithIndentation = "\n  |  ";

    public string Translate<T>(IQueryable<T> query, string tableName)
    {
        return Translate(query.Expression, tableName);
    }

    public string Translate(Expression expression, string tableName)
    {
        var kqlBuilder = new StringBuilder(tableName);
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
        TranslateExpression(methodCall.Arguments[0], kqlBuilder, expression);

        // Handle the current method call
        var translator = _translators.FirstOrDefault(t => t.LinqMethods.Contains(methodCall.Method.Name));
        if (translator is null)
        {
            throw new NotSupportedException($"{GetType().Name} - Method {methodCall.Method.Name} is not supported.");
        }
        var kql = translator.Handle(methodCall, parent);
        if (!string.IsNullOrEmpty(kql))
        {
            kqlBuilder.Append($"{PipeWithIndentation}{kql}");
        }
    }
}