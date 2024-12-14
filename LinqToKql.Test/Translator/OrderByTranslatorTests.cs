namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class OrderByTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleOrderByAsync()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date),
            [_tableName, "sort by Date asc"]
        );

    [Fact]
    public Task Translate_ShouldHandleOrderByDescAsync()
        => AssertQueryAsync(
            _q.OrderByDescending(x => x.Date),
            [_tableName, "sort by Date desc"]
        );

    [Fact]
    public Task Translate_ShouldHandleThenByAsync()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date).ThenBy(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id asc"]
        );

    [Fact]
    public Task Translate_ShouldHandleThenByDescAsync()
        => AssertQueryAsync(
            _q.OrderBy(x => x.Date).ThenByDescending(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id desc"]
        );
}
