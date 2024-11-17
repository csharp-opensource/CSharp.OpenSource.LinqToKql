using CSharp.OpenSource.LinqToKql.Translator;
using System.Collections;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public class LinqToKqlProvider<T> : IQueryable<T>, IQueryProvider
{
    protected readonly LinqToKQLQueryTranslator Translator = new();
    protected readonly string TableName;
    public Type ElementType => typeof(T);
    private readonly Expression _expression;
    public Expression Expression => _expression;
    public IQueryProvider Provider => this;
    protected ILinqToKqlProviderExecutor? ProviderExecutor;

    public LinqToKqlProvider(string tableName, Expression? expression, ILinqToKqlProviderExecutor? providerExecutor = null)
    {
        TableName = tableName;
        _expression = expression ?? Expression.Constant(this);
        ProviderExecutor = providerExecutor;
    }

    public virtual object? Execute(Expression expression)
        => Execute<object>(expression);

    public virtual TResult Execute<TResult>(Expression expression)
    {
        if (ProviderExecutor == null) { throw new InvalidOperationException("ProviderExecutor is not set."); }
        var kql = Translator.Translate(expression, TableName);
        return ProviderExecutor.ExecuteAsync<TResult>(kql).GetAwaiter().GetResult();
    }

    public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => Clone<TElement>(expression);

    protected virtual LinqToKqlProvider<S> Clone<S>(Expression expression)
        => new LinqToKqlProvider<S>(TableName, expression, ProviderExecutor);

    public virtual IQueryable CreateQuery(Expression expression) => Provider.CreateQuery<T>(expression);
    public virtual IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerator<T>>(Expression);
    IEnumerator IEnumerable.GetEnumerator() => Provider.Execute<IEnumerator<T>>(Expression);
}
