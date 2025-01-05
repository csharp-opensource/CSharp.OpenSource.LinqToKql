using CSharp.OpenSource.LinqToKql.Http;
using System.Diagnostics;

namespace CSharp.OpenSource.LinqToKql.Test;

public static class E2EHelper
{
    public static KustoHttpClient Client;
    static E2EHelper()
    {
        Client = new("http://localhost:8080", "", "TestDatabase1");
        Client.HttpClient.Timeout = TimeSpan.FromSeconds(3);
    }

    public static bool IsE2E => Environment.GetEnvironmentVariable("E2E_TESTING") == "1" || Debugger.IsAttached;
}
