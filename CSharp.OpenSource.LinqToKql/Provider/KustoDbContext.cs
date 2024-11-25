using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Provider;

public abstract class KustoDbContext : IKustoDbContext
{
    private IKustoDbContextExecutor _executor;
    public ILinqToKqlProviderExecutor ProviderExecutor => _executor.Executor;
    private LinqToKQLQueryTranslatorConfig? _config;
    public LinqToKQLQueryTranslatorConfig Config => _config ??= GetConfig();
    protected virtual LinqToKQLQueryTranslatorConfig GetConfig() => new();

    public KustoDbContext(IKustoDbContextExecutor executor)
    {
        _executor = executor;
    }

    public LinqToKqlProvider<T> CreateQuery<T>(string tableOrKQL, string? database = null)
        => new LinqToKqlProvider<T>(tableOrKQL, expression: null, providerExecutor: ProviderExecutor, defaultDbName: database);
}
