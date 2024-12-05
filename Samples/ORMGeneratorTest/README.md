```csharp
using AutoGen;
using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Http;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddKustoDbContext<AutoGenORMKustoDbContext, KustoHttpClient>(sp => new KustoHttpClient("mycluster", "auth", "dbName"));
var provider = services.BuildServiceProvider();
var dbContext = provider.GetRequiredService<AutoGenORMKustoDbContext>();

dbContext.TestTable1.Where(x => x.TestColumn == "test").ToList();
dbContext.db2<object>("customQuery").ToList();
dbContext.func1().ToList();
dbContext.func2("name", "lastName").ToList();
```