namespace CSharp.OpenSource.LinqToKql.Provider;

public interface IKustoDbContextExecutor
{
    ILinqToKqlProviderExecutor Executor { get; set; }
}

public interface IKustoDbContextExecutor<T> : IKustoDbContextExecutor where T : IKustoDbContext
{
    ILinqToKqlProviderExecutor Executor { get; set; }
}