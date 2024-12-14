using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class NestedTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public void Translate_ShouldHandleSimpleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Nested.Id2),
            [_tableName, "summarize by tostring(Nested.Id2)"]
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
            [_tableName, "sort by tostring(Nested.Name2) desc"]
        );

    [Fact]
    public void Translate_DisableNestedTrueSelects()
        => AssertQuery(
            _q.Select(x => new { Name3 = x.Name, Nested = new { Name4 = x.Nested.Name2, } }),
            [_tableName, "project Name3=Name, Nested"],
            config: new() { DisableNestedProjection = true }
        );

    [Fact]
    public void Translate_DisableNestedFalseSelects()
        => AssertQuery(
            _q.Select(x => new { Name3 = x.Name, Nested = new { Name4 = x.Nested.Name2, } }),
            [_tableName, "extend Name3=Name, Nested=bag_pack(\"Name4\",Nested.Name2)"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public void Translate_DisableNestedFalseWithClassSelects()
        => AssertQuery(
            _q.Select(x => new { Name3 = x.Name, Nested = new SampleObject3 { Name3 = x.Nested.Name2, } }),
            [_tableName, "extend Name3=Name, Nested=bag_pack(\"Name3\",Nested.Name2)"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public void Translate_ShouldHandleSelectCondition()
        => AssertQuery(
            _q.Select(x => new { Name3 = x.Name, Nested = x.Nested != null ? new SampleObject3 { Name3 = x.Nested.Name2, } : default }),
            [_tableName, "extend Name3=Name, Nested=iff((isnotnull(Nested)),Name3=Nested.Name2,dynamic(null))"],
            config: new() { DisableNestedProjection = false }
        );

    [Fact]
    public void Translate_ShouldHandleSelectCondition_DisableNestedTrue()
        => AssertQuery(
            _q.Select(x => new { Name3 = x.Name, Nested = x.Nested != null ? new SampleObject3 { Name3 = x.Nested.Name2, } : default }),
            [_tableName, "project Name3=Name, Nested"],
            config: new() { DisableNestedProjection = true }
        );
}