using System.Formats.Asn1;
using System.Net.Http.Headers;
using CityGeneration.Core.Data.DataModels;
using CityGeneration.Core.RoadModels;

namespace CityGeneration.Core.BuildingModels;

public static class BuildingSetter
{
    public static List<BuildingData> SetBuildingDataInNetwork(ref RoadNetwork roadNetwork, float roadOffset, float lotCellSize)
    {
        var buildingDataList = new List<BuildingData>();
        var sectors = roadNetwork.GetAllSectors();
        foreach (var sector in sectors)
        {
            if (sector.VertexList.Count < 3) continue;
            
            var lotAndCentroid = CreateLotFromSector(ref roadNetwork, sector, roadOffset);
            if (lotAndCentroid == null) continue;
            var lot = lotAndCentroid.Value.Item1;
            var centroid = lotAndCentroid.Value.Item2;
            var newBuildings = BreakSectorIntoGrid(lot, lotCellSize, centroid);
            buildingDataList.AddRange(newBuildings);
        }

        return buildingDataList;
    }

    private static (Vector2[], Vector2)? CreateLotFromSector(ref RoadNetwork roadNetwork, RoadSector sector, float roadOffset)
    {
        if (sector.Type == "Blocked") return null;
        
        var verts = new List<Vector2>();
        foreach (var vid in sector.VertexList)
        {
            verts.Add(roadNetwork.GetVertex(vid));
        }
        verts = CleanUpSector(verts);
        if (verts.Count < 3) return null;
        Vector2 centroid = GetArithmeticCentroid(verts);
        verts = GetSectorInsideOffset(verts, roadOffset);
        if (verts.Count < 3) return null;
        //Check convex
        return (verts.ToArray(), centroid);
    }

    private static List<Vector2> CleanUpSector(List<Vector2> verts)
    {
        int i = 0;
        List<Vector2> cleanVertices = [];
        while (i < verts.Count)
        {
            Vector2 v = verts[i];
            Vector2 nextV = verts[(i+1)%verts.Count];
            Vector2 prevV;
            if (i == 0) prevV = verts[^1];
            else prevV = verts[(i-1)%verts.Count];
            bool isExtra = Vector2.IsPointOnSegment(v, prevV, nextV);
            isExtra = Vector2.IsPointOnSegment(prevV, v, nextV);
            if (!isExtra) cleanVertices.Add(v);
            i++;
        }
        return cleanVertices;
    }

    private static Vector2 GetArithmeticCentroid(List<Vector2> verts)
    {
        Vector2 center = new Vector2(0, 0);
        if (verts.Count < 3) return center;
        foreach (var v in verts) center += v;
        return center/verts.Count;
    }

    private static Vector2 GetGeometricCentroid(List<Vector2> verts)
    {
        Vector2 center = new Vector2(0f, 0f);
        if (verts.Count < 3) return center;
        float signedArea = 0f;
        for (int i = 0; i < verts.Count; i++)
        {
            var a = verts[i];
            var b = verts[(i+1)%verts.Count];
            var cross = a.X * b.X + a.Y * b.Y;
            signedArea += cross;
            center += (a+b)*cross;
        }

        signedArea *= .5f;
        if (signedArea == 0f) return GetArithmeticCentroid(verts);
        center /= 6*signedArea;
        return center;
    }

    private static bool IsPointInPolygon(List<Vector2> verts, Vector2 point)
    {
        float maxX = verts.Max(x => x.X);
        float maxY = verts.Max(x => x.Y);
        float minX = verts.Min(x => x.X);
        float minY = verts.Min(x => x.Y);
        Vector2 right = new Vector2(maxX + 10, point.Y);
        Vector2 left = new Vector2(minX - 10, point.Y);
        Vector2 down = new Vector2(point.X, maxY + 10);
        Vector2 up = new Vector2(point.X, minY - 10);
        int rightCrossed = 0;
        int leftCrossed = 0;
        int downCrossed = 0;
        int upCrossed = 0;
        
        for (int i = 0; i < verts.Count; i++)
        {
            var a = verts[i];
            var b = verts[(i+1)%verts.Count];
            if (Vector2.GetSegmentIntersection(a, b, point, right) != null) rightCrossed++;
            if (Vector2.GetSegmentIntersection(a, b, point, down) != null) downCrossed++;
            if (Vector2.GetSegmentIntersection(a, b, point, left) != null) leftCrossed++;
            if (Vector2.GetSegmentIntersection(a, b, point, up) != null) upCrossed++;
        }
        if (rightCrossed%2 == 0 || leftCrossed%2 == 0 || 
            upCrossed%2 == 0 || downCrossed%2 == 0) return false; 
        return true;
    }

    private static List<Vector2> GetSectorInsideOffset(List<Vector2> verts, float offset)
    {
        var newVerts = new List<Vector2>();
        var rotationValue = -MathF.PI / 2;
        var vS = verts[0];
        var vE = verts[1];
        var dir = (vE - vS).Normalized();
        var off = dir * offset;
        var testPos = vS + off + dir.Rotate(rotationValue) * offset;
        if (!IsPointInPolygon(verts, testPos))
        {
            rotationValue = MathF.PI / 2;
            testPos = vS + off + dir.Rotate(rotationValue) * offset;
            if (!IsPointInPolygon(verts, testPos))
            {
                rotationValue = -MathF.PI / 2;
                vS = verts[1];
                vE = verts[2];
                dir = (vE - vS).Normalized();
                off = dir * offset;
                testPos = vS + off + dir.Rotate(rotationValue) * offset;
                if (!IsPointInPolygon(verts, testPos))
                {
                    rotationValue = MathF.PI / 2;
                    testPos = vS + off + dir.Rotate(rotationValue) * offset;
                    if (!IsPointInPolygon(verts, testPos))
                    {
                        {
                            return [];
                        }
                    }
                }
            }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            var v = verts[i];
            var v2 = verts[(i+1)%verts.Count];
            Vector2 v0;
            if (i == 0) v0 = verts[^1];
            else v0 = verts[(i-1)%verts.Count];
            var vV0 = (v0-v).Normalized();
            var vV2 = (v2-v).Normalized();
            float angle = Vector2.AngleBetweenVectors(vV0, vV2);
            var offsetVec = offset*(1/MathF.Tan(angle*.5f)) * vV2;
            var sideTurn = offset*vV2.Rotate(rotationValue);
            var newPos = v + offsetVec + sideTurn;
            var sOff = vV2 * offset;
            //var newPos = v1 + off + dir.Rotate(rotationValue) * offset;
            newVerts.Add(newPos);
        }

        if (newVerts.Count < 3) return [];
        newVerts = GetSectorWithoutSelfIntersections(newVerts);
        return newVerts;
    }

    private static List<Vector2> GetSectorWithoutSelfIntersections(List<Vector2> verts)
    {
        List<Vector2> newVerts = [];
        List<Vector2> skippedVerts = [];
        for (int i = 0; i < verts.Count; i++)
        {
            bool skip = false;
            var v = verts[i];
            int ni = (i + 1) % verts.Count;
            var v2 = verts[ni];
            if (skippedVerts.Contains(v)) continue;
            if (skippedVerts.Contains(v2)) continue;
            for (int j = 0; j < verts.Count; j++)
            {
                int nj = (j + 1) % verts.Count;
                if (j == i || j == ni ||
                    nj == i || nj == ni) continue;
                var sv = verts[j];
                var sv2 = verts[nj];
                if (skippedVerts.Contains(sv)) continue;
                if (skippedVerts.Contains(sv2)) continue;
                if (Vector2.GetSegmentIntersection(v, v2, sv, sv2) != null)
                {
                    skip = true;
                    break;
                }
            }

            if (skip)
            {
                skippedVerts.Add(v);
                continue;
            }
            newVerts.Add(v);
        }
        if (newVerts.Count < 3) return [];
        return newVerts;
    }

    private static List<BuildingData> BreakSectorIntoGrid(Vector2[] lot, float cellSize, Vector2 centroid)
    {
        
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Vector2 v in lot)
        {
            if (v.X > maxX) maxX = v.X;
            if (v.X < minX) minX = v.X;
            if (v.Y > maxY) maxY = v.Y;
            if (v.Y < minY) minY = v.Y;
        }
        
        int w = (int)MathF.Ceiling((maxX - minX) / cellSize);
        int h = (int)MathF.Ceiling((maxY - minY) / cellSize);
        int[][] grid = new int[h][];
        for (int i = 0; i < h; i++)
        {
            int[] row = new int[w];
            for (int j = 0; j < w; j++)
            {
                row[j] = 1;
                Vector2 a = new Vector2(minX + j * cellSize, minY + i * cellSize);
                Vector2 b = new Vector2(minX + j * cellSize, minY + i * cellSize - cellSize);
                Vector2 c = new Vector2(minX + j * cellSize + cellSize, minY + i * cellSize);
                Vector2 d = new Vector2(minX + j * cellSize + cellSize, minY + i * cellSize - cellSize);
                Vector2 centroidSquare = new(
                    minX + j*cellSize + cellSize / 2f,
                    minY + i*cellSize - cellSize / 2f);
                for (int vi = 0; vi < lot.Length; vi++)
                {
                    Vector2 v = lot[vi];
                    Vector2 nv = lot[(vi + 1) % lot.Length];
                    
                    bool flag1 = Vector2.GetSegmentIntersection(a, b, v, nv) != null;
                    bool flag2 = Vector2.GetSegmentIntersection(a, c, v, nv) != null;
                    bool flag3 = Vector2.GetSegmentIntersection(c, d, v, nv) != null;
                    bool flag4 = Vector2.GetSegmentIntersection(b, d, v, nv) != null;
                    bool flagC = Vector2.GetSegmentIntersection(centroidSquare, centroid, v, nv) != null;

                    if (flag1 || flag2 || flag3 || flag4 || flagC)
                    {
                        row[j] = 0;
                        break;
                    }
                }
            }

            grid[i] = row;
        }
        return BreakGridIntoBuildings(grid, minX, minY, cellSize);
    }

    private static List<BuildingData> BreakGridIntoBuildings(int[][] grid, float minX, float minY, float cellSize)
    {
        List<BuildingData> buildings = [];
        int buildNum = 2;

        for (int r = 0; r < grid.Length; r++)
            for (int c = 0; c < grid[r].Length; c++)
            {
                if (grid[r][c] != 1) continue;
                float minC = c * cellSize;
                float topR = r * cellSize;
                float cDist = cellSize;
                float rDist = -cellSize;

                int step = SetSquareBuilding(grid, r, c, buildNum);
                if (step == 0)
                    continue;

                topR = (r + step - 1) * cellSize;
                cDist *= step;
                rDist *= step;
                var a1 = new Vector2(minX+minC, minY+topR); // 0 0 Upper Left
                var a2 = new Vector2(minX+minC, minY+topR+rDist); // 0 -1 Down Left
                var a3 = new Vector2(minX+minC+cDist, minY+topR+rDist); // 1 -1 Down Right
                var a4 = new Vector2(minX+minC+cDist, minY+topR); // 1 0 Upper Right
                buildNum++;
                BuildingData newBuilding = new([a1, a2, a3, a4]);
                buildings.Add(newBuilding);
            }
        
        return buildings;
    }

    private static int SetSquareBuilding(int[][] grid, int row, int col, int buildNum)
    {
        int step = 1;

        while (true)
        {
            if (step >= 12) break; //Limit size
            int r = row + step;
            int c = col + step;
            if (r >= grid.Length) break;
            if (c >= grid[row].Length) break;
            if (grid[r][c] != 1) break;
            
            bool canExpand = true;
            for (int i = 0; i < step; i++)
            {
                bool a = grid[r][col + i] == 1;
                bool b = grid[row+i][c] == 1;
                if (!a || !b)
                {
                    canExpand = false;
                    break;
                }
            }

            if (!canExpand) break;
            step++;
        }

        if (step <= 3)
        {
            return 0;
        }
        grid[row][col] = buildNum;
        for (int s = 0; s < step; s++)
        {
            int r = row + s;
            int c = col + s;
            for (int i = 0; i < s; i++)
            {
                grid[r][col+i] = buildNum;
                grid[row+i][c] = buildNum;
            }
            grid[r][c] = buildNum;
        }

        for (int r = row - 1; r < row + step + 1; r++)
            for (int c = col - 1; c < col + step + 1; c++)
            {
                if (r < 0 || r >= grid.Length || c < 0 || c >= grid[row].Length) continue;
                if (r >= row && r < row+step && c >= col && c < col+step) continue;
                grid[r][c] = -1;
            }
        
        return step;
    }
}