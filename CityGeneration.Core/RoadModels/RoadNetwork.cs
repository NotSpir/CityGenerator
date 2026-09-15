using System.ComponentModel.DataAnnotations;
using CityGeneration.Core.Data.DataModels;

namespace CityGeneration.Core.RoadModels;

public class RoadNetwork
{
    public readonly Dictionary<int, List<int>> VertexEdges;
    public readonly List<Vector2> VertexList;
    public readonly List<RoadSegment> RoadSegments;
    public List<RoadSector> Sectors;
    private Dictionary<Vector2, RoadChunk> _chunks;
    
    const float ChunkSize = 128f;
    private const float VertexEpsilon = 1.0f;

    public RoadNetwork(List<Vector2> vertexList, List<RoadSegment> roadSegments, Dictionary<int, List<int>> vertexEdges, List<RoadSector> sectors)
    {
        VertexList = vertexList;
        RoadSegments = roadSegments;
        VertexEdges =  vertexEdges;
        Sectors = sectors;
        _chunks = [];
    }
    
    public RoadNetwork()
    {
        VertexList = [];
        RoadSegments = [];
        VertexEdges = new Dictionary<int, List<int>>();
        Sectors = [];
        _chunks = [];
    }
    
    public int AddVertex(Vector2 vertex)
    {
        var chunkPos = CreateChunkIfNotExists(vertex);
        foreach (var vId in _chunks[chunkPos].GetVertices())
        {
            if ((VertexList[vId] - vertex).Length() <= VertexEpsilon)
            {
                return vId;
            }
        }
        
        VertexList.Add(vertex);
        int newVId = VertexList.Count - 1;
        return newVId;
    }

    public Vector2 GetVertex(int Id)
    {
        return VertexList[Id];
    }
    
    public int GetLastVertexId()
    {
        int vId = VertexList.Count - 1;
        return vId;
    }

    public int AddSegment(int startId, int endId, string type = "Full")
    {
        RoadSegments.Add(new RoadSegment(startId, endId, type));
        int eId = RoadSegments.Count - 1;
        return eId;
    }
    
    public int AddSegment(RoadSegment segment)
    {
        RoadSegments.Add(segment);
        int eId = RoadSegments.Count - 1;
        return eId;
    }
    
    public RoadSegment GetSegment(int Id)
    {
        return RoadSegments[Id];
    }
    
    public (RoadSegment, RoadSegment) SplitSegment(int segmentIndex, int splittingVertexId)
    {
        RoadSegment originalSegment = RoadSegments[segmentIndex];
        if (originalSegment.Type == "Split") RoadSegments[segmentIndex].SetType("Old-Split");
        else RoadSegments[segmentIndex].SetType("Old-Full");
        RoadSegment newSegment1 = new RoadSegment(originalSegment.StartId, splittingVertexId, "Split");
        RoadSegment newSegment2 = new RoadSegment(splittingVertexId, originalSegment.EndId, "Split");
        return (newSegment1, newSegment2);
    }

    public Vector2 CreateChunkIfNotExists(Vector2 worldPosition)
    {
        Vector2 chunkPosition = WorldToChunkPosition(worldPosition);
        return CreateChunkIfNotExistsAtChunkPos(chunkPosition);
    }
    
    public Vector2 CreateChunkIfNotExistsAtChunkPos(Vector2 chunkPosition)
    {
        if (_chunks.ContainsKey(chunkPosition)) return chunkPosition;
        RoadChunk newChunk = new RoadChunk();
        _chunks.Add(chunkPosition, newChunk);
        return chunkPosition;
    }

    public Vector2 WorldToChunkPosition(Vector2 worldPosition)
    {
        float chunkX = MathF.Round(worldPosition.X/ChunkSize);
        float chunkY = MathF.Round(worldPosition.Y/ChunkSize);
        Vector2 chunkPosition = new Vector2(chunkX, chunkY);
        return chunkPosition;
    }

    private void AddSegmentToChunk(Vector2 chunkPosition, int segmentId)
    {
        _chunks[chunkPosition].AddEdge(segmentId);
    }
    
    public int[] GetAllSegmentsInChunk(Vector2 chunkPosition)
    {
        return _chunks[chunkPosition].GetEdges().ToArray();
    }
    
    public int[] GetAllVerticesInChunk(Vector2 chunkPosition)
    {
        return _chunks[chunkPosition].GetVertices().ToArray();
    }

    public void RemoveSegmentInChunk(Vector2 chunkPosition, int edgeId)
    {
        _chunks[chunkPosition].RemoveEdge(edgeId);
    }

    public int AddSegmentToNetwork(RoadSegment segment)
    {
        int segmentId = AddSegment(segment);
        AddSegmentToVertexGraph(segmentId);
        Vector2 startV =  VertexList[segment.StartId];
        Vector2 endV =  VertexList[segment.EndId];
        Vector2[] crossedChunks = GetAllCrossedChunks(startV, endV);
        foreach (Vector2 chunk in crossedChunks)
        {
            CreateChunkIfNotExistsAtChunkPos(chunk);
            AddSegmentToChunk(chunk, segmentId);
        }
        return segmentId;
    }
    
    public int AddSegmentToNetwork(int startVerticeId, int endVerticeId, string type = "Full")
    {
        int segmentId = AddSegment(startVerticeId, endVerticeId, type);
        AddSegmentToVertexGraph(segmentId);
        Vector2 startV =  VertexList[startVerticeId];
        Vector2 endV =  VertexList[endVerticeId];
        Vector2[] crossedChunks = GetAllCrossedChunks(startV, endV);
        foreach (Vector2 chunk in crossedChunks)
        {
            CreateChunkIfNotExistsAtChunkPos(chunk);
            AddSegmentToChunk(chunk, segmentId);
        }
        return segmentId;
    }

    private int[] GetSegmentsFromVertex(int vertexId)
    {
        return VertexEdges[vertexId].ToArray();
    }
    
    private void AddSegmentToVertexGraph(int segmentId)
    {
        RoadSegment segment = RoadSegments[segmentId];
        int startVId = segment.StartId;
        int endVId = segment.EndId;
        if (!VertexEdges.ContainsKey(startVId)) VertexEdges.Add(startVId, []);
        if (!VertexEdges.ContainsKey(endVId)) VertexEdges.Add(endVId, []);
        VertexEdges[startVId].Add(segmentId);
        VertexEdges[endVId].Add(segmentId);
    }
    private void RemoveSegmentInVertexGraph(int segmentId)
    {
        RoadSegment segment = RoadSegments[segmentId];
        int startVId = segment.StartId;
        int endVId = segment.EndId;
        if (VertexEdges.ContainsKey(startVId)) VertexEdges[startVId].Remove(segmentId);
        if (VertexEdges.ContainsKey(endVId)) VertexEdges[endVId].Remove(segmentId);
    }

    private (int, int) SplitSegmentToNetwork(int segmentId, int splittingVertexId)
    {
        RoadSegment s = RoadSegments[segmentId];
        var newSegments = SplitSegment(segmentId, splittingVertexId);
        RoadSegment s1 = newSegments.Item1;
        RoadSegment s2 = newSegments.Item2;
        Vector2 startV =  VertexList[s.StartId];
        Vector2 endV =  VertexList[s.EndId];
        Vector2[] crossedChunks = [];
        foreach (Vector2 chunk in crossedChunks)
        {
            CreateChunkIfNotExistsAtChunkPos(chunk);
            RemoveSegmentInChunk(chunk, segmentId);
        }
        int segmentId1 = AddSegmentToNetwork(s1);
        int segmentId2 = AddSegmentToNetwork(s2);
        return (segmentId1, segmentId2);
    }
    
    //Sector hell begins here
    public int AddSector(RoadSector sector)
    {
        Sectors.Add(sector);
        return Sectors.Count - 1;
    }

    public int GetSectorsCount()
    {
        return Sectors.Count;
    }
    
    public RoadSector[] GetAllSectors()
    {
        return Sectors.ToArray();
    }
    
    public RoadSector GetSector(int id)
    {
        return Sectors[id];
    }
    
    public void ReplaceSectors(List<RoadSector> sectors)
    {
        Sectors = sectors;
    }
    public float CalculateSectorArea(int[] vertexIds)
    {
        float area = 0f;
        for (int i = 0; i < vertexIds.Length; i++)
        {
            int j = (i+1) % vertexIds.Length;
            Vector2 v1 = VertexList[vertexIds[i]];
            Vector2 v2 = VertexList[vertexIds[j]];
            area += v1.X*v2.Y - v1.Y*v2.X;
        }
        return area*.5f;
    }

    public (RoadSector, RoadSector)? SplitSectorLongestToOpposite(int sectorId, float deviation = 0, bool checkCrossing = false, int retries = 0)
    {
        RoadSector sector = Sectors[sectorId];
        int[] segments = sector.Edges.ToArray();
        int longestIIdx = 0;
        int longestIdx = 0;
        float longestLength = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            int eId = segments[i];
            RoadSegment segment = RoadSegments[eId];
            Vector2 subStartV = VertexList[segment.StartId];
            Vector2 subEndV = VertexList[segment.EndId];
            float length = (subEndV - subStartV).Length();
            if (length > longestLength)
            {
                longestIIdx = i;
                longestIdx = eId;
                longestLength = length;
            }
        }
        var longestSegment = RoadSegments[longestIdx];
        Vector2 longStartV = VertexList[longestSegment.StartId];
        Vector2 longEndV = VertexList[longestSegment.EndId];
        Vector2 midPoint = Vector2.Lerp(longStartV, longEndV, .5f);
        
        int oppositeIIdx = (longestIIdx + (int)Math.Floor(segments.Length / 2.0f)) % segments.Length;
        int oppositeIdx = segments[oppositeIIdx];
        var oppositeSegment = RoadSegments[oppositeIdx];
        Vector2 oppositeStartV = VertexList[oppositeSegment.StartId];
        Vector2 oppositeEndV = VertexList[oppositeSegment.EndId];
        var oppositeSegmentPoint = Vector2.Lerp(oppositeStartV, oppositeEndV, deviation);

        if (!checkCrossing) return SplitSectorByTwoPoints(sectorId, midPoint, oppositeSegmentPoint);

        int attempts = retries;
        while (attempts > 0)
        {
            bool success = true;
            foreach (var eid in segments)
            {
                if (eid == longestIdx || eid == oppositeIdx) continue;
                RoadSegment s = RoadSegments[eid];
                Vector2 startV = VertexList[s.StartId];
                Vector2 endV = VertexList[s.EndId];
                Vector2? intersection = Vector2.GetSegmentIntersection(midPoint, oppositeSegmentPoint, startV, endV);
                if (intersection.HasValue)
                {
                    success = false;
                }
            }

            if (success) break;
            oppositeIIdx++;
            oppositeIIdx = oppositeIIdx % segments.Length;
            oppositeIdx = segments[oppositeIIdx];
            oppositeSegment = RoadSegments[oppositeIdx];
            oppositeSegmentPoint = (oppositeStartV + oppositeEndV) * deviation;
            attempts--;
        }
        return SplitSectorByTwoPoints(sectorId, midPoint, oppositeSegmentPoint);
    }

    (RoadSector, RoadSector)? SplitSectorByTwoPoints(int sid, Vector2 point1, Vector2 point2)
    {
        var vId1 = AddVertex(point1);
        var vId2 = AddVertex(point2);
        AddSegmentToNetwork(vId1, vId2);
        var sector = Sectors[sid];
        var sVertexArray = sector.VertexList.ToArray();
        var sSegmentArray = sector.Edges.ToArray();

        int insert_pos1 = -1;
        int insert_pos2 = -1;
        for (int i = 0; i < sSegmentArray.Length; i++)
        {
            int eId = sSegmentArray[i];
            if (IsVertexOnEdge(vId1, eId) && insert_pos1 == -1)
            {
                SplitSegmentToNetwork(eId, vId1);
                insert_pos1 = i+1;
            }
            if (IsVertexOnEdge(vId2, eId) && insert_pos2 == -1)
            {
                SplitSegmentToNetwork(eId, vId2);
                insert_pos2 = i+1;
            }
        }

        if (insert_pos1 == -1)
        {
            for (int i = 0; i < sVertexArray.Length; i++)
            {
                if (sVertexArray[i] == vId1)
                {
                    insert_pos1 = i+1;
                    break;
                }
            }
        }
        if (insert_pos2 == -1)
        {
            for (int i = 0; i < sVertexArray.Length; i++)
            {
                if (sVertexArray[i] == vId2)
                {
                    insert_pos2 = i+1;
                    break;
                }
            }
        }

        if (insert_pos1 == -1 || insert_pos2 == -1) 
            return null;
        
        int[] newVertexArray = new int[sVertexArray.Length + 2];
        int j = 0;
        for (int i = 0; i < newVertexArray.Length; i++)
        {
            if (insert_pos1 == i)
            {
                insert_pos2++;
                newVertexArray[i] = vId1;
                continue;
            }
            if (insert_pos2 == i)
            {
                insert_pos1++;
                newVertexArray[i] = vId2;
                continue;
            }
            newVertexArray[i] = sVertexArray[j++];
        }
        
        RoadSector newSector1 = new RoadSector();
        RoadSector newSector2 = new RoadSector();

        int idx = 0;
        int swap_idx = -1;
        int last_idx = -1;
        while (idx < newVertexArray.Length)
        {
            if (idx == insert_pos1 && swap_idx == -1)
            {
                idx = insert_pos2;
                swap_idx = insert_pos1;
                last_idx = insert_pos2;
                continue;
            }
            if (idx == insert_pos2 && swap_idx == -1)
            {
                idx = insert_pos1;
                swap_idx = insert_pos2;
                last_idx = insert_pos1;
                continue;
            }
            newSector1.VertexList.Add(newVertexArray[idx++]);
        }

        idx = swap_idx-1;
        while (idx <= last_idx)
        {
            newSector2.VertexList.Add(newVertexArray[idx++]);
        }
        newSector1.Area = CalculateSectorArea(newSector1.VertexList.ToArray());
        newSector2.Area = CalculateSectorArea(newSector2.VertexList.ToArray());
        return (newSector1, newSector2);
    }

    public void ReconstructSectorFromVertexArray(ref RoadSector sector)
    {
        for (int i = 0; i < sector.VertexList.Count; i++)
        {
            int vId =  sector.VertexList[i];
            int nextVId =  sector.VertexList[(i+1)%sector.VertexList.Count];
            var vEdges = VertexEdges[vId];
            bool foundEdge = false;
            foreach (var eId in vEdges)
            {
                RoadSegment edge = RoadSegments[eId];
                if (edge.StartId == nextVId || edge.EndId == nextVId)
                {
                    sector.Edges.Add(eId);
                    foundEdge = true;
                    break;
                }
            }

            if (foundEdge) continue;
        }
    }
    
    bool IsVertexOnEdge(int vid, int eid)
    {
        var vert = VertexList[vid];
        var segm = RoadSegments[eid];
        var startV = VertexList[segm.StartId];
        var endV = VertexList[segm.EndId];
        return Vector2.IsPointOnSegment(vert, endV, startV);
    }

    Vector2[] GetAllCrossedChunks(Vector2 start, Vector2 end)
    {
        List<Vector2> crossedChunks = [];
        Vector2 startChunk = WorldToChunkPosition(start);
        Vector2 endChunk = WorldToChunkPosition(end);

        List<Vector2> cells = [];
        int xStep = 1;
        int yStep = 1;
        float error;
        float errorPrev;
        float x = start.X;
        float y = start.Y;
        float ddy;
        float ddx;
        float dx = end.X - x;
        float dy = end.Y - y;
        if (dy<0)
        {
            yStep = -1;
            dy = -dy;
        }    
        if (dx<0)
        {
            xStep = -1;
            dx = -dx;
        }

        ddx = 2 * dx;
        ddy = 2 * dy;
        if (ddx > ddy)
        {
            error = dx;
            errorPrev = dx;
            for (int i = 0; i < dx; i++)
            {
                x += xStep;
                error += ddy;
                if (error > ddx)
                {
                    y += yStep;
                    error -= ddx;
                    if (error+errorPrev < ddx) cells.Add(new Vector2(x, y-yStep));
                    else if (error+errorPrev > ddx) cells.Add(new Vector2(x-xStep, y));
                    else
                    {
                        cells.Add(new Vector2(x, y-yStep));
                        cells.Add(new Vector2(x-xStep, y));
                    }
                } 
                cells.Add(new Vector2(x, y));
                errorPrev = error;
            }
        }
        else
        {
            error = dy;
            errorPrev = dy;
            for (int i = 0; i < dy; i++)
            {
                x += yStep;
                error += ddx;
                if (error > ddy)
                {
                    x += xStep;
                    error -= ddy;
                    if (error+errorPrev < ddy) cells.Add(new Vector2(x-xStep, y));
                    else if (error+errorPrev > ddy) cells.Add(new Vector2(x, y-yStep));
                    else
                    {
                        cells.Add(new Vector2(x, y-yStep));
                        cells.Add(new Vector2(x-xStep, y));
                    }
                } 
                cells.Add(new Vector2(x, y));
                errorPrev = error;
            }
        }
        foreach (var c in cells)
        {
            var chunk = WorldToChunkPosition(c);
            if (!crossedChunks.Contains(chunk)) crossedChunks.Add(chunk);
        }
        return crossedChunks.ToArray();
    }
}