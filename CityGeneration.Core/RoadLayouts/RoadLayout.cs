using CityGeneration.Core.Data.DataModels;
using CityGeneration.Core.RoadModels;

namespace CityGeneration.Core.RoadLayouts;

public class RoadLayout : IRoadLayout
{
    protected int Seed = 0;
    protected int SplitIterations = 1;
    protected float BlockChance = 0.02f;
    protected float deviation = 0.0f;
    
    public void GenerateCity(ref RoadNetwork roadNetwork)
    {
        UpdateRoadNetwork(ref roadNetwork);
        SplitSectors(ref roadNetwork, SplitIterations);
        PlaceBuildings(ref roadNetwork);
    }

    public virtual void UpdateRoadNetwork(ref RoadNetwork roadNetwork)
    {
        
    }

    public void SplitSectors(ref RoadNetwork roadNetwork, int iterations)
    {
        int ci = 0;
        Random random = new Random(Seed);
        while (ci < iterations)
        {
            List<RoadSector> sectors = [];
            int sectCount = roadNetwork.GetSectorsCount();
            for (int sid = 0; sid < sectCount; sid++)
            {
                var sector = roadNetwork.GetSector(sid);
                if (!sector.Splittable) //If not splittable, keep as is
                {
                    sectors.Add(sector);
                    continue;
                }

                if (random.NextDouble() < BlockChance)
                {
                    sector.Splittable = false;
                    sectors.Add(sector);
                    continue;
                }
                
                float ranDeviation = (float)((0.5-deviation) + random.NextDouble() * deviation);
                var result = roadNetwork.SplitSectorLongestToOpposite(sid, ranDeviation);
                if (result == null)
                {
                    sector.Splittable = false;
                    sectors.Add(sector);
                    continue;
                }

                var sect1 = result.Value.Item1;
                var sect2 = result.Value.Item2;
                roadNetwork.ReconstructSectorFromVertexArray(ref sect1);
                roadNetwork.ReconstructSectorFromVertexArray(ref sect2);
                sectors.Add(sect1);
                sectors.Add(sect2);
            }
            roadNetwork.ReplaceSectors(sectors);
            ci++;
        }
    }

    public void PlaceBuildings(ref RoadNetwork roadNetwork)
    {
        
    }
}