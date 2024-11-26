namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorDatabaseConfig
{
    public string DatabaseName { get; set; }
    public ORMGeneratorFilterConfig Filters { get; set; } = new();
    public string ModelSubFolderName { get; set; }
}