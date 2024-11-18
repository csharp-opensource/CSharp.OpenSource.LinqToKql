namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class TakeTranslatorTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public void Translate_ShouldHandleTake()
        => AssertQuery(
            _q.Take(50),
            [_tableName, "take 50"]
        );
}