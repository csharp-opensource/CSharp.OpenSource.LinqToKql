namespace CSharp.OpenSource.LinqToKql.Provider;

public interface ILinqToKqlProviderExecutor
{
    Task<T?> ExecuteAsync<T>(string kql, string? database = null);
}
