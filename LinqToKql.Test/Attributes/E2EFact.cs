namespace CSharp.OpenSource.LinqToKql.Test.Attributes;

public class E2EFactAttribute : FactAttribute
{
    public override string Skip { get => E2EHelper.IsE2E ? null : "this test run only in e2e"; set => _ = value; }
}
