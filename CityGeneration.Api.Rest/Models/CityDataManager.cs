using CityGeneration.Core.BuildingModels;
using CityGeneration.Core.Data.DataModels;
using CityGeneration.Core.PathSearchModels;
using CityGeneration.Core.RoadLayouts;
using CityGeneration.Core.RoadModels;

namespace CityGeneration.Contracts.Rest.Entities;

public static class CityDataManager
{
    public static RoadPlacementData CreateAndRetrieveCityData(int seed, int width, int height, int splitIterations,  float blockChance, float deviation)
    {
        RoadNetwork roadNetwork =  new RoadNetwork();
        RectangleLayout rectangle = new RectangleLayout(seed, width, height, splitIterations, blockChance, deviation);
        rectangle.GenerateCity(ref roadNetwork);
        return new() { 
            VertexList = roadNetwork.VertexList, 
            RoadSegments = roadNetwork.RoadSegments, 
            VertexEdges = roadNetwork.VertexEdges, 
            Sectors = roadNetwork.Sectors
        };
    }
    
    public static RoadNetwork LoadRoadNetwork(RoadPlacementData networkData)
    {
        return new RoadNetwork(networkData.VertexList,  networkData.RoadSegments, networkData.VertexEdges, networkData.Sectors);
    }
    
    public static List<BuildingData> GetBuildingsInNetwork(RoadNetwork roadNetwork, int cellSize)
    {
        var buildings = BuildingSetter.SetBuildingDataInNetwork(ref roadNetwork, 8f, cellSize);
        return buildings;
    }

    public static List<int> GetPathBetweenTwoVIds(RoadNetwork roadNetwork, int start, int end)
    {
        return PathFinder.GetRouteFromVidToVid(roadNetwork, start, end);
    }
}