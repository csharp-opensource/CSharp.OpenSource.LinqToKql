using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Provider;

public interface IKustoDbContext
{
    LinqToKQLQueryTranslatorConfig Config { get; }
    ILinqToKqlProviderExecutor ProviderExecutor { get; }

    LinqToKqlProvider<T> CreateQuery<T>(string tableOrKQL, string? database = null);
}