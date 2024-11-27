namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorColumnModification
{
    public List<string> DatabasePatterns { get; set; }
    public List<string> TableOrFunctionPatterns { get; set; }
    public List<string> ColumnNamePatterns { get; set; }
    public string NewColumnType { get; set; }
    public string ColumnAttributes { get; set; }
    public bool Exclude { get; set; }
}