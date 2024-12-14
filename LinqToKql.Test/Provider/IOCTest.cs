using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public class IOCTest
{
    private string _cluster = "https://myclusterUrl/";
    private string _auth = "myBearerToken";
    private string _defaultDbName = "myDatabaseName";

    [Fact]
    public void InjectionTestWithInterface()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKustoDbContext<IMyMultiDbContext, MyMultiDbContext, ILinqToKqlProviderExecutor>(sp => new KustoHttpClient(_cluster, _auth, _defaultDbName));

        var sp = serviceCollection.BuildServiceProvider();
        sp.GetRequiredService<MyMultiDbContext>();
        sp.GetRequiredService<IMyMultiDbContext>();
    }

    [Fact]
    public void InjectionTestDiffContextInjections()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKustoDbContext<MyMultiDbContext, ILinqToKqlProviderExecutor>(sp => new KustoHttpClient(_cluster, _auth, _defaultDbName));
        serviceCollection.AddKustoDbContext<MyMultiDbContext2, ILinqToKqlProviderExecutor>(sp => new KustoHttpClient(_cluster, _auth, _defaultDbName));

        var sp = serviceCollection.BuildServiceProvider();
        var svc1 = sp.GetService<MyMultiDbContext>();
        var svc2 = sp.GetService<MyMultiDbContext2>();
    }
}
