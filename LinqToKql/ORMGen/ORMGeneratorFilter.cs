namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorFilter
{
    /// <summary>
    /// Pattern can include wildcards with '*'
    /// </summary>
    public string Pattern { get; set; }
    public bool Exclude { get; set; }
}