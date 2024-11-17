using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.OpenSource.LinqToKql.Test.Model;

public class SampleObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public long Value { get; set; }
}
