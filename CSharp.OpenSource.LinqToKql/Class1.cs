using System.Collections;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql;

public abstract class LinqToKqlProvider<T> : IQueryable<T>, IQueryProvider
{
    protected readonly LinqToKQLQueryTranslator Translator = new();
    protected readonly string TableName;
    public Type ElementType => typeof(T);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => this;

    public LinqToKqlProvider(string tableName)
    {
        TableName = tableName;
    }

    protected abstract Task<TResult> ExecuteAsync<TResult>(string kql);

    public virtual object? Execute(Expression expression)
        => Execute<object>(expression);

    public virtual TResult Execute<TResult>(Expression expression)
        => ExecuteAsync<TResult>("").GetAwaiter().GetResult();

    public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        throw new NotImplementedException();
    }
    public virtual IQueryable CreateQuery(Expression expression) => Provider.CreateQuery<T>(expression);
    public virtual IEnumerator<T> GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Provider.Execute<IEnumerator<T>>(Expression);
}

