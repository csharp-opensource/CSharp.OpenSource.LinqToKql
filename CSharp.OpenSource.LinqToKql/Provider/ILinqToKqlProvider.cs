namespace CSharp.OpenSource.LinqToKql.Provider;

public interface ILinqToKqlProvider<T> : IQueryable<T>, IQueryProvider, IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    string? DefaultDbName { get; set; }
    string TableOrKQL { get; set; }
}
