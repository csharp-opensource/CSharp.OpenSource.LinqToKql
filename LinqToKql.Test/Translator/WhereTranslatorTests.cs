using Microsoft.EntityFrameworkCore;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class WhereTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
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
            [_tableName, "where Name in ('name1', 'name2', 'name3')"]
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
    public void Translate_WhereListIn()
        => AssertQuery(
            _q.Where(x => x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Numbers in (1)", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereListNotIn()
        => AssertQuery(
            _q.Where(x => !x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where !(Numbers in (1))", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringIn()
        => AssertQuery(
            _q.Where(x => x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringNotIn()
        => AssertQuery(
            _q.Where(x => !x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where !(Name has_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringStartsWith()
        => AssertQuery(
            _q.Where(x => x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringNotStartsWith()
        => AssertQuery(
            _q.Where(x => !x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where !(Name startswith_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringEndsWith()
        => AssertQuery(
            _q.Where(x => x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringNotEndsWith()
        => AssertQuery(
            _q.Where(x => !x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where !(Name endswith_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringLikeStartsWith()
        => AssertQuery(
            _q.Where(x => EF.Functions.Like(x.Name, "%na")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringLikeEndsWith()
        => AssertQuery(
            _q.Where(x => EF.Functions.Like(x.Name, "na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public void Translate_WhereStringLikeHas() 
        => AssertQuery(
            _q.Where(x => EF.Functions.Like(x.Name, "%na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'na'", "project Date, Description"]
        );
}