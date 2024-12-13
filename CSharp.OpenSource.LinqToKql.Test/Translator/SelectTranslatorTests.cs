using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class SelectTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public void Translate_ShouldHandleSelectWithInit()
        => AssertQuery(
            _q.Select(x => new SampleObject2 { Name2 = x.Name, Id2 = x.Id }),
            [_tableName, "project Name2=Name, Id2=Id"]
        );

    [Fact]
    public void Translate_ShouldHandleFirst()
        => AssertQuery(
            _q.First(),
            [_tableName, "take 1"]
        );

    [Fact]
    public void Translate_ShouldHandleFirstOrDefault()
        => AssertQuery(
            _q.FirstOrDefault(),
            [_tableName, "take 1"]
        );

    [Fact]
    public void Translate_ShouldHandleLast()
        => AssertQuery(
            _q.OrderBy(x => x.Id).Last(),
            [_tableName, "sort by Id asc", "take 1"]
        );

    [Fact]
    public void Translate_ShouldHandleLastOrDefault()
        => AssertQuery(
            _q.OrderBy(x => x.Id).LastOrDefault(),
            [_tableName, "sort by Id asc", "take 1"]
        );
}
