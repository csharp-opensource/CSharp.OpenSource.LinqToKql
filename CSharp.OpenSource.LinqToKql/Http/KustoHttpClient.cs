﻿using CSharp.OpenSource.LinqToKql.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CSharp.OpenSource.LinqToKql.Http;

public class KustoHttpClient : IKustoHttpClient
{
    protected readonly HttpClient _httpClient = new();
    protected string ClusterUrl { get; set; }
    protected string AuthBearerValue { get; set; }
    protected string Database { get; set; }

    public KustoHttpClient(string cluster, string auth, string db)
    {
        ClusterUrl = cluster;
        AuthBearerValue = auth;
        Database = db;
    }

    public virtual Task<T> ExecuteAsync<T>(string kql) => QueryAsync<T>(Database, kql);

    public virtual async Task<IKustoQueryResult?> QueryAsync(string csl, string apiVersion = "v2")
    {
        if (string.IsNullOrEmpty(ClusterUrl)) { throw new InvalidOperationException($"{nameof(ClusterUrl)} must be set before querying."); }
        if (string.IsNullOrEmpty(AuthBearerValue)) { throw new InvalidOperationException($"{nameof(AuthBearerValue)} must be set before querying."); }
        var requestUri = $"{ClusterUrl}/{apiVersion}/rest/query";
        var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { db = Database, csl }), Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new("Bearer", AuthBearerValue);
        var response = await _httpClient.SendAsync(req);
        response.EnsureSuccessStatusCode();
        IKustoQueryResult? res = apiVersion == "v1"
            ? await response.Content.ReadFromJsonAsync<KustoQueryResultV1>()
            : await response.Content.ReadFromJsonAsync<KustoQueryResultV2>();
        return res;
    }

    public virtual async Task<T> QueryAsync<T>(string csl, string apiVersion = "v2")
    {
        var res = await QueryAsync(csl, apiVersion);
        if (res == null)
        {
            throw new InvalidOperationException("Failed to deserialize Kusto query result.");
        }
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(res.ToDictonaryList()))!;
    }
}
