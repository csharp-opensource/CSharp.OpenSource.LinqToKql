using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Provider;

public interface IKustoDbContext : ILinqToKqlProvider
{
    LinqToKQLQueryTranslatorConfig Config { get; }

    LinqToKqlProvider<T> CreateQuery<T>(string tableOrKQL, string? database = null);
}