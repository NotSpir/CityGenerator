namespace CityGeneration.Core.Data.DataModels;

public struct RoadSegment(int startId, int endId, string type = "Full")
{
    public int StartId {get; set; } = startId;
    public int EndId {get; set; } = endId;
    public string Type {get; set; }  = type;
    
    public void SetType(string type)
    {
        Type = type;
    }
}