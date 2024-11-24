public interface ILinqToKqlProvider<T> : IQueryable<T>, IQueryProvider, IOrderedQueryable<T>, IAsyncEnumerable<T>
{
}
