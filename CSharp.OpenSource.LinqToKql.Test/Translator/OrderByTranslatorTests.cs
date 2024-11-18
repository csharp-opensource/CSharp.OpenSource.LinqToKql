namespace CSharp.OpenSource.LinqToKql.Tests;

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
}