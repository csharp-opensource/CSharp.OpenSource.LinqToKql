namespace CSharp.OpenSource.LinqToKql.Models;

public class KustoQueryResultV1 : IKustoQueryResult
{
    public List<KustoQueryResultV1Table> Tables { get; set; }

    public List<Dictionary<string, object?>> ToDictonaryList()
    {
        var resultList = new List<Dictionary<string, object?>>();

        if (Tables == null || Tables.Count == 0)
        {
            return resultList;
        }

        // only the first table is the results, other tables are metadata
        var table = Tables[0];
        var columns = table.Columns.Select(c => c.ColumnName).ToList();
        var rows = table.Rows;

        foreach (var row in rows)
        {
            var rowDict = new Dictionary<string, object?>();
            for (var i = 0; i < columns.Count; i++)
            {
                rowDict[columns[i]] = row[i];
            }
            resultList.Add(rowDict);
        }

        return resultList;
    }
}
