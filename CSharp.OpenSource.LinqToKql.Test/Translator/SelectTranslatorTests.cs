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
}