namespace CityGeneration.Core.PathSearchModels;

public struct ASearchCandidate
{
    public int VId;
    public float Distance;
    public int? Prev;
}