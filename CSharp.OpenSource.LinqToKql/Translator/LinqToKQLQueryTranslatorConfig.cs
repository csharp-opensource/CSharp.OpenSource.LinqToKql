namespace CSharp.OpenSource.LinqToKql.Translator;

public class LinqToKQLQueryTranslatorConfig
{
    public string PipeWithIndentation { get; set; } = "\n  |  ";
    public bool DisableNestedProjection { get; set; }
}