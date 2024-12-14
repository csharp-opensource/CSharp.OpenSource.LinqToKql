using CSharp.OpenSource.LinqToKql.Translator;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Provider;

public abstract class KustoDbContext : IKustoDbContext
{
    private IKustoDbContextExecutor _executor;
    public ILinqToKqlProviderExecutor ProviderExecutor => _executor.Executor;
    private LinqToKQLQueryTranslatorConfig? _config;
    public LinqToKQLQueryTranslatorConfig Config => _config ??= GetConfig();
    private LinqToKqlProvider<object> dummyProvider => new(string.Empty, expression: null, providerExecutor: ProviderExecutor, defaultDbName: DefaultDbName, config: GetConfig());

    public string? DefaultDbName { get; set; }
    public string TableOrKQL { get => ""; set => _ = value; }

    public LinqToKQLQueryTranslator Translator => throw new NotImplementedException();

    public Func<ILinqToKqlProvider, Exception, Task<bool>>? ShouldRetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected virtual LinqToKQLQueryTranslatorConfig GetConfig() => new();

    public KustoDbContext(IKustoDbContextExecutor executor)
    {
        _executor = executor;
    }

    public LinqToKqlProvider<T> CreateQuery<T>(string tableOrKQL, string? database = null)
        => new LinqToKqlProvider<T>(tableOrKQL, expression: null, providerExecutor: ProviderExecutor, defaultDbName: database ?? DefaultDbName, config: GetConfig());

    public LinqToKqlProvider<S> Clone<S>(Expression? expression = null) 
        => dummyProvider.Clone<S>(expression);

    public string TranslateToKQL(Expression? expression = null)
        => dummyProvider.TranslateToKQL(expression);
}
