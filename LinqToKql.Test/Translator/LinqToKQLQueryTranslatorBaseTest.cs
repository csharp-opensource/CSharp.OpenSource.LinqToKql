using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Provider;
using CSharp.OpenSource.LinqToKql.Test.Model;
using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public abstract class LinqToKQLQueryTranslatorBaseTest
{
    protected LinqToKQLQueryTranslator GetTranslator(LinqToKQLQueryTranslatorConfig? config = null) => new(config);
    protected const string _tableName = "SampleTable";
    protected readonly IQueryable<SampleObject> _q = new LinqToKqlProvider<SampleObject>(_tableName, null, E2EHelper.Client, null, null);

    protected async Task AssertQueryAsync<T>(IQueryable<T> queryable, string[] expectedArray, LinqToKQLQueryTranslatorConfig? config = null)
    {
        config ??= new();
        var translator = GetTranslator(config);
        var kql = queryable.AsKQL().Clone<T>();
        kql.Translator = translator;
        var expected = string.Join(
            translator.PipeWithIndentation,
            expectedArray
        );
        // Act
        var actual = kql.TranslateToKQL();

        // Assert
        var partsExpected = expected.Split(config.PipeWithIndentation);
        var actualParts = actual.Split(config.PipeWithIndentation);
        for (var i = 0; i < partsExpected.Length; i++)
        {
            Assert.Equal(partsExpected[i], actualParts[i]);
        }
        Assert.Equal(expected, actual);
        if (E2EHelper.IsE2E)
        {
            await E2EHelper.Client.ExecuteAsync<object>(actual);
        }
    }
}