using CSharp.OpenSource.LinqToKql.Models;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorTable
{
    public string Name { get; set; }
    public List<ShowSchemaResult> Columns { get; set; }
}