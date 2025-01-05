using CSharp.OpenSource.LinqToKql.Test.Attributes;

namespace CSharp.OpenSource.LinqToKql.Test.Translator;

public class E2EOperationsTests : LinqToKQLQueryTranslatorBaseTest
{
    [E2EFact]
    public void Translate_Any()
    {
        var b = _q.Any(x => x.IsActive == true);
        Assert.True(b);
    }

    [E2EFact]
    public void Translate_MatchOne()
    {
        var a = _q.First(x => x.IsActive == true);
        var b = _q.FirstOrDefault(x => x.IsActive == true);
        var c = _q.Single(x => x.IsActive == true);
        var d = _q.SingleOrDefault(x => x.IsActive == true);
        Assert.True(a?.IsActive);
        Assert.True(b?.IsActive);
        Assert.True(c?.IsActive);
        Assert.True(d?.IsActive);
    }

    [E2EFact]
    public void Translate_NoMatch()
    {
        var b = _q.FirstOrDefault(x => x.Id == -3123);
        var d = _q.SingleOrDefault(x => x.Id == -3123);
        Assert.Null(b);
        Assert.Null(d);
    }
}