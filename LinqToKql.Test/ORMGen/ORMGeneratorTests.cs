using CSharp.OpenSource.LinqToKql.ORMGen;
using CSharp.OpenSource.LinqToKql.Models;
using CSharp.OpenSource.LinqToKql.Provider;
using Moq;
using CSharp.OpenSource.LinqToKql.Test.Attributes;

namespace CSharp.OpenSource.LinqToKql.Test.ORMGen;

public class ORMGeneratorTests
{
    private const string DbName1 = "TestDatabase1";
    private const string DbName2 = "TestDatabase2";
    private readonly Mock<ILinqToKqlProviderExecutor> _mockExecutor = new();

    private void SetupProviderExecutor()
    {
        // Arrange
        _mockExecutor.Setup(executor => executor.ExecuteAsync<List<ShowSchemaResult>>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string kql, string db) =>
            {
                if (db == DbName1)
                {
                    return new()
                    {
                        new() { DatabaseName = DbName1, TableName = "TestTable", ColumnName = "TestColumn", ColumnType = "string" }
                    };
                }
                if (db == DbName2)
                {
                    return new()
                    {
                        new() { DatabaseName = DbName2, TableName = "TestTable", ColumnName = "TestColumn", ColumnType = "string" }
                    };
                }
                return new();
            });

        _mockExecutor.Setup(executor => executor.ExecuteAsync<List<ShowFunctionsResult>>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string kql, string db) =>
            {
                if (db == DbName1)
                {
                    return new()
                    {
                        new() { Body = "myOtherTable1 | take 1", Name = "func1" }
                    };
                }
                if (db == DbName2)
                {
                    return new()
                    {
                        new() { Body = "myOtherTable2 | where userName == name and userLastName == lastName | take 1", Name = "func2", Parameters = "(name: string, lastName: string)" }
                    };
                }
                return new();
            });

        _mockExecutor.Setup(executor => executor.ExecuteAsync<List<GetSchemaResult>>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string kql, string db) =>
            {
                if (kql.Contains("func2"))
                {
                    return new()
                    {
                        new() { ColumnName = "userName", ColumnType = "string", DataType = "System.String", },
                        new() { ColumnName = "userLastName", ColumnType = "string", DataType = "System.String", },
                    };
                }
                if (kql.Contains("func1"))
                {
                    return new()
                    {
                        new() { ColumnName = "name", ColumnType = "string", DataType = "System.String", }
                    };
                }
                return new();
            });
    }

    [Fact]
    public async Task GenerateAsync_ShouldCreateDbContextAndModels()
    {
        // Arrange & Act
        SetupProviderExecutor();
        var providerExecutor = _mockExecutor.Object;
        var ormGenerator = new ORMGenerator(new()
        {
            ProviderExecutor = providerExecutor,
            ModelsFolderPath = "Models",
            DbContextFolderPath = "../../../../Samples/ORMGeneratorTest/AutoGen",
            DbContextName = "AutoGenORMKustoDbContext",
            Namespace = "AutoGen",
            ModelsNamespace = "AutoGen",
            DbContextNamespace = "AutoGen",
            CreateDbContext = true,
            CleanFolderBeforeCreate = true,
            EnableNullable = true,
            FileScopedNamespaces = true,
            DatabaseConfigs = new List<ORMGeneratorDatabaseConfig>
            {
                new ORMGeneratorDatabaseConfig
                {
                    DatabaseName = DbName1,
                    Filters = new ORMGeneratorFilterConfig()
                },
                new ORMGeneratorDatabaseConfig
                {
                    DatabaseName = DbName2,
                    DatabaseDisplayName = "db2",
                    Filters = new ORMGeneratorFilterConfig()
                }
            }
        });
        await ormGenerator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(ormGenerator.Config.DbContextFilePath));
        Assert.True(Directory.GetFiles(ormGenerator.Config.ModelsFolderPath, "*.cs", SearchOption.AllDirectories).Any());
    }

    [E2EFact]
    public async Task GenerateAsync_ShouldCreateDbContextAndModelsE2E()
    {
        // Arrange & Act
        var providerExecutor = E2EHelper.Client;
        var ormGenerator = new ORMGenerator(new()
        {
            ProviderExecutor = providerExecutor,
            ModelsFolderPath = "Models",
            DbContextFolderPath = "../../../../Samples/ORMGeneratorTest/AutoGenE2E",
            DbContextName = "AutoGenORMKustoDbContextE2E",
            Namespace = "AutoGenE2E",
            ModelsNamespace = "AutoGenE2E",
            DbContextNamespace = "AutoGenE2E",
            CreateDbContext = true,
            CleanFolderBeforeCreate = true,
            EnableNullable = true,
            FileScopedNamespaces = true,
            DatabaseConfigs = new List<ORMGeneratorDatabaseConfig>
            {
                new ORMGeneratorDatabaseConfig
                {
                    DatabaseName = DbName1,
                    Filters = new ORMGeneratorFilterConfig()
                },
                new ORMGeneratorDatabaseConfig
                {
                    DatabaseName = DbName2,
                    DatabaseDisplayName = "db2",
                    Filters = new ORMGeneratorFilterConfig()
                }
            }
        });
        await ormGenerator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(ormGenerator.Config.DbContextFilePath));
        Assert.True(Directory.GetFiles(ormGenerator.Config.ModelsFolderPath, "*.cs", SearchOption.AllDirectories).Any());
    }

    [Fact]
    public void CsharpType_ShouldReturnCorrectCsharpType()
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var boolType = ormGenerator.CsharpType("bool");
        var stringType = ormGenerator.CsharpType("string");
        var intType = ormGenerator.CsharpType("int");

        // Assert
        Assert.Equal("bool?", boolType);
        Assert.Equal("string?", stringType);
        Assert.Equal("int?", intType);
    }

    [Fact]
    public void DataTypeTranslate_ShouldReturnCorrectKustoType()
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var stringType = ormGenerator.DataTypeTranslate("System.String");
        var intType = ormGenerator.DataTypeTranslate("System.Int32");
        var boolType = ormGenerator.DataTypeTranslate("System.Boolean");

        // Assert
        Assert.Equal("string?", stringType);
        Assert.Equal("int?", intType);
        Assert.Equal("bool?", boolType);
    }

    [Fact]
    public void DefaultValue_ShouldReturnCorrectDefaultValue()
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var boolValue = ormGenerator.DefaultValue("bool");
        var stringValue = ormGenerator.DefaultValue("string");
        var intValue = ormGenerator.DefaultValue("int");

        // Assert
        Assert.Equal("false", boolValue);
        Assert.Equal("''", stringValue);
        Assert.Equal("-1", intValue);
    }

    [Fact]
    public void GetModelName_ShouldReturnUniqueModelNames()
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var modelName1 = ormGenerator.GetModelName("TestModel");
        var modelName2 = ormGenerator.GetModelName("TestModel");

        // Assert
        Assert.Equal("TestModel", modelName1);
        Assert.Equal("TestModel1", modelName2);
    }

    [Fact]
    public void Match_ShouldReturnCorrectMatchResult()
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var isMatch1 = ormGenerator.Match("TestTable", "Test*");
        var isMatch2 = ormGenerator.Match("TestTable", "Test*able");
        var isMatch3 = ormGenerator.Match("TestTable", "Table");

        // Assert
        Assert.True(isMatch1);
        Assert.True(isMatch2);
        Assert.False(isMatch3);
    }

    [Fact]
    public async Task VerifyKqlValidityOnServer()
    {
        var e2eTesting = Environment.GetEnvironmentVariable("E2E_TESTING");
        if (e2eTesting == "1")
        {
            SetupProviderExecutor();
            var providerExecutor = _mockExecutor.Object;
            var ormGenerator = new ORMGenerator(new()
            {
                ProviderExecutor = providerExecutor,
                ModelsFolderPath = "Models",
                DbContextFolderPath = "../../../../Samples/ORMGeneratorTest/AutoGen",
                DbContextName = "AutoGenORMKustoDbContext",
                Namespace = "AutoGen",
                ModelsNamespace = "AutoGen",
                DbContextNamespace = "AutoGen",
                CreateDbContext = true,
                CleanFolderBeforeCreate = true,
                EnableNullable = true,
                FileScopedNamespaces = true,
                DatabaseConfigs = new List<ORMGeneratorDatabaseConfig>
                {
                    new ORMGeneratorDatabaseConfig
                    {
                        DatabaseName = DbName1,
                        Filters = new ORMGeneratorFilterConfig()
                    },
                    new ORMGeneratorDatabaseConfig
                    {
                        DatabaseName = DbName2,
                        DatabaseDisplayName = "db2",
                        Filters = new ORMGeneratorFilterConfig()
                    }
                }
            });
            await ormGenerator.GenerateAsync();

            Assert.True(File.Exists(ormGenerator.Config.DbContextFilePath));
            Assert.True(Directory.GetFiles(ormGenerator.Config.ModelsFolderPath, "*.cs", SearchOption.AllDirectories).Any());
        }
    }

    [Theory]
    [InlineData("Test*", "TestTable", true)]
    [InlineData("Test*able", "TestTable", true)]
    [InlineData("Table", "TestTable", false)]
    [InlineData("Test*", "Test", true)]
    [InlineData("*Table", "TestTable", true)]
    [InlineData("T*able", "TestTable", true)]
    [InlineData("*able", "TestTable", true)]
    [InlineData("*Test*", "TestTable", true)]
    [InlineData("TestTable", "TestTable", true)]
    [InlineData("*", "TestTable", true)]
    [InlineData("*", "", true)]
    [InlineData("Test*", "", false)]
    [InlineData("*able", "Table", true)]
    [InlineData("T*", "T", true)]
    [InlineData("T*e", "Te", true)]
    [InlineData("T*e", "TestTable", true)]
    [InlineData("TestTable*", "TestTableExtra", true)]
    [InlineData("Test*Extra", "TestTableExtra", true)]
    [InlineData("*Test*", "AnyTestString", true)]
    [InlineData("T*", "Tast", true)]
    [InlineData("Te*st", "Test", true)]
    [InlineData("Te*st", "TebTest", true)]
    [InlineData("Te*st", "Tast", false)]
    [InlineData("Te*st", "Teest", true)]
    [InlineData("**", "TestTable", true)]
    [InlineData("Te**le", "TestTable", true)]
    [InlineData("Te*", "TestTable", true)]
    [InlineData("Te*", "Te", true)]
    [InlineData("*le", "Table", true)]
    [InlineData("*st", "Test", true)]
    [InlineData("Tes*", "Test", true)]
    [InlineData("*Test*", "PrefixTestSuffix", true)]
    [InlineData("*Test*", "Test", true)]
    [InlineData("Test*", "Testing123", true)]
    [InlineData("TestTable", "DifferentTable", false)]
    [InlineData("Table*", "TestTable", false)]
    [InlineData("Test*Extra", "TestExtraExtra", true)]
    [InlineData("Te*Aable", "TebTable", false)]
    [InlineData("*Test", "TestTable", false)]
    [InlineData("*ble", "TestTable", true)]
    [InlineData("T*", "X", false)]
    [InlineData("Te*Zst", "TeTst", false)]
    [InlineData("TestTable", "TestTableExtra", false)]
    [InlineData("Test*", "Table", false)]
    public void GlobMatchTest(string pattern, string input, bool expected)
    {
        // Arrange & Act
        var ormGenerator = new ORMGenerator(new());
        var isMatch = ormGenerator.Match(input, pattern);
        // Assert
        Assert.Equal(expected, isMatch);
    }
}
