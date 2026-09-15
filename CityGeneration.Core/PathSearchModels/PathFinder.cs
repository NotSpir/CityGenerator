using CityGeneration.Core.Data.DataModels;
using CityGeneration.Core.RoadModels;

namespace CityGeneration.Core.PathSearchModels;

public static class PathFinder
{
    public static List<int> GetRouteFromVidToVid(RoadNetwork roadNetwork, int svid, int evid)
    {
        if (evid >= roadNetwork.VertexList.Count) return new List<int>();
        if (evid < 0) return new List<int>();
        if (svid >= roadNetwork.VertexList.Count) return new List<int>();
        if (svid < 0) return new List<int>();
        if (svid == evid) return new List<int> { svid };

        var checkedVids = new Dictionary<int, ASearchCheckedInfo>();
        var candidates = new List<ASearchCandidate>
        {
            new ASearchCandidate { VId = svid, Distance = 0.0f, Prev = null }
        };

        while (candidates.Count > 0)
        {
            candidates.Sort((a, b) => b.Distance.CompareTo(a.Distance));

            var vinfo = candidates[candidates.Count - 1];
            candidates.RemoveAt(candidates.Count - 1);

            int vid = vinfo.VId;
            int? prev = vinfo.Prev;
            Vector2 vert = roadNetwork.VertexList[vid];
            float distance = vinfo.Distance;

            var outEdges = roadNetwork.VertexEdges[vid];
            var successorVids = new List<int>();

            foreach (var eid in outEdges)
            {
                var e = roadNetwork.RoadSegments[eid];
                int suvid = vid == e.StartId ? e.EndId : e.StartId;
                successorVids.Add(suvid);
            }

            foreach (var suvid in successorVids)
            {
                Vector2 suvert = roadNetwork.VertexList[suvid];
                float sudistance = distance + (suvert - vert).Length();

                if (suvid == evid)
                {
                    var pathVids = new List<int> { suvid, vid };
                    if (checkedVids.Count == 0) return pathVids;
                    int? currVid = prev;
                    if (prev.HasValue) pathVids.Add(prev.Value);
                    while (true)
                    {
                        if (!checkedVids.TryGetValue(currVid.Value, out var cinfo))
                            return pathVids;

                        currVid = cinfo.Prev;
                        if (!currVid.HasValue) return pathVids;
                        pathVids.Add(currVid.Value);
                        if (currVid.Value == svid) return pathVids;
                    }
                }

                if (checkedVids.TryGetValue(suvid, out var checkedInfo))
                {
                    if (checkedInfo.Distance < sudistance) continue;

                    checkedInfo.Distance = sudistance;
                    checkedInfo.Prev = vid;
                }

                bool hasCandidate = false;

                for (int i = 0; i < candidates.Count; i++)
                {
                    var c = candidates[i];
                    if (c.VId != suvid)
                        continue;

                    if (c.Distance > sudistance)
                    {
                        c.Distance = sudistance;
                        c.VId = suvid;
                        c.Prev = vid;
                        candidates[i] = c;
                        break;
                    }

                    hasCandidate = true;
                    break;
                }

                if (hasCandidate)
                    continue;

                candidates.Add(new ASearchCandidate
                {
                    VId = suvid,
                    Distance = sudistance,
                    Prev = vid
                });
            }

            checkedVids[vid] = new ASearchCheckedInfo
            {
                Distance = distance,
                Prev = prev
            };
        }
        return new List<int>();
    }
}