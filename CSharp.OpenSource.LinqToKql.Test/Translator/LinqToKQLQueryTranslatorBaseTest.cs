using CSharp.OpenSource.LinqToKql.Test.Model;
using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Tests;

public abstract class LinqToKQLQueryTranslatorBaseTest
{
    protected LinqToKQLQueryTranslator GetTranslator() => new();
    protected readonly string _tableName = "myTable";
    protected readonly IQueryable<SampleObject> _q = new[] { new SampleObject { } }.AsQueryable();

    protected void AssertQuery<T>(IQueryable<T> queryable, string[] expectedArray)
    {
        var translator = GetTranslator();
        var expected = string.Join(
            translator.PipeWithIndentation,
            expectedArray
        );
        // Act
        var actual = GetTranslator().Translate(queryable, _tableName);
        // Assert
        Assert.Equal(expected, actual);
    }
}