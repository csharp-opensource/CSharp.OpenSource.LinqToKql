﻿using CSharp.OpenSource.LinqToKql.Translator;
using System.Collections;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public class LinqToKqlProvider<T> : ILinqToKqlProvider<T>
{
    protected readonly LinqToKQLQueryTranslator Translator;
    public string TableOrKQL { get; set; }
    public string? DefaultDbName { get; set; }
    public Type ElementType => typeof(T);
    private readonly Expression _expression;
    public Expression Expression => _expression;
    public IQueryProvider Provider => this;
    protected ILinqToKqlProviderExecutor ProviderExecutor;

    public LinqToKqlProvider(
        string tableOrKQL,
        Expression? expression,
        ILinqToKqlProviderExecutor providerExecutor,
        LinqToKQLQueryTranslatorConfig? config = null,
        string? defaultDbName = null)
    {
        TableOrKQL = tableOrKQL;
        _expression = expression ?? Expression.Constant(this);
        ProviderExecutor = providerExecutor;
        config ??= new();
        Translator = new(config);
        DefaultDbName = defaultDbName;
    }

    public virtual object? Execute(Expression expression)
        => Execute<object>(expression);

    public virtual TResult Execute<TResult>(Expression expression)
        => ExecuteAsync<TResult>(expression).GetAwaiter().GetResult();

    public virtual Task<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        if (ProviderExecutor == null) { throw new InvalidOperationException("ProviderExecutor is not set."); }
        var kql = Translator.Translate(expression, TableOrKQL);
        return ProviderExecutor.ExecuteAsync<TResult>(kql, DefaultDbName);
    }

    public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => Clone<TElement>(expression);

    protected virtual LinqToKqlProvider<S> Clone<S>(Expression expression)
        => new LinqToKqlProvider<S>(TableOrKQL, expression, ProviderExecutor, Translator.Config, DefaultDbName);

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
}
