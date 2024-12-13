using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp.OpenSource.LinqToKql.Test;

public class EndToEndTests
{
    private string _cluster = "https://myclusterUrl/";
    private string _auth = "myBearerToken";
    private string _defaultDbName = "myDatabaseName";

    [Fact]
    public async Task VerifyKqlValidityOnServer()
    {
        var e2eTesting = Environment.GetEnvironmentVariable("E2E_TESTING");
        if (e2eTesting == "1")
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKustoDbContext<MyDbContext, ILinqToKqlProviderExecutor>(sp => new KustoHttpClient(_cluster, _auth, _defaultDbName));
            var sp = serviceCollection.BuildServiceProvider();
            var dbContext = sp.GetRequiredService<MyDbContext>();

            var kql = "SampleKqlQuery";
            var result = await dbContext.ExecuteKqlAsync(kql);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }
    }
}

public class MyDbContext : KustoDbContext
{
    public MyDbContext(IKustoDbContextExecutor<MyDbContext> executor) : base(executor)
    {
    }

    public async Task<KqlResult> ExecuteKqlAsync(string kql)
    {
        var providerExecutor = (KustoHttpClient)ProviderExecutor;
        var result = await providerExecutor.QueryAsync<KqlResult>(kql);
        return result;
    }
}

public class KqlResult
{
    public bool IsSuccess { get; set; }
}
