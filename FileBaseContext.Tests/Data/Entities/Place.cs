using NetTopologySuite.Geometries;

namespace FileBaseContext.Tests.Data.Entities;

public class Place
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Point Location { get; set; }
}