namespace CSharp.OpenSource.LinqToKql.Test.Model;

public class SampleObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public DateOnly DateOnly { get; set; }
    public TimeOnly TimeOnly { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsActive { get; set; }
    public int Year { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public long Value { get; set; }
    public List<long> Numbers { get; set; }
    public SampleObject2 Nested { get; set; }
}
