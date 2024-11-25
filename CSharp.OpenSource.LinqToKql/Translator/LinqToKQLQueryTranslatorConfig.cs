namespace CSharp.OpenSource.LinqToKql.Translator;

public class LinqToKQLQueryTranslatorConfig
{
    public string PipeWithIndentation { get; set; } = "\n  |  ";
    /// <summary>
    /// if not disable, translator will use extend and can cause performance issues
    /// </summary>
    public bool DisableNestedProjection { get; set; }
}