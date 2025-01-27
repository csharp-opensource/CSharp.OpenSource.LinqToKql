using CSharp.OpenSource.LinqToKql.Translator;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public interface ILinqToKqlProvider<T> : IQueryable<T>, IQueryProvider, IOrderedQueryable<T>, IAsyncEnumerable<T>, ICloneable, ILinqToKqlProvider
{
}

public interface ILinqToKqlProvider
{
    string? DefaultDbName { get; set; }
    string TableOrKQL { get; set; }
    LinqToKQLQueryTranslator Translator { get; set; }
    Func<ILinqToKqlProvider, Exception, Task<bool>>? ShouldRetry { get; set; }
    ILinqToKqlProviderExecutor ProviderExecutor { get; set; }
    Func<ILinqToKqlProvider, string, string>? PreExecute { get; set; }

    LinqToKqlProvider<S> Clone<S>(Expression? expression = null, bool cloneExpressionOnNull = true);
    string TranslateToKQL(Expression? expression = null);
}
