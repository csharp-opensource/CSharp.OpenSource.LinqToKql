using CSharp.OpenSource.LinqToKql.Provider;
using CSharp.OpenSource.LinqToKql.Test.Model;
using CSharp.OpenSource.LinqToKql.Translator;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public interface IMyMultiDbContext : IKustoDbContext
{
    IQueryable<SampleObject> SampleObjects { get; }
    IQueryable<SampleObject2> SampleObjects2 { get; }

    IQueryable<SampleObject> Func(int minYear);
    IQueryable<SampleObject> Func2(int minYear);
    IQueryable<SampleObject> Func3(int minYear);
}

public class MyMultiDbContext : KustoDbContext, IMyMultiDbContext
{
    public MyMultiDbContext(IKustoDbContextExecutor<MyMultiDbContext> executor) : base(executor)
    {
    }

    // from default db
    public IQueryable<SampleObject> SampleObjects
        => CreateQuery<SampleObject>("sampleObjects");

    // from different db
    public IQueryable<SampleObject2> SampleObjects2
        => CreateQuery<SampleObject2>("sampleObjects2", database: "diffDb");

    public IQueryable<SampleObject> Func(int minYear)
        => CreateQuery<SampleObject>($"sampleObjects | where year > {minYear}");

    public IQueryable<SampleObject> Func2(int minYear)
        => CreateQuery<SampleObject>($"sampleObjectsByMinYear({minYear})");

    public IQueryable<SampleObject> Func3(int minYear)
        => SampleObjects.Where(x => x.Year > minYear);

    protected override LinqToKQLQueryTranslatorConfig GetConfig() => new() { DisableNestedProjection = true };
}

public class MyMultiDbContext2 : MyMultiDbContext
{
    public MyMultiDbContext2(IKustoDbContextExecutor<MyMultiDbContext> executor) : base(executor)
    {
    }
}

