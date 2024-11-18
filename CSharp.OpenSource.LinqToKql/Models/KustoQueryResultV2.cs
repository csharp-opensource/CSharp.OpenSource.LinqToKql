namespace CSharp.OpenSource.LinqToKql.Models;

public class KustoQueryResultV2 : List<KustoQueryResultV2Item>, IKustoQueryResult
{
    public List<Dictionary<string, object?>> ToDictonaryList()
    {
        var resultList = new List<Dictionary<string, object?>>();
        if (this == null || Count == 0)
        {
            return resultList;
        }
        var resultTable = this.FirstOrDefault(x => x.TableName == "PrimaryResult");
        if (resultTable == null)
        {
            return resultList;
        }

        // only the first table is the results, other tables are metadata
        var columns = resultTable.Columns.Select(c => c.ColumnName).ToList();
        var rows = resultTable.Rows;

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
