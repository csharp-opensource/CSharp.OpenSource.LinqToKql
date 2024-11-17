using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Tests;

public class LinqToKQLQueryTranslatorTests
{
    private LinqToKQLQueryTranslator GetTranslator() => new();
    private readonly string _tableName = "myTable";
    private readonly IQueryable<SampleObject> _q = new[] { new SampleObject { } }.AsQueryable();

    private void AssertQuery<T>(IQueryable<T> queryable, string[] expectedArray)
    {
        var translator = GetTranslator();
        var expected = string.Join(
            translator.PipeWithIndentation,
            expectedArray
        );
        // Act
        var result = GetTranslator().Translate(queryable, _tableName);
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Translate_ShouldHandleWhereAndSelect() 
        => AssertQuery(
            _q.Where(x => x.Id > 1).Select(x => new { x.Date, x.Description }),
            ["myTable", "where Id > 1", "project Date, Description"]
        );

    [Fact]
    public void Translate_ShouldHandleMultipleWhereClauses()
        => AssertQuery(
            _q.Where(x => x.Date > DateTime.Now).Select(x => new { x.Date, x.Description }),
            ["myTable", "where Id > 1", "where Date > now()"]
        );

    [Fact]
    public void Translate_ShouldHandleOrderBy()
        => AssertQuery(
            _q.OrderBy(x => x.Date),
            ["myTable", "sort by Date asc"]
        );

    [Fact]
    public void Translate_ShouldHandleOrderByDesc()
        => AssertQuery(
            _q.OrderByDescending(x => x.Date),
            ["myTable", "sort by Date desc"]
        );

    [Fact]
    public void Translate_ShouldHandleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new { Date = g.Key, Count = g.Count() }),
            ["myTable", "summarize Count=count() by Date"]
        );
}