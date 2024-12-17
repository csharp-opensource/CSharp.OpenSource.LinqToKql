﻿using CSharp.OpenSource.LinqToKql.Extensions;
using CSharp.OpenSource.LinqToKql.Models;
using CSharp.OpenSource.LinqToKql.Provider;
using System.Text.RegularExpressions;

namespace CSharp.OpenSource.LinqToKql.ORMGen;

public class ORMGenerator
{
    public ORMGeneratorConfig Config { get; set; }
    protected virtual ORMKustoDbContext DbContext { get; set; }
    protected const string NewLine = "\n";
    protected const string TAB = "    ";
    private Dictionary<string, int> _modelsNames = new();

    public ORMGenerator(ORMGeneratorConfig config)
    {
        Config = config;
        DbContext = new(new KustoDbContextExecutor(config.ProviderExecutor));
    }

    public virtual async Task GenerateAsync()
    {
        PrepareConfig();
        PrepareFolders();
        var models = new List<ORMGenaratedModel>();
        foreach (var dbConfig in Config.DatabaseConfigs)
        {
            dbConfig.DatabaseDisplayName ??= dbConfig.DatabaseName;
            Console.WriteLine(" ");
            Console.WriteLine("-------------------------");
            Console.WriteLine($" Start Generate {dbConfig.DatabaseName}");
            Console.WriteLine("-------------------------");
            Console.WriteLine(" ");
            models.Add(new() { DatabaseConfig = dbConfig, TypeName = "T", KQL = "{kql}", TableOrFunctionDeclaration = $"{dbConfig.DatabaseDisplayName}<T>(string kql = \"\")" });
            var tables = await GetTablesAsync(dbConfig);
            if (tables.Count > 0)
            {
                Console.WriteLine("---------- Tables ----------");
                foreach (var table in tables)
                {
                    Console.WriteLine($"{table.Name} Start");
                    models.Add(await GenerateTableModelAsync(table, dbConfig));
                    Console.WriteLine($"{table.Name} End");
                }
                Console.WriteLine(" ");
            }

            var functions = await GetFunctionsAsync(dbConfig);
            if (functions.Count > 0)
            {
                Console.WriteLine("---------- Functions ----------");
                foreach (var function in functions)
                {
                    Console.WriteLine($"{function.Name} Start");
                    models.Add(await GenerateFunctionModelAsync(function, dbConfig));
                    Console.WriteLine($"{function.Name} End");
                }
                Console.WriteLine(" ");
            }
        }
        if (Config.CreateDbContext)
        {
            Console.WriteLine("---------- DbContext ----------");
            await GenerateDbContextAsync(models);
        }
    }

    private void PrepareConfig()
    {
        Config.ModelsNamespace ??= Config.Namespace ?? throw new ArgumentNullException(nameof(Config.ModelsNamespace));
        Config.DbContextNamespace ??= Config.Namespace ?? throw new ArgumentNullException(nameof(Config.DbContextNamespace));
        Config.DbContextName ??= $"My{nameof(ORMKustoDbContext)}";
        Config.ModelsFolderPath ??= "Models";
        Config.ModelsFolderPath = Config.ModelsFolderPath.Replace("\\", "/");
        Config.DbContextFolderPath = Config.DbContextFolderPath.Replace("\\", "/");
        if (!Config.ModelsFolderPath.Contains("/") && Config.DbContextFolderPath.Contains("/"))
        {
            Config.ModelsFolderPath = Path.Combine(Config.DbContextFolderPath, Config.ModelsFolderPath);
        }
    }

    protected virtual async Task GenerateDbContextAsync(List<ORMGenaratedModel> models)
    {
        var usings = new List<string>
        {
            Config.Namespace,
            Config.ModelsNamespace,
            typeof(ORMKustoDbContext).Namespace!,
            typeof(LinqToKqlProvider<>).Namespace!,
            typeof(ObjectExtension).Namespace!,
        };
        var lines = new List<string>
        {
            $"public partial class {Config.DbContextName} : {nameof(ORMKustoDbContext)}",
            $"{{",
        };
        if (Config.DbContextCreateConstructor)
        {
            lines.AddRange(new List<string>
            {
                $"{TAB}public {Config.DbContextName}(IKustoDbContextExecutor<{Config.DbContextName}> executor) : base(executor)",
                $"{TAB}{{",
                $"{TAB}}}",
                "",
            });
        }

        // props
        foreach (var (model, index) in models.Select((model, index) => (model, index)))
        {
            if (index != 0) { lines.Add(""); }
            lines.Add($"{TAB}public virtual IQueryable<{model.TypeName}> {model.TableOrFunctionDeclaration}");
            lines.Add($"{TAB}{TAB}=> CreateQuery<{model.TypeName}>($\"{model.KQL}\", GetDatabaseName(\"{model.DatabaseConfig.DatabaseName}\"));");
        }
        lines.Add($"}}");
        await File.WriteAllTextAsync(Config.DbContextFilePath, WrapContentWithNamespaceAndUsing(lines, usings, Config.DbContextNamespace));
    }

    protected virtual string WrapContentWithNamespaceAndUsing(List<string> lines, List<string> usings, string @namespace, string? referenceHint = null)
    {
        var res = new List<string>();
        res.Add($"// <auto-generated> This file has been auto generated by {typeof(ORMGenerator).FullName}. </auto-generated>");
        if (referenceHint != null)
        {
            res.Add($"// <auto-generated> {referenceHint} </auto-generated>");
        }
        res.Add("#pragma warning disable IDE1006 // Naming Styles");
        if (Config.EnableNullable) { res.Add("#nullable enable"); }
        usings = usings.Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .Where(x => x != @namespace)
            .Select(x => x.StartsWith("using") ? x : $"using {x};")
            .ToList();
        res.AddRange(usings);
        res.Add("");
        res.Add($"namespace {@namespace}{(Config.FileScopedNamespaces ? ";" : "")}");
        if (!Config.FileScopedNamespaces) { res.Add($"{{"); }
        else { res.Add(""); }
        res.AddRange(lines.Select(line => $"{(Config.FileScopedNamespaces ? "" : TAB)}{line}"));
        if (!Config.FileScopedNamespaces) { res.Add($"}}"); }
        return string.Join(NewLine, res);
    }

    protected virtual async Task<ORMGenaratedModel> GenerateFunctionModelAsync(ShowFunctionsResult function, ORMGeneratorDatabaseConfig dbConfig)
    {
        var typeName = GetModelName(function.Name);
        var csharpParams = string.Join(", ", function.ParametersItems.Select(x => $"{x.Type} {x.Name}"));
        var kqlParams = string.Join(", ", function.ParametersItems.Select(x => $"{{{x.Name}.GetKQLValue()}}"));

        var usings = new List<string> { "System" };
        var modelDeclarationLines = GetModelDeclarationLines(typeName, function.Name, dbConfig.DatabaseName);
        var lines = new List<string>(modelDeclarationLines);
        var functionColumns = await GetFunctionSchemaAsync(function, dbConfig);
        foreach (var column in functionColumns)
        {
            GenerateProperty(function.Name, dbConfig, lines, column.DataType, column.ColumnName);
        }
        lines.Add($"}}");
        var fileContent = WrapContentWithNamespaceAndUsing(
            lines,
            usings,
            Config.ModelsNamespace,
            referenceHint: $"database('{dbConfig.DatabaseName}').{function.Name}{function.Parameters}"
        );
        var modelFolder = GetDbModelsFolder(dbConfig);
        var filePath = Path.Combine(modelFolder, $"{typeName}.cs");
        await File.WriteAllTextAsync(filePath, fileContent);
        return new()
        {
            TypeName = typeName,
            TableOrFunctionDeclaration = $"{typeName}({csharpParams})",
            KQL = $"{function.Name}({kqlParams})",
            DatabaseConfig = dbConfig,
        };
    }

    private string GetDbModelsFolder(ORMGeneratorDatabaseConfig dbConfig)
    {
        var modelFolder = Config.DatabaseConfigs.Count > 1
            ? Path.Combine(Config.ModelsFolderPath, dbConfig.DatabaseDisplayName)
            : Config.ModelsFolderPath;
        if (!Directory.Exists(modelFolder)) { Directory.CreateDirectory(modelFolder); }
        return modelFolder;
    }

    private List<string> GetModelDeclarationLines(string typeName, string name, string databaseName)
    {
        var modification = Config.TableOrFunctionModifications
            .Where(x => x.DatabasePatterns.Count == 0 || x.DatabasePatterns.Any(p => Match(p, databaseName)))
            .Where(x => x.TableOrFunctionPatterns.Count == 0 || x.TableOrFunctionPatterns.Any(p => Match(p, name)))
            .FirstOrDefault();
        var res = new List<string>();
        var declaration = $"public partial class {typeName}";
        if (modification == null)
        {
            res.Add(declaration);
            res.Add("{");
            return res;
        }
        if (!string.IsNullOrEmpty(modification.ClassAttributes))
        {
            res.Add(modification.ClassAttributes);
        }
        if (!string.IsNullOrEmpty(modification.ClassInherit))
        {
            declaration += $" : {modification.ClassInherit}";
        }
        res.Add(declaration);
        res.Add("{");
        res.AddRange(modification.BodyExtraLines.Select(x => $"{TAB}{x}"));
        return res;
    }

    private void GenerateProperty(string tableOrFunctionName, ORMGeneratorDatabaseConfig dbConfig, List<string> lines, string columnType, string columnName)
    {
        var modification = Config.ColumnModifications
            .Where(x => x.DatabasePatterns.Count == 0 || x.DatabasePatterns.Any(p => Match(p, dbConfig.DatabaseName)))
            .Where(x => x.TableOrFunctionPatterns.Count == 0 || x.TableOrFunctionPatterns.Any(p => Match(p, tableOrFunctionName)))
            .Where(x => x.ColumnNamePatterns.Count == 0 || x.ColumnNamePatterns.Any(p => Match(p, columnName)))
            .FirstOrDefault();
        if (modification?.Exclude == true)
        {
            return;
        }
        var attributes = modification?.ColumnAttributes ?? "";
        if (attributes.Length > 0)
        {
            lines.Add($"{TAB}{attributes}");
        }
        columnType = modification?.NewColumnType ?? DataTypeTranslate(columnType);
        lines.Add($"{TAB}public virtual {columnType} {columnName} {{ get; set; }}");
    }

    private async Task<List<GetSchemaResult>> GetFunctionSchemaAsync(ShowFunctionsResult function, ORMGeneratorDatabaseConfig dbConfig)
    {
        return await DbContext.CreateQuery<GetSchemaResult>($"{function.Name}({string.Join(", ", function.ParametersItems.Select(x => DefaultValue(x.Type)))})", dbConfig.DatabaseName)
            .Take(1)
            .FromKQL("getschema")
            .ToListAsync();
    }

    protected virtual async Task<ORMGenaratedModel> GenerateTableModelAsync(ORMGeneratorTable table, ORMGeneratorDatabaseConfig dbConfig)
    {
        var typeName = GetModelName(table.Name);
        var usings = new List<string> { "System" };
        var modelDeclarationLines = GetModelDeclarationLines(typeName, table.Name, dbConfig.DatabaseName);
        var lines = new List<string>(modelDeclarationLines);
        foreach (var column in table.Columns)
        {
            GenerateProperty(table.Name, dbConfig, lines, column.ColumnType, column.ColumnName);
        }
        lines.Add($"}}");
        var fileContent = WrapContentWithNamespaceAndUsing(
            lines,
            usings,
            Config.ModelsNamespace,
            referenceHint: $"database('{dbConfig.DatabaseName}').{table.Name}"
        );
        var modelFolder = GetDbModelsFolder(dbConfig);
        var filePath = Path.Combine(modelFolder, $"{typeName}.cs");
        await File.WriteAllTextAsync(filePath, fileContent);
        return new()
        {
            TypeName = typeName,
            TableOrFunctionDeclaration = typeName,
            KQL = table.Name,
            DatabaseConfig = dbConfig,
        };
    }

    protected virtual async Task<List<ORMGeneratorTable>> GetTablesAsync(ORMGeneratorDatabaseConfig dbConfig)
    {
        var tables = await DbContext.CreateQuery<ShowSchemaResult>(".show schema", dbConfig.DatabaseName)
            .Where(x => x.DatabaseName == dbConfig.DatabaseName)
            .ToListAsync();
        var filters = Config.Filters.TableFilters
            .Concat(Config.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.GlobalFilters)
            .Concat(dbConfig.Filters.TableFilters)
            .ToList();
        return ApplyFilters(tables, t => t.TableName, filters)
                .Where(x => !string.IsNullOrEmpty(x.TableName))
                .GroupBy(x => x.TableName)
                .Select(x => new ORMGeneratorTable { Name = x.Key, Columns = x.Where(x => !string.IsNullOrEmpty(x.ColumnName)).ToList() })
                .ToList();
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
        functions = ApplyFilters(functions, t => t.Name, filters).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
        foreach (var function in functions)
        {
            function.ParametersItems = function.Parameters?.TrimStart('(').TrimEnd(')')
              .Split(',')
              .Where(x => !string.IsNullOrEmpty(x)) // solve empty funcs
              .Select(x => x.Trim().Split(':').Select(x => x.Trim()).ToArray())
              .Select(x => new ORMGeneratorFunctionParam
              {
                  Name = x[0],
                  Type = x[1],
              })
              .ToList() ?? new();
        }
        return functions;
    }

    protected virtual List<T> ApplyFilters<T>(List<T> list, Func<T, string> valueGetter, List<ORMGeneratorFilter> filters)
    {
        return list.FindAll(table =>
        {
            if (filters.Any(f => !f.Exclude && Match(valueGetter(table), f.Pattern)))
            {
                return true;
            }
            if (filters.Any(f => f.Exclude && Match(valueGetter(table), f.Pattern)))
            {
                return false;
            }
            return true;
        });
    }

    protected virtual void PrepareFolders()
    {
        if (!Directory.Exists(Config.ModelsFolderPath)) { Directory.CreateDirectory(Config.ModelsFolderPath); }
        if (!Directory.Exists(Config.DbContextFolderPath)) { Directory.CreateDirectory(Config.DbContextFolderPath); }
        if (Config.CleanFolderBeforeCreate)
        {
            if (File.Exists(Config.DbContextFilePath))
            {
                File.Delete(Config.DbContextFilePath);
            }
            foreach (var file in Directory.GetFiles(Config.ModelsFolderPath, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }
    }

    // https://learn.microsoft.com/en-us/kusto/query/scalar-data-types/?view=microsoft-fabric
    public string CsharpType(string kustoType) => kustoType switch
    {
        "bool" or "boolean" => "bool?",
        "datetime" or "date" => $"{nameof(DateTime)}?",
        "decimal" => "decimal?",
        "guid" or "uudi" or "uniqueid" => $"{nameof(Guid)}?",
        "int" => "int?",
        "long" => "long?",
        "real" => "double?",
        "double" => "double?",
        "string" => $"string{(Config.EnableNullable ? "?" : "")}",
        "timespan" or "time" => $"{nameof(TimeSpan)}?",
        "dynamic" => "object?",
        _ => "object?",
    };

    // https://github.com/microsoft/Kusto-Query-Language/blob/master/doc/scalar-data-types/index.md
    public virtual string DataTypeTranslate(string kustoDataType)
    {
        var type = kustoDataType.Replace("System.", "");
        type = type switch
        {
            nameof(String) => nameof(String).ToLower(),
            nameof(Object) => nameof(Object).ToLower(),
            nameof(SByte) => "bool",
            nameof(Boolean) => "bool",
            nameof(Int64) => "long",
            nameof(Int16) => "short",
            nameof(Int32) => "int",
            nameof(Double) => nameof(Double).ToLower(),
            nameof(Decimal) => nameof(Decimal).ToLower(),
            "System.Data.SqlTypes.SqlDecimal" => nameof(Decimal).ToLower(),
            _ => type,
        };
        if (!Config.EnableNullable && type == "string")
        {
            return type;
        }
        return type + "?";
    }

    public string DefaultValue(string kustoType) => kustoType switch
    {
        "bool" or "boolean" => false.GetKQLValue(),
        "datetime" or "date" => DateTime.UtcNow.GetKQLValue(),
        "decimal" or "int" or "long" or "double" or "real" => (-1).GetKQLValue(),
        "guid" or "uudi" or "uniqueid" => Guid.Empty.GetKQLValue(),
        "string" => "''",
        "timespan" or "time" => TimeSpan.FromSeconds(1).GetKQLValue(),
        "dynamic" => "dynamic({})",
        _ => "null",
    };

    public string GetModelName(string name)
    {
        if (!_modelsNames.ContainsKey(name))
        {
            _modelsNames[name] = 1;
            return name;
        }
        var finalName = name + _modelsNames[name];
        _modelsNames[name]++;
        return finalName;
    }

    public bool Match(string value, string pattern)
    {
        // Escape the pattern and replace '*' and '?' with regex equivalents
        string regexPattern = "^" + Regex.Escape(pattern)
                                     .Replace("\\*", ".*")
                                     .Replace("\\?", ".") + "$";
        return Regex.IsMatch(value, regexPattern);
    }
}
