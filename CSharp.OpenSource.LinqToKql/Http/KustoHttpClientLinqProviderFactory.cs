using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.Http;

public abstract class KustoHttpClientLinqProviderFactory
{
    public LinqToKqlProvider<T> CreateQuery<T>(string cluster, string auth, string db, string tableName)
        => new LinqToKqlProvider<T>(tableName, expression: null, providerExecutor: new KustoHttpClient(cluster, auth, db));
}
