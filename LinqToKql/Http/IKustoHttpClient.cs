using CSharp.OpenSource.LinqToKql.Models;
using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.Http;

public interface IKustoHttpClient : ILinqToKqlProviderExecutor
{
    Task<IKustoQueryResult?> QueryAsync(string csl, string apiVersion = "v2");
    Task<T> QueryAsync<T>(string csl, string apiVersion = "v2");
}