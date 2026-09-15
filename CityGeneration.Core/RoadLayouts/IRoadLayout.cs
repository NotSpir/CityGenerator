using CityGeneration.Core.RoadModels;

namespace CityGeneration.Core.RoadLayouts;

public interface IRoadLayout
{
    public void GenerateCity(ref RoadNetwork roadNetwork)
    {
        
    }


    public virtual void UpdateRoadNetwork(ref RoadNetwork roadNetwork)
    {
        
    }

    public virtual void SplitSectors(ref RoadNetwork roadNetwork, int iterations)
    {
        
    }
    
    public virtual void PlaceBuildings(ref RoadNetwork roadNetwork)
    {
        
    }
}