using Microsoft.EntityFrameworkCore;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class WhereTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleWhereAndSelect()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1).Select(x => new { x.Date, x.Description }),
            [_tableName, "where Id > 1", "project Date, Description"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithOr()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1 || x.Id == 3),
            [_tableName, "where Id > 1 or Id == 3"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWitContains()
        => AssertQueryAsync(
            _q.Where(x => new string[] { "name1", "name2", "name3" }.Contains(x.Name)),
            [_tableName, "where Name in ('name1', 'name2', 'name3')"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithAnd()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1 && x.Id == 3),
            [_tableName, "where Id > 1 and Id == 3"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithNot()
        => AssertQueryAsync(
            _q.Where(x => !(x.Id > 1 && x.Id == 8)),
            [_tableName, "where not(Id > 1 and Id == 8)"]
        );

    [Fact]
    public Task Translate_WhereDateTime()
    {
        AssertQueryAsync(
            _q.Where(x => x.Date > new DateTime(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({new DateTime(1999, 1, 1):yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
        var filter = new DateTime(2000, 1, 1);
        AssertQueryAsync(
            _q.Where(x => x.Date > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({filter:yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public Task Translate_WhereDateOnly()
    {
        var filter = new DateOnly(2000, 1, 1);
        AssertQueryAsync(
            _q.Where(x => x.DateOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({filter:yyyy-MM-dd})", "project Date, Description"]
        );
        AssertQueryAsync(
            _q.Where(x => x.DateOnly > new DateOnly(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({new DateOnly(1999, 1, 1):yyyy-MM-dd})", "project Date, Description"]
        );
    }

    [Fact]
    public Task Translate_WhereTimeSpan()
    {
        AssertQueryAsync(
            _q.Where(x => x.Time > new TimeSpan(1, 22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(1.22:01:01)", "project Date, Description"]
        );
        var filter = new TimeSpan(2, 23, 1, 1);
        AssertQueryAsync(
            _q.Where(x => x.Time > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(2.23:01:01)", "project Date, Description"]
        );
    }

    [Fact]
    public Task Translate_WhereTimeOnly()
    {
        var filter = new TimeOnly(23, 1, 1);
        AssertQueryAsync(
            _q.Where(x => x.TimeOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({filter:HH:mm:ss.f})", "project Date, Description"]
        );
        AssertQueryAsync(
            _q.Where(x => x.TimeOnly > new TimeOnly(22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({new TimeOnly(22, 1, 1):HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public Task Translate_WhereListIn()
        => AssertQueryAsync(
            _q.Where(x => x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Numbers in (1)", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereListNotIn()
        => AssertQueryAsync(
            _q.Where(x => !x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Numbers in (1))", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringIn()
        => AssertQueryAsync(
            _q.Where(x => x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotIn()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name has_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringStartsWith()
        => AssertQueryAsync(
            _q.Where(x => x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotStartsWith()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name startswith_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringEndsWith()
        => AssertQueryAsync(
            _q.Where(x => x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotEndsWith()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name endswith_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringLikeStartsWith()
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "%na")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringLikeEndsWith()
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringLikeHas() 
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "%na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'na'", "project Date, Description"]
        );
}