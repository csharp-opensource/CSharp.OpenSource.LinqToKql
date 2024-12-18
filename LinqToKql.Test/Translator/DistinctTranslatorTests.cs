using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class DistinctTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleDistinctAsync()
        => AssertQueryAsync(
            _q.Distinct(),
            [_tableName, "distinct *"]
        );
}
