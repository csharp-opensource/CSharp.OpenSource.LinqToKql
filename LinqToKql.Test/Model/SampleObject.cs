using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
    [JsonProperty("prop_newtonsoft")]
    public long PropNewtonsoft { get; set; }
    [JsonPropertyName("prop_text_json")]
    public long PropTextJson { get; set; }
    [DataMember(Name = "prop_data_member")]
    public long PropDataMember { get; set; }
}
