namespace CSharp.OpenSource.LinqToKql.Models;

public interface IKustoQueryResult
{
    List<Dictionary<string, object?>> ToDictonaryList();
}
