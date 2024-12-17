using CSharp.OpenSource.LinqToKql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class WhereTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleWhereAndSelectAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1).Select(x => new { x.Date, x.Description }),
            [_tableName, "where Id > 1", "project Date, Description"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithOrAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1 || x.Id == 3),
            [_tableName, "where Id > 1 or Id == 3"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWitContainsAsync()
        => AssertQueryAsync(
            _q.Where(x => new string[] { "name1", "name2", "name3" }.Contains(x.Name)),
            [_tableName, "where Name in ('name1', 'name2', 'name3')"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithAndAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Id > 1 && x.Id == 3),
            [_tableName, "where Id > 1 and Id == 3"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithNotAsync()
        => AssertQueryAsync(
            _q.Where(x => !(x.Id > 1 && x.Id == 8)),
            [_tableName, "where not(Id > 1 and Id == 8)"]
        );

    [Fact]
    public async Task Translate_WhereDateTimeAsync()
    {
        await AssertQueryAsync(
            _q.Where(x => x.Date > new DateTime(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({new DateTime(1999, 1, 1):yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
        var filter = new DateTime(2000, 1, 1);
        await AssertQueryAsync(
            _q.Where(x => x.Date > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Date > datetime({filter:yyyy-MM-dd HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public async Task Translate_WhereDateOnlyAsync()
    {
        var filter = new DateOnly(2000, 1, 1);
        await AssertQueryAsync(
            _q.Where(x => x.DateOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({filter:yyyy-MM-dd})", "project Date, Description"]
        );
        await AssertQueryAsync(
            _q.Where(x => x.DateOnly > new DateOnly(1999, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where DateOnly > datetime({new DateOnly(1999, 1, 1):yyyy-MM-dd})", "project Date, Description"]
        );
    }

    [Fact]
    public async Task Translate_WhereTimeSpanAsync()
    {
        await AssertQueryAsync(
            _q.Where(x => x.Time > new TimeSpan(1, 22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(1.22:01:01)", "project Date, Description"]
        );
        var filter = new TimeSpan(2, 23, 1, 1);
        await AssertQueryAsync(
            _q.Where(x => x.Time > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Time > timespan(2.23:01:01)", "project Date, Description"]
        );
    }

    [Fact]
    public async Task Translate_WhereTimeOnlyAsync()
    {
        var filter = new TimeOnly(23, 1, 1);
        await AssertQueryAsync(
            _q.Where(x => x.TimeOnly > filter).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({filter:HH:mm:ss.f})", "project Date, Description"]
        );
        await AssertQueryAsync(
            _q.Where(x => x.TimeOnly > new TimeOnly(22, 1, 1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where TimeOnly > timespan({new TimeOnly(22, 1, 1):HH:mm:ss.f})", "project Date, Description"]
        );
    }

    [Fact]
    public Task Translate_WhereListInAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Numbers in (1)", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereListNotInAsync()
        => AssertQueryAsync(
            _q.Where(x => !x.Numbers.Contains(1)).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Numbers in (1))", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringInAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotInAsync()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.Contains("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name has_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringStartsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotStartsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.StartsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name startswith_cs 'c#')", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringEndsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'c#'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringNotEndsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => !x.Name.EndsWith("c#")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where not(Name endswith_cs 'c#')", "project Date, Description"]
        );

    [Theory]
    [InlineData("na", "==")]
    [InlineData("%na%", "has_cs")]
    [InlineData("%na", "startswith_cs")]
    [InlineData("na%", "endswith_cs")]
    public Task Translate_LikeAsync(string pattern, string action)
        => AssertQueryAsync(
            _q.Like(y => y.Name, pattern, '%'),
            [_tableName, $"where Name {action} 'na'"]
        );

    [Theory]
    [InlineData("na", "==")]
    [InlineData("%na%", "has_cs")]
    [InlineData("%na", "startswith_cs")]
    [InlineData("na%", "endswith_cs")]
    public Task Translate_LikeByPropNameAsync(string pattern, string action)
        => AssertQueryAsync(
            _q.Like("Name", pattern, '%'),
            [_tableName, $"where Name {action} 'na'"]
        );

    [Theory]
    [InlineData("na", "==")]
    [InlineData("*na*", "has_cs")]
    [InlineData("*na", "startswith_cs")]
    [InlineData("na*", "endswith_cs")]
    public Task Translate_LikeByStringMethodAsync(string pattern, string action)
        => AssertQueryAsync(
            _q.Where(x => x.Name.KqlLike(pattern, '*')),
            [_tableName, $"where Name {action} 'na'"]
        );

    [Fact]
    public Task Translate_WhereStringLikeStartsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "%na")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name startswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringLikeEndsWithAsync()
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name endswith_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereStringLikeHasAsync()
        => AssertQueryAsync(
            _q.Where(x => EF.Functions.Like(x.Name, "%na%")).Select(x => new { x.Date, x.Description }),
            [_tableName, $"where Name has_cs 'na'", "project Date, Description"]
        );

    [Fact]
    public Task Translate_WhereOrAsync()
        => AssertQueryAsync(
            _q.Or(new() { x => x.IsActive == true, x => x.IsActive == false}),
            [_tableName, $"where IsActive == true or IsActive == false"]
        );
}