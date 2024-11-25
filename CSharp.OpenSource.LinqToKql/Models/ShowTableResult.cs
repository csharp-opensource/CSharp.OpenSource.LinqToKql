namespace CSharp.OpenSource.LinqToKql.Models;

public class ShowTableResult
{
    public string DatabaseName { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string ColumnType { get; set; }
    public sbyte IsDefaultTable { get; set; }
    public sbyte IsDefaultColumn { get; set; }
    public string PrettyName { get; set; }
    public string Version { get; set; }
    public string Folder { get; set; }
    public string DocString { get; set; }
}
