namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class OrderByTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
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
    public void Translate_ShouldHandleThenBy()
        => AssertQuery(
            _q.OrderBy(x => x.Date).ThenBy(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id asc"]
        );

    [Fact]
    public void Translate_ShouldHandleThenByDesc()
        => AssertQuery(
            _q.OrderBy(x => x.Date).ThenByDescending(x => x.Id),
            [_tableName, "sort by Date asc", "sort by Id desc"]
        );
}
