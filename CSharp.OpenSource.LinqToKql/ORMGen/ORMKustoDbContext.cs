using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMKustoDbContext : KustoDbContext
{
    public ORMKustoDbContext(IKustoDbContextExecutor executor) : base(executor)
    {
    }

    public virtual string GetDatabaseName(string dbNameFromORMGen) => dbNameFromORMGen;
}
