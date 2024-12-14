namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class TakeTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task Translate_ShouldHandleTake()
        => AssertQueryAsync(
            _q.Take(50),
            [_tableName, "take 50"]
        );
}
