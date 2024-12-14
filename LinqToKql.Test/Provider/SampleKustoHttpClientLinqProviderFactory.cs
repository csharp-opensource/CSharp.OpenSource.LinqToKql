using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public class SampleKustoHttpClientLinqProviderFactory : KustoHttpClientLinqProviderFactory
{
    private string _cluster = "https://myclusterUrl/";
    private string _auth = "myBearerToken";
    private string _defaultDbName = "myDatabaseName";

    public IQueryable<SampleObject> SampleObject
        => CreateQuery<SampleObject>(_cluster, _auth, _defaultDbName, "SampleObjectTable");

    public IQueryable<SampleObject> SampleObjectFunction(long number)
        => CreateQuery<SampleObject>(_cluster, _auth, _defaultDbName, $"SampleObjectFunction({number})");

    public IQueryable<SampleObject2> SampleObject2(long number)
        => CreateQuery<SampleObject2>(_cluster, _auth, _defaultDbName, $"SampleObject2Table");
}
