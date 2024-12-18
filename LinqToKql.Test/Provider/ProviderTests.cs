using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Provider;
using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public class ProviderTests
{
    [Fact]
    public void CloneTest()
    {
        var q = new LinqToKqlProvider<SampleObject>("test", expression: null, providerExecutor: null);
        var q1 = q.Where(x => x.Year == 8 && x.Description == "desc")
                   .AsKQL()
                   .Clone<Test1>();
        var q2 = q1.Where(x => x.Test == "1")
                   .AsKQL();
        var kql = q2.TranslateToKQL();
        var expected = string.Join(q1.Translator.PipeWithIndentation, ["test", "where Year == 8 and Description == 'desc'", "where Test == '1'"]);
        Assert.Equal(expected, kql);
    }

    [Fact]
    public void FromKQLTest()
    {
        var q = new LinqToKqlProvider<SampleObject>("test", expression: null, providerExecutor: null);
        var q1 = q.Where(x => x.Year == 8 && x.Description == "desc")
                  .FromKQL("| where 1 == 1", appendKQL: true)
                  .FromKQL("where 2 == 2", appendKQL: true);
        var q2 = q1.FromKQL("newTable", appendKQL: false);

        var expected1 = string.Join(q1.Translator.PipeWithIndentation, ["test", "where Year == 8 and Description == 'desc'", "where 1 == 1", "where 2 == 2"]);
        var expected2 = "newTable";
        var actual1 = q1.TranslateToKQL();
        var actual2 = q2.TranslateToKQL();
        Assert.Equal(expected1, actual1);
        Assert.Equal(expected2, actual2);
    }
}

public interface Test1
{
    int Year { get; }
    string Test { get; }
}