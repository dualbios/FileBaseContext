using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileBaseContext.Tests.Data.Entities;

[Table("Places")]
public class Place
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Point Location { get; set; }
}