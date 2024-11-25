using System.Text.RegularExpressions;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGeneratorFilter
{
    /// <summary>
    /// Pattern can include wildcards with '*'
    /// </summary>
    public string Pattern { get; set; }
    public bool Exclude { get; set; }

    public bool Match(string value)
    {
        // Escape the pattern and replace '*' and '?' with regex equivalents
        string regexPattern = "^" + Regex.Escape(Pattern)
                                     .Replace("\\*", ".*")
                                     .Replace("\\?", ".") + "$";
        return Regex.IsMatch(value, regexPattern);
    }
}