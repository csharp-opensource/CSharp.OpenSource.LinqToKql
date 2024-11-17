using CSharp.OpenSource.LinqToKql.Test.Model;
using CSharp.OpenSource.LinqToKql.Translator;

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
            [_tableName, "where Id > 1", "project Date, Description"]
        );

    [Fact]
    public void Translate_ShouldHandleWhereWithOr()
        => AssertQuery(
            _q.Where(x => x.Id > 1 || x.Id == 3),
            [_tableName, "where Id > 1 or Id == 3"]
        );

    [Fact]
    public void Translate_ShouldHandleWhereWitContains()
        => AssertQuery(
            _q.Where(x => new string[] { "name1", "name2", "name3" }.Contains(x.Name)),
            [_tableName, "where Name has ('name1', 'name2', 'name3')"]
        );

    [Fact]
    public void Translate_ShouldHandleWhereWithAnd()
        => AssertQuery(
            _q.Where(x => x.Id > 1 && x.Id == 3),
            [_tableName, "where Id > 1 and Id == 3"]
        );

    [Fact]
    public void Translate_ShouldHandleWhereWithNot()
        => AssertQuery(
            _q.Where(x => !(x.Id > 1 && x.Id == 8)),
            [_tableName, "where !(Id > 1 and Id == 8)"]
        );

    [Fact]
    public void Translate_WhereDateTime()
    {
        AssertQuery(
            _q.Where(x => x.Date > new DateTime(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({new DateTime(1999, 1, 1):yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
        var filter = new DateTime(2000, 1, 1);
        AssertQuery(
            _q.Where(x => x.Date > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({filter:yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public void Translate_WhereDateOnly()
    {
        var filter = new DateOnly(2000, 1, 1);
        AssertQuery(
            _q.Where(x => x.DateOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({filter:yyyy-MM-dd})", "project Date, Description"]
        );
        AssertQuery(
            _q.Where(x => x.DateOnly > new DateOnly(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({new DateOnly(1999, 1, 1):yyyy-MM-dd})", "project Date, Description"]
        );
    }

    [Fact]
    public void Translate_WhereTimeSpan()
    {
        AssertQuery(
            _q.Where(x => x.Time > new TimeSpan(1, 22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(1.22:01:01)", "project Date, Description"]
        );
        var filter = new TimeSpan(2, 23, 1, 1);
        AssertQuery(
            _q.Where(x => x.Time > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(2.23:01:01)", "project Date, Description"]
        );
    }

    [Fact]
    public void Translate_WhereTimeOnly()
    {
        var filter = new TimeOnly(23, 1, 1);
        AssertQuery(
            _q.Where(x => x.TimeOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({filter:HH:mm:ss.f})", "project Date, Description"]
        );
        AssertQuery(
            _q.Where(x => x.TimeOnly > new TimeOnly(22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({new TimeOnly(22, 1, 1):HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public void Translate_ShouldHandleOrderBy()
        => AssertQuery(
            _q.OrderBy(x => x.Date),
            [_tableName, "sort by Date asc"]
        );

    [Fact]
    public void Translate_ShouldHandleOrderByDesc()
        => AssertQuery(
            _q.OrderByDescending(x => x.Date),
            [_tableName, "sort by Date desc"]
        );

    [Fact]
    public void Translate_ShouldHandleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new { Date = g.Key, Count = g.Count() }),
            [_tableName, "summarize Count=count() by Date", "project Date, Count"]
        );

    [Fact]
    public void Translate_ShouldHandleSelectWithInit()
        => AssertQuery(
            _q.Select(x => new SampleObject2 { Name2 = x.Name, Id2 = x.Id }),
            [_tableName, "project Name2 = Name, Id2 = Id"]
        );

    [Fact]
    public void Translate_ShouldHandleTake()
        => AssertQuery(
            _q.Take(50),
            [_tableName, "take 50"]
        );
}