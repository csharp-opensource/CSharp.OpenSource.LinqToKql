﻿using CSharp.OpenSource.LinqToKql.Translator;
using System.Collections;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public class LinqToKqlProvider<T> : ILinqToKqlProvider<T>
{
    public LinqToKQLQueryTranslator Translator { get; set; }
    public string TableOrKQL { get; set; }
    public string? DefaultDbName { get; set; }
    public Type ElementType => typeof(T);
    private readonly Expression _expression;
    public Expression Expression => _expression;
    public IQueryProvider Provider => this;
    public ILinqToKqlProviderExecutor ProviderExecutor { get; set; }
    public Func<ILinqToKqlProvider, Exception, Task<bool>>? ShouldRetry { get; set; }
    public Func<ILinqToKqlProvider, string, string>? PreExecute { get; set; }

    public LinqToKqlProvider(
        string tableOrKQL,
        Expression? expression,
        ILinqToKqlProviderExecutor providerExecutor,
        LinqToKQLQueryTranslatorConfig? config = null,
        string? defaultDbName = null,
        Func<ILinqToKqlProvider, Exception, Task<bool>>? shouldRetry = null,
        Func<ILinqToKqlProvider, string, string>? preExecute = null)
    {
        TableOrKQL = tableOrKQL;
        _expression = expression ?? Expression.Constant(this);
        ProviderExecutor = providerExecutor;
        config ??= new();
        Translator = new(config);
        DefaultDbName = defaultDbName;
        ShouldRetry = shouldRetry;
        PreExecute = preExecute;
    }

    public virtual object? Execute(Expression expression)
        => Execute<object>(expression);

    public virtual TResult Execute<TResult>(Expression expression)
        => ExecuteAsync<TResult>(expression).GetAwaiter().GetResult()!;

    public virtual async Task<TResult?> ExecuteAsync<TResult>(Expression expression)
    {
        if (ProviderExecutor == null) { throw new InvalidOperationException("ProviderExecutor is not set."); }
        var kql = TranslateToKQL(expression);
        try
        {
            if (PreExecute != null)
            {
                kql = PreExecute(this, kql);
            }
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
        expression ??= cloneExpressionOnNull ? Expression : null;
        if (typeof(T) != typeof(S) && expression != null)
        {
            expression = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Cast),
                new[] { typeof(S) },
                expression
            );
        }
        var kql = TableOrKQL;
        return new(
            kql,
            expression,
            ProviderExecutor,
            Translator.Config,
            DefaultDbName,
            ShouldRetry,
            PreExecute
        );
    }

    public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var results = await ExecuteAsync<List<T>>(Expression);
        foreach (var result in results!)
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
