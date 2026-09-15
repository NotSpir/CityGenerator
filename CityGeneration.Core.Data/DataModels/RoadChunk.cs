namespace CityGeneration.Core.Data.DataModels;

public struct RoadChunk
{
    private List<int> _edgeIds;
    private List<int> _verticeIds;

    public RoadChunk(List<int> edges, List<int> vertices)
    {
        _verticeIds = [];
        _edgeIds = [];
        _edgeIds.AddRange(edges);
        _verticeIds.AddRange(vertices);
    }
    public RoadChunk()
    {
        _verticeIds = [];
        _edgeIds = [];
    }

    public void AddEdge(int edgeId)
    {
        _edgeIds.Add(edgeId);
    }

    public void RemoveEdge(int edgeId)
    {
        _edgeIds.Remove(edgeId);
    }
    
    public void AddVertex(int vertId)
    {
        _verticeIds.Add(vertId);
    }
    
    public List<int> GetEdges()
    {
        return _edgeIds;
    }

    public List<int> GetVertices()
    {
        return _verticeIds;
    }
}