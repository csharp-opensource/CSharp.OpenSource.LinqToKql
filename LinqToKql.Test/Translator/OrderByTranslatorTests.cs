namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class OrderByTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleOrderBy()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date),
            [_tableName, "sort by Date asc"]
        );

    [Fact]
    public Task Translate_ShouldHandleOrderByDesc()
        => AssertQueryAsync(
            _q.OrderByDescending(x => x.Date),
            [_tableName, "sort by Date desc"]
        );

    [Fact]
    public Task Translate_ShouldHandleThenBy()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date).ThenBy(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id asc"]
        );

    [Fact]
    public Task Translate_ShouldHandleThenByDesc()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date).ThenByDescending(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id desc"]
        );
}
