# CSharp.OpenSource.LinqToKql

[![Nuget Package](https://github.com/csharp-opensource/CSharp.OpenSource.LinqToKql/actions/workflows/nugetPublish.yml/badge.svg)](https://github.com/csharp-opensource/CSharp.OpenSource.LinqToKql/actions/workflows/nugetPublish.yml)
[![NuGet Version](https://img.shields.io/nuget/v/CSharp.OpenSource.LinqToKql.svg)](https://www.nuget.org/packages/CSharp.OpenSource.LinqToKql/)


# Example of using KustoDbContext
```csharp
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
```

# Example of using KustoHttpClientLinqProviderFactory
```csharp
using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public class SampleKustoHttpClientLinqProviderFactory : KustoHttpClientLinqProviderFactory
{
    private string _cluster = "https://myclusterUrl/";
    private string _auth = "myBearerToken";
    private string _defaultDbName = "myDatabaseName";

    public IQueryable<SampleObject> SampleObject
        => CreateQuery<SampleObject>(_cluster, _auth, _defaultDbName, "SampleObjectTable");

    public IQueryable<SampleObject> SampleObjectFunction(long number)
        => CreateQuery<SampleObject>(_cluster, _auth, _defaultDbName, $"SampleObjectFunction({number})");

    public IQueryable<SampleObject2> SampleObject2(long number)
        => CreateQuery<SampleObject2>(_cluster, _auth, _defaultDbName, $"SampleObject2Table");
}
```