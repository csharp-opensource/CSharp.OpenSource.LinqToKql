# CSharp.OpenSource.LinqToKql

[![Nuget Package](https://github.com/csharp-opensource/CSharp.OpenSource.LinqToKql/actions/workflows/nugetPublish.yml/badge.svg)](https://github.com/csharp-opensource/CSharp.OpenSource.LinqToKql/actions/workflows/nugetPublish.yml)
[![NuGet Version](https://img.shields.io/nuget/v/CSharp.OpenSource.LinqToKql.svg)](https://www.nuget.org/packages/CSharp.OpenSource.LinqToKql/)


# Example of using KustoHttpClientLinqProviderFactory
```csharp
using CSharp.OpenSource.LinqToKql.Http;
using CSharp.OpenSource.LinqToKql.Test.Model;

namespace CSharp.OpenSource.LinqToKql.Test.Provider;

public class SampleKustoHttpClientLinqProviderFactory : KustoHttpClientLinqProviderFactory
{
    private string _cluster = "https://myclusterUrl/";
    private string _auth = "myBearerToken";
    private string _dbName = "myDatabaseName";

    public IQueryable<SampleObject> SampleObject
        => CreateQuery<SampleObject>(_cluster, _auth, _dbName, "SampleObjectTable");

    public IQueryable<SampleObject> SampleObjectFunction(long number)
        => CreateQuery<SampleObject>(_cluster, _auth, _dbName, $"SampleObjectFunction({number})");

    public IQueryable<SampleObject2> SampleObject2(long number)
        => CreateQuery<SampleObject2>(_cluster, _auth, _dbName, $"SampleObject2Table");
}
```