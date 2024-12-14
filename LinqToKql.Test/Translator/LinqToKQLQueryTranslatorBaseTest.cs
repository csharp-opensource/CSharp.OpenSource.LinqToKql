using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Test.Model;
using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public abstract class LinqToKQLQueryTranslatorBaseTest
{
    protected LinqToKQLQueryTranslator GetTranslator(LinqToKQLQueryTranslatorConfig? config = null) => new(config);
    protected readonly string _tableName = "SampleTable";
    protected readonly IQueryable<SampleObject> _q = new[] { new SampleObject { } }.AsQueryable();
    protected readonly KustoHttpClient _client;

    protected LinqToKQLQueryTranslatorBaseTest()
    {
        _client = new("http://localhost:8080", "", "TestDatabase1");
        _client.HttpClient.Timeout = TimeSpan.FromSeconds(3);
    }

    protected void AssertQuery<T>(IQueryable<T> queryable, string[] expectedArray, LinqToKQLQueryTranslatorConfig? config = null)
    {
        config ??= new();
        var translator = GetTranslator(config);
        var expected = string.Join(
            translator.PipeWithIndentation,
            expectedArray
        );
        // Act
        var actual = translator.Translate(queryable, _tableName);

        // Assert
        var partsExpected = expected.Split(config.PipeWithIndentation);
        var actualParts = actual.Split(config.PipeWithIndentation);
        for (var i = 0; i < partsExpected.Length; i++)
        {
            Assert.Equal(partsExpected[i], actualParts[i]);
        }
        Assert.Equal(expected, actual);
        if (Environment.GetEnvironmentVariable("E2E_TESTING") == "1")
        {
            _client.ExecuteAsync<object>(actual).Wait();
        }
    }
}