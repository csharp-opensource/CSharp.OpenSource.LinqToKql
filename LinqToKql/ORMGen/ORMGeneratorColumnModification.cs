namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorColumnModification
{
    public List<string> DatabasePatterns { get; set; } = new();
    public List<string> TableOrFunctionPatterns { get; set; } = new();
    public List<string> ColumnNamePatterns { get; set; } = new();
    public string NewColumnType { get; set; }
    public string ColumnAttributes { get; set; }
    public bool Exclude { get; set; }
}