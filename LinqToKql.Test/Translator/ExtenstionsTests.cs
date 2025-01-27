using CSharp.OpenSource.LinqToKql.Extensions;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class ExtenstionsTests : LinqToKQLQueryTranslatorBaseTest
{
    [Fact]
    public Task WithPreExecute_TestAsync()
        => AssertQueryAsync(
            _q.WithPreExecute((p, kql) => $"{kql}{p.Translator.PipeWithIndentation}take 10").Select(x => x.Nested),
            [_tableName, "project Nested" , "take 10"]
        );
}