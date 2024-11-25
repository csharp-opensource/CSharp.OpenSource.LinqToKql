namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorFilterConfig
{
    public List<ORMGeneratorFilter> TableFilters { get; set; }
    public List<ORMGeneratorFilter> FunctionFilters { get; set; }
    public List<ORMGeneratorFilter> GlobalFilters { get; set; }
}