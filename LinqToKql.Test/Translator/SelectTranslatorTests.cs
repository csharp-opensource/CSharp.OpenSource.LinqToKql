using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class SelectTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleSelectWithInitAsync()
        => AssertQueryAsync(
            _q.Select(x => new SampleObject2 { Name2 = x.Name, Id2 = x.Id }),
            [_tableName, "project Name2=Name, Id2=Id"]
        );

    [Fact]
    public Task Translate_ShouldHandleDistinctByAsync()
        => AssertQueryAsync(
            _q.DistinctBy(x => new { x.Name, x.Id }),
            [_tableName, "distinct Name, Id"]
        );

    [Fact]
    public Task Translate_ShouldHandleDistinctAsync()
        => AssertQueryAsync(
            _q.Select(x => new { x.Name, x.Id }).Distinct(),
            [_tableName, "distinct Name, Id"]
        );
}
