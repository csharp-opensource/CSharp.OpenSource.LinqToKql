using CSharp.OpenSource.LinqToKql.Translator;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public interface ILinqToKqlProvider<T> : IQueryable<T>, IQueryProvider, IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    string? DefaultDbName { get; set; }
    string TableOrKQL { get; set; }
    LinqToKQLQueryTranslator Translator { get; }

    LinqToKqlProvider<S> Clone<S>(Expression? expression = null);
    string TranslateToKQL(Expression? expression = null);
}
