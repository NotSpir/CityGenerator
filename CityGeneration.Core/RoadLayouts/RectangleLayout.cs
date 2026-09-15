using CityGeneration.Core.Data.DataModels;
using CityGeneration.Core.RoadModels;

namespace CityGeneration.Core.RoadLayouts;

public class RectangleLayout:RoadLayout
{
    private readonly int _width;
    private readonly int _height;
    
    public RectangleLayout(int seed, int width, int height, int splitIterations,  float blockChance, float newDeviation)
    {
        Seed = seed;
        _width = width;
        _height = height;
        SplitIterations = splitIterations;
        BlockChance = blockChance;
        deviation = newDeviation;
    }


    public override void UpdateRoadNetwork(ref RoadNetwork roadNetwork)
    {
        CreateRectangleSector(ref roadNetwork, new Vector2(0, 0));
    }

    private void CreateRectangleSector(ref RoadNetwork roadNetwork, Vector2 center)
    {
        Vector2 a = new Vector2(center.X - _width, center.Y + _height);
        Vector2 b = new Vector2(center.X - _width, center.Y - _height);
        Vector2 c = new Vector2(center.X + _width, center.Y - _height);
        Vector2 d = new Vector2(center.X + _width, center.Y + _height);
        int aId = roadNetwork.AddVertex(a);
        int bId = roadNetwork.AddVertex(b);
        int cId = roadNetwork.AddVertex(c);
        int dId = roadNetwork.AddVertex(d);
        roadNetwork.AddSegmentToNetwork(aId, bId);
        roadNetwork.AddSegmentToNetwork(bId, cId);
        roadNetwork.AddSegmentToNetwork(cId, dId);
        roadNetwork.AddSegmentToNetwork(dId, aId);
        
        RoadSector sector = new RoadSector([], [aId, bId, cId, dId]);
        roadNetwork.ReconstructSectorFromVertexArray(ref sector);
        sector.Area = roadNetwork.CalculateSectorArea(sector.VertexList.ToArray());
        roadNetwork.AddSector(sector);
    }
}