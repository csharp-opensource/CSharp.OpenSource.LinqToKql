namespace CSharp.OpenSource.LinqToKql.Provider;

public class KustoDbContextExecutor : IKustoDbContextExecutor
{
    public ILinqToKqlProviderExecutor Executor { get; set; }
    public KustoDbContextExecutor(ILinqToKqlProviderExecutor executor)
    {
        Executor = executor;
    }
}

public class KustoDbContextExecutor<T> : KustoDbContextExecutor, IKustoDbContextExecutor<T> where T : IKustoDbContext
{
    public KustoDbContextExecutor(ILinqToKqlProviderExecutor executor) : base(executor)
    {
    }
}
