using CSharp.OpenSource.LinqToKql.ORMGen;

namespace CSharp.OpenSource.LinqToKql.Models;

public class ShowFunctionsResult
{
    public string Name { get; set; }
    public string Parameters { get; set; }
    public string Body { get; set; }
    public string Folder { get; set; }
    public string DocString { get; set; }
    public List<ORMGeneratorFunctionParam> ParametersItems { get; internal set; }
}