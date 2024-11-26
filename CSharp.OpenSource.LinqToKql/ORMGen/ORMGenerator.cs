using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Models;
using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGenerator
{
    public ORMGeneratorConfig Config { get; set; }
    protected virtual ORMKustoDbContext DbContext { get; set; }
    protected const string NewLine = "\n";
    protected const string TAB = "    ";

    public ORMGenerator(ORMGeneratorConfig config)
    {
        Config = config;
        DbContext = new(new KustoDbContextExecutor(config.ProviderExecutor));
    }

    public virtual async Task GenerateAsync()
    {
        PrepareFolders();
        var models = new List<ORMGenaratedModel>();
        foreach (var dbConfig in Config.DatabaseConfigs)
        {
            var tables = await GetTablesAsync(dbConfig);
            foreach (var table in tables)
            {
                models.Add(await GenerateTableModelAsync(table, dbConfig));
            }
            var functions = await GetFunctionsAsync(dbConfig);
            foreach (var function in functions)
            {
                models.Add(await GenerateFunctionModelAsync(function, dbConfig));
            }
        }
        if (Config.CreateDbContext)
        {
            await GenerateDbContextAsync(models);
        }
    }

    protected virtual async Task GenerateDbContextAsync(List<ORMGenaratedModel> models)
    {
        var namespaces = new List<string>
            {
                Config.Namespace,
                Config.ModelsNamespace,
                typeof(ORMKustoDbContext).Namespace!,
                typeof(LinqToKqlProvider<>).Namespace!,
            }
            .Distinct()
            .Select(x => $"using {x};");
        var dbContextName = Config.DbContextName ?? $"My{nameof(ORMKustoDbContext)}";
        var lines = new List<string>(namespaces)
        {
            "",
            $"namespace {Config.DbContextNamespace}",
            $"{{",
            $"{TAB}public class {dbContextName} : {nameof(ORMKustoDbContext)}",
            $"{TAB}{{",

            // ctor
            $"{TAB}public {dbContextName}(IKustoDbContextExecutor executor) : base(executor)",
            $"{TAB}{{",
            $"{TAB}}}"
        };

        // props
        foreach (var model in models)
        {
            lines.Add($"{TAB}{TAB}public IQueryable<{model.TypeName}> {model.TableOrFunctionDeclaration}");
            lines.Add($"{TAB}{TAB}{TAB}=> CreateQuery<{model.TypeName}>(\"{model.KQL}\");");
        }

        lines.Add($"{TAB}}}");
        lines.Add($"}}");

        var fileContent = string.Join(NewLine, lines);
        await File.WriteAllTextAsync(Config.DbContextPath, fileContent);
    }

    protected virtual async Task<ORMGenaratedModel> GenerateFunctionModelAsync(ShowFunctionsResult function, ORMGeneratorDatabaseConfig dbConfig)
    {
        var funcParams = function.Parameters.TrimStart('(').TrimEnd(')')
            .Split(',')
            .Select(x => x.Split(':'))
            .Select(x => new 
            { 
                name = x[0], 
                type = x[1], 
                csharpType = KustoTypeTranslate(x[1]),
            });
        var csharpParams = string.Join(", ", funcParams.Select(x => $"{x.type} {x.name}"));
        var kqlParams = string.Join(", ", funcParams.Select(x =>
        {
            if (x.type == "bool")
            {
                return $"{{{x.name}.ToString().ToLower()}}";
            }
            if (x.type == nameof(DateTime))
            {
                return $"datetime({{{x.name}:yyyy-MM-dd HH:mm:ss.f}}";
            }
            if (x.type == nameof(TimeSpan))
            {
                return $"timespan({{{x.name}.Days}}.{{{x.name}.Hours:D2}}:{{{x.name}.Minutes:D2}}:{{{x.name}.Seconds:D2}})";
            }
            if (x.type == "string")
            {
                return $"'{{{x.name}}}'";
            }
            return $"{{{x.name}}}";
        }));
        return new()
        {
            TypeName = function.Name,
            KQL = $"{function.Name}({kqlParams})",
            TableOrFunctionDeclaration = $"{function.Name}({csharpParams})"
        };
    }

    protected virtual async Task<ORMGenaratedModel> GenerateTableModelAsync(ShowTableResult table, ORMGeneratorDatabaseConfig dbConfig)
    {
        return new()
        {
            TypeName = table.TableName,
            KQL = table.TableName,
            TableOrFunctionDeclaration = table.TableName
        };
    }

    protected virtual async Task<List<ShowTableResult>> GetTablesAsync(ORMGeneratorDatabaseConfig dbConfig)
    {
        var tables = await DbContext.CreateQuery<ShowTableResult>(".show schema", dbConfig.DatabaseName)
            .Where(x => x.DatabaseName == dbConfig.DatabaseName)
            .ToListAsync();
        var filters = Config.Filters.TableFilters
            .Concat(Config.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.TableFilters)
            .ToList();
        return ApplyFilters(tables, t => t.TableName, filters);
    }

    protected virtual async Task<List<ShowFunctionsResult>> GetFunctionsAsync(ORMGeneratorDatabaseConfig dbConfig)
    {
        var functions = await DbContext.CreateQuery<ShowFunctionsResult>(".show functions", dbConfig.DatabaseName)
            .ToListAsync();
        var filters = Config.Filters.FunctionFilters
            .Concat(Config.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.FunctionFilters)
            .ToList();
        functions = ApplyFilters(functions, t => t.Name, filters);
        return functions;
    }

    protected virtual string DataTypeTranslate(string kustoDataType)
    {
        var type = kustoDataType.Replace("System.", "");
        type = type switch
        {
            nameof(String) => nameof(String).ToLower(),
            nameof(Object) => nameof(Object).ToLower(),
            nameof(SByte) => nameof(SByte).ToLower(),
            _ => type,
        };
        return type;
    }

    // https://learn.microsoft.com/en-us/kusto/query/scalar-data-types/?view=microsoft-fabric
    protected virtual string KustoTypeTranslate(string kustoType) =>
        kustoType switch
        {
            "bool" or "boolean" => "bool",
            "datetime" or "date" => nameof(DateTime),
            "decimal" => "decimal",
            "guid" or "uudi" or "uniqueid" => nameof(Guid),
            "int" => "int",
            "long" => "long",
            "real" => "double",
            "double" => "double",
            "string" => "string",
            "timespan" or "time" => nameof(TimeSpan),
            "dynamic" => "object",
            _ => "object",
        };

    protected virtual string PropDeclaration(string type, string name)
        => $"public {DataTypeTranslate(type)} {name} {{ get; set; }}";

    protected virtual List<T> ApplyFilters<T>(List<T> list, Func<T, string> valueGetter, List<ORMGeneratorFilter> filters)
    {
        return list.FindAll(table =>
        {
            if (filters.Any(f => !f.Exclude && f.Match(valueGetter(table))))
            {
                return true;
            }
            if (filters.Any(f => f.Exclude && f.Match(valueGetter(table))))
            {
                return false;
            }
            return true;
        });
    }

    protected virtual void PrepareFolders()
    {
        if (!Directory.Exists(Config.ModelsFolderPath)) { Directory.CreateDirectory(Config.ModelsFolderPath); }
        var dbContextFolder = Path.GetDirectoryName(Config.DbContextPath)!;
        if (!Directory.Exists(dbContextFolder)) { Directory.CreateDirectory(dbContextFolder); }
        if (Config.CleanModelsFolderBeforeCreate)
        {
            foreach (var file in Directory.GetFiles(Config.ModelsFolderPath, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }
    }
}
