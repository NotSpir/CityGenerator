using CityGeneration.Core.Data.DataModels;

namespace CityGeneration.Contracts.Rest.Entities;

public record struct RoadPlacementData()
{
    public List<Vector2> VertexList { get; set; }
    public List<RoadSegment> RoadSegments { get; set; }
    public Dictionary<int, List<int>> VertexEdges { get; set; }
    public List<RoadSector> Sectors { get; set;  }
    public int cellSize = 4;
}