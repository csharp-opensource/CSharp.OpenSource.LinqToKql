using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class GroupByTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translate_ShouldHandleSimpleGroupBy(bool disableNestedProjection)
        => AssertQuery(
            _q.GroupBy(x => x.Date),
            [_tableName, "summarize by Date"],
            new() { DisableNestedProjection = disableNestedProjection, }
        );

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translate_ShouldHandleSimpleGroupByWithObject(bool disableNestedProjection)
        => AssertQuery(
            _q.GroupBy(x => new { x.Date, x.Description }),
            [_tableName, "summarize by Date, Description"],
            new() { DisableNestedProjection = disableNestedProjection, }
        );

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translate_ShouldHandleGroupBy(bool disableNestedProjection)
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new { Date = g.Key, Count = g.Count() }),
            [_tableName, "summarize Count=count() by Date"],
            new() { DisableNestedProjection = disableNestedProjection, }
        );

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translate_ShouldHandleGroupByWithObject(bool disableNestedProjection)
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new GroupResult { Key = g.Key, Count = g.Count() }),
            [_tableName, "summarize Key=take_any(Date), Count=count() by Date"],
            new() { DisableNestedProjection = disableNestedProjection, }
        );

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translate_ShouldHandleGroupByWithObject2(bool disableNestedProjection)
        => AssertQuery(
            _q.Where(x => x.Id == 1).GroupBy(x => x.Date).Select(g => new GroupResult { Key = g.Key, Count = g.Count() }),
            [_tableName, "where Id == 1", "summarize Key=take_any(Date), Count=count() by Date"],
            new() { DisableNestedProjection = disableNestedProjection, }
        );
}