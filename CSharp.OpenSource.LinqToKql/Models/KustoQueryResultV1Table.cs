namespace CSharp.OpenSource.LinqToKql;

public class KustoQueryResultV1Table
{
    public string TableName { get; set; }
    public List<List<object?>> Rows { get; set; }
    public List<KustoQueryResultV1TableColumn> Columns { get; set; }
}
