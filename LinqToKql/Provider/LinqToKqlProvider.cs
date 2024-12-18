using CSharp.OpenSource.LinqToKql.Translator;
using System.Collections;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public class LinqToKqlProvider<T> : ILinqToKqlProvider<T>
{
    public LinqToKQLQueryTranslator Translator { get; }
    public string TableOrKQL { get; set; }
    public string? DefaultDbName { get; set; }
    public Type ElementType => typeof(T);
    private readonly Expression _expression;
    public Expression Expression => _expression;
    public IQueryProvider Provider => this;
    public ILinqToKqlProviderExecutor ProviderExecutor { get; }
    public Func<ILinqToKqlProvider, Exception, Task<bool>>? ShouldRetry { get; set; }

    public LinqToKqlProvider(
        string tableOrKQL,
        Expression? expression,
        ILinqToKqlProviderExecutor providerExecutor,
        LinqToKQLQueryTranslatorConfig? config = null,
        string? defaultDbName = null,
        Func<ILinqToKqlProvider, Exception, Task<bool>>? shouldRetry = null)
    {
        TableOrKQL = tableOrKQL;
        _expression = expression ?? Expression.Constant(this);
        ProviderExecutor = providerExecutor;
        config ??= new();
        Translator = new(config);
        DefaultDbName = defaultDbName;
        ShouldRetry = shouldRetry;
    }

    public virtual object? Execute(Expression expression)
        => Execute<object>(expression);

    public virtual TResult Execute<TResult>(Expression expression)
        => ExecuteAsync<TResult>(expression).GetAwaiter().GetResult();

    public virtual async Task<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        if (ProviderExecutor == null) { throw new InvalidOperationException("ProviderExecutor is not set."); }
        var kql = TranslateToKQL(expression);
        try
        {
            return await ProviderExecutor.ExecuteAsync<TResult>(kql, DefaultDbName);
        }
        catch (Exception ex) 
        {
            var shouldRetry = ShouldRetry == null ? false : await ShouldRetry(this, ex);
            if (shouldRetry) 
            {
                return await ProviderExecutor.ExecuteAsync<TResult>(kql, DefaultDbName);
            }
            throw;
        }
    }

    public virtual string TranslateToKQL(Expression? expression = null)
        => Translator.Translate(expression ?? Expression, TableOrKQL);

    public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => Clone<TElement>(expression);

    public virtual LinqToKqlProvider<S> Clone<S>(Expression? expression = null, bool cloneExpressionOnNull = true)
    {
        var kql = TableOrKQL;
        expression ??= cloneExpressionOnNull ? Expression : null;
        if (typeof(T) != typeof(S))
        {
            expression = null;
            kql = TranslateToKQL();
        }
        return new(
            kql,
            expression,
            ProviderExecutor,
            Translator.Config,
            DefaultDbName,
            ShouldRetry
        );
    }

    public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var results = await ExecuteAsync<List<T>>(Expression);
        foreach (var result in results)
        {
            yield return result;
        }
    }

    public virtual IQueryable CreateQuery(Expression expression) => Provider.CreateQuery<T>(expression);
    protected virtual IEnumerator<T> GetGenericEnumerator() => Provider.Execute<List<T>>(Expression).GetEnumerator();
    public virtual IEnumerator<T> GetEnumerator() => GetGenericEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetGenericEnumerator();
    public object Clone() => Clone<T>();
}
