namespace CSharp.OpenSource.LinqToKql.Tests;

public class NestedTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public void Translate_ShouldHandleSimpleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Nested.Id2),
            [_tableName, "summarize by Nested.Id2"]
        );

    [Fact]
    public void Translate_ShouldHandleWhereWithOr()
        => AssertQuery(
            _q.Where(x => x.Nested.Id2 > 1 || x.Nested.Id2 == 3),
            [_tableName, "where Nested.Id2 > 1 or Nested.Id2 == 3"]
        );

    [Fact]
    public void Translate_ShouldHandleOrderByDesc()
        => AssertQuery(
            _q.OrderByDescending(x => x.Nested.Name2),
            [_tableName, "sort by Nested.Name2 desc"]
        );
}