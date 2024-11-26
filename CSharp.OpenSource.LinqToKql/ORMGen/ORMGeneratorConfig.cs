using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorConfig
{
    public ORMGeneratorFilterConfig Filters { get; set; } = new();
    public string Namespace { get; set; }
    public List<ORMGeneratorDatabaseConfig> DatabaseConfigs { get; set; }
    public bool CreateDbContext { get; set; } = true;
    public bool CleanModelsFolderBeforeCreate { get; set; }
    public string ModelsFolderPath { get; set; }
    public string ModelsNamespace { get; set; }
    public string DbContextName { get; set; }
    public string DbContextFolderPath { get; set; }
    public string DbContextNamespace { get; set; }
    public ILinqToKqlProviderExecutor ProviderExecutor { get; set; }
}
