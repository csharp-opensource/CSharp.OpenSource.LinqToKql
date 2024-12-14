using CSharp.OpenSource.LinqToKql.Extensions;

namespace CSharp.OpenSource.LinqToKql.Tests;

public class ObjectExtensionTests
{
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void GetKQLValue_Bool_ReturnsCorrectString(bool input, string expected)
    {
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_TimeOnly_ReturnsCorrectString()
    {
        var input = new TimeOnly(14, 30, 15, 123);
        var expected = "timespan(14:30:15.1)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_TimeSpan_ReturnsCorrectString()
    {
        var input = new TimeSpan(1, 2, 3, 4);
        var expected = "timespan(1.02:03:04)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_DateTime_ReturnsCorrectString()
    {
        var input = new DateTime(2023, 10, 5, 14, 30, 15, 123);
        var expected = "datetime(2023-10-05 14:30:15.1)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_DateOnly_ReturnsCorrectString()
    {
        var input = new DateOnly(2023, 10, 5);
        var expected = "datetime(2023-10-05)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_String_ReturnsCorrectString()
    {
        var input = "test'string";
        var expected = "'test\\'string'";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_IEnumerable_ReturnsCorrectString()
    {
        var input = new List<int> { 1, 2, 3 };
        var expected = "(1, 2, 3)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_Null_ReturnsNullString()
    {
        object? input = null;
        var expected = "dynamic(null)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_OtherTypes_ReturnsToString()
    {
        var input = 123;
        var expected = "123";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_ListOfStrings_ReturnsCorrectString()
    {
        var input = new List<string> { "one", "two", "three" };
        var expected = "('one', 'two', 'three')";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKQLValue_ListOfMixedTypes_ReturnsCorrectString()
    {
        var input = new List<object> { 1, "two", true };
        var expected = "(1, 'two', true)";
        var result = input.GetKQLValue();
        Assert.Equal(expected, result);
    }
}