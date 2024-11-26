namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorFilterConfig
{
    public List<ORMGeneratorFilter> TableFilters { get; set; } = new();
    public List<ORMGeneratorFilter> FunctionFilters { get; set; } = new();
    public List<ORMGeneratorFilter> GlobalFilters { get; set; } = new();
}