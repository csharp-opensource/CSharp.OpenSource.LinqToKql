using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Tests;

public class GroupByTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public void Translate_ShouldHandleSimpleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Date),
            [_tableName, "summarize by Date"]
        );

    [Fact]
    public void Translate_ShouldHandleSimpleGroupByWithObject()
        => AssertQuery(
            _q.GroupBy(x => new { x.Date, x.Description }),
            [_tableName, "summarize by Date, Description"]
        );

    [Fact]
    public void Translate_ShouldHandleGroupBy()
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new { Date = g.Key, Count = g.Count() }),
            [_tableName, "summarize Count=count() by Date", "project Date, Count"]
        );

    [Fact]
    public void Translate_ShouldHandleGroupByWithObject()
        => AssertQuery(
            _q.GroupBy(x => x.Date).Select(g => new GroupResult { Key = g.Key, Count = g.Count() }),
            [_tableName, "summarize Key=Date, Count=count() by Date", "project Key=Key, Count=Count"]
        );

    //[Fact]
    //public void Translate_ShouldHandleGroupByWithObjectAndKeyObject()
    //    => AssertQuery(
    //        _q.GroupBy(x => new { x.Date, x.Description }).Select(g => new GroupResult { Key = g.Key, Count = g.Count() }),
    //        [_tableName, "summarize Count=count() by Date, Description", "project Date, Description, Count"]
    //    );
}