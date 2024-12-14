using CSharp.OpenSource.LinqToKql.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp.OpenSource.LinqToKql.Http;

public class KustoHttpClient : IKustoHttpClient
{
    protected readonly HttpClient _httpClient = new();
    protected string ClusterUrl { get; set; }
    protected string AuthBearerValue { get; set; }
    protected string DefaultDbName { get; set; }

    public KustoHttpClient(string cluster, string auth, string defaultDbName)
    {
        ClusterUrl = cluster;
        AuthBearerValue = auth;
        DefaultDbName = defaultDbName;
    }

    public virtual Task<T> ExecuteAsync<T>(string kql, string? database = null) => QueryAsync<T>(kql, database: database);

    public virtual async Task<IKustoQueryResult?> QueryAsync(string csl, string apiVersion = "v2", string? database = null)
    {
        if (string.IsNullOrEmpty(ClusterUrl)) { throw new InvalidOperationException($"{nameof(ClusterUrl)} must be set before querying."); }
        var requestUri = $"{ClusterUrl}/{apiVersion}/rest/query";
        if (csl.Trim().StartsWith("."))
        {
            apiVersion = "v1";
            requestUri = $"{ClusterUrl}/v1/rest/mgmt";
        }
        var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { db = database ?? DefaultDbName, csl }), Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrEmpty(AuthBearerValue))
        {
            req.Headers.Authorization = new("Bearer", AuthBearerValue);
        }
        var response = await _httpClient.SendAsync(req);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }
        IKustoQueryResult? res = apiVersion == "v1"
            ? await response.Content.ReadFromJsonAsync<KustoQueryResultV1>()
            : await response.Content.ReadFromJsonAsync<KustoQueryResultV2>();
        return res;
    }

    public virtual async Task<T> QueryAsync<T>(string csl, string apiVersion = "v2", string? database = null)
    {
        var res = await QueryAsync(csl, apiVersion, database);
        if (res == null)
        {
            throw new InvalidOperationException("Failed to deserialize Kusto query result.");
        }
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(res.ToDictonaryList()))!;
    }
}
