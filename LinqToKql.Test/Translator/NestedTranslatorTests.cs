using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class NestedTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleSimpleGroupBy()
        => AssertQueryAsync(
            _q.GroupBy(x => x.Nested.Id2),
            [_tableName, "summarize by tostring(Nested.Id2)"]
        );

    [Fact]
    public Task Translate_ShouldHandleWhereWithOr()
        => AssertQueryAsync(
            _q.Where(x => x.Nested.Id2 > 1 || x.Nested.Id2 == 3),
            [_tableName, "where Nested.Id2 > 1 or Nested.Id2 == 3"]
        );

    [Fact]
    public Task Translate_ShouldHandleOrderByDesc()
        => AssertQueryAsync(
            _q.OrderByDescending(x => x.Nested.Name2),
            [_tableName, "sort by tostring(Nested.Name2) desc"]
        );

    [Fact]
    public Task Translate_DisableNestedTrueSelects()
        => AssertQueryAsync(
            _q.Select(x => new { Name3 = x.Name, Nested = new { Name4 = x.Nested.Name2, } }),
            [_tableName, "project Name3=Name, Nested"],
            config: new() { DisableNestedProjection = true }
        );

    [Fact]
    public Task Translate_DisableNestedFalseSelects()
        => AssertQueryAsync(
            _q.Select(x => new { Name3 = x.Name, Nested = new { Name4 = x.Nested.Name2, } }),
            [_tableName, "extend Name3=Name, Nested=bag_pack(\"Name4\",Nested.Name2)"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public Task Translate_DisableNestedFalseWithClassSelects()
        => AssertQueryAsync(
            _q.Select(x => new { Name3 = x.Name, Nested = new SampleObject3 { Name3 = x.Nested.Name2, } }),
            [_tableName, "extend Name3=Name, Nested=bag_pack(\"Name3\",Nested.Name2)"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public Task Translate_ShouldHandleSelectCondition()
        => AssertQueryAsync(
            _q.Select(x => new { Name3 = x.Name, Nested = x.Nested != null ? new SampleObject3 { Name3 = x.Nested.Name2, } : default }),
            [_tableName, "extend Name3=Name, Nested=iff((isnotnull(Nested)),Name3=Nested.Name2,dynamic(null))"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public Task Translate_ShouldHandleSelectCondition_DisableNestedTrue()
        => AssertQueryAsync(
            _q.Select(x => new { Name3 = x.Name, Nested = x.Nested != null ? new SampleObject3 { Name3 = x.Nested.Name2, } : default }),
            [_tableName, "project Name3=Name, Nested"],
            config: new() { DisableNestedProjection = true }
        );
}