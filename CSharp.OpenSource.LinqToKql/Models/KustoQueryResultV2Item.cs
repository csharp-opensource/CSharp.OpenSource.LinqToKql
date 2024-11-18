namespace CSharp.OpenSource.LinqToKql;

public class KustoQueryResultV2Item
{
    public string FrameType { get; set; }
    public bool? IsProgressive { get; set; }
    public string Version { get; set; }
    public bool? IsFragmented { get; set; }
    public string ErrorReportingPlacement { get; set; }
    public int? TableId { get; set; }
    public string TableKind { get; set; }
    public string TableName { get; set; }
    public List<KustoQueryResultV2Column> Columns { get; set; }
    public List<List<object>> Rows { get; set; }
    public bool? HasErrors { get; set; }
    public bool? Cancelled { get; set; }
}
