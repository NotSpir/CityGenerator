namespace CityGeneration.Core.Data.DataModels;

public struct RoadSector
{
    public List<int> Edges { get; set; }
    public List<int> VertexList { get; set; }
    public float Area;

    public bool Splittable = true;
    public string Type;

    public RoadSector()
    {
        Edges = [];
        VertexList = [];
        Area = 0;
        Splittable = true;
        Type = "Normal";
    }
    
    public RoadSector(List<int> edges, List<int> vertices, float newArea = 0, string type = "Normal")
    {
        Edges = edges;
        VertexList = vertices;
        Area = newArea;
        Splittable = true;
        Type = type;
    }
}