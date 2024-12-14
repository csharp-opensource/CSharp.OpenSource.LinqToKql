
namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorTableOrFunctionModification
{
    public List<string> DatabasePatterns { get; set; } = new();
    public List<string> TableOrFunctionPatterns { get; set; } = new();
    public string ClassAttributes { get; set; }
    public string ClassInherit { get; set; }
    public List<string> BodyExtraLines { get; set; } = new();
}