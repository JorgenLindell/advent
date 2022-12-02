using common;
using System.Collections.Generic;
using System.Linq;

namespace _12
{
    public class CaveSystemSolver
    {
        public static Dictionary<string, Cave> Caves = new Dictionary<string, Cave>();

        public static List<string> GetPaths(Cave start, Cave end, bool ignoreIsolated)
        {
            var nonIsolatedCaves = Caves.Values.ToList();//.Where(c => !c.IsIsolated || c.Name=="end").ToList();
            Dictionary<Cave, int> visited = nonIsolatedCaves.ToDictionary(x => x, x => 0);

            var foundPaths = new List<string>();
            var path = new ExpandingList<Cave>();
            int pathIndex = 0;
            // CollectPathsTo(start, end, ref pathIndex, "");
            //return foundPaths.Select(l => l).ToList();

            var foundCaves = new List<IList<Cave>>();
            Queue<(Cave, IList<Cave>, string)> nodesToAnalyse = new Queue<(Cave, IList<Cave>, string)>();
            nodesToAnalyse.Enqueue((start, new List<Cave>(), ""));
            BreadthFirst();

            return foundCaves.Select(l => l.Select(c => c.Name).StringJoin()).ToList();

            bool CollectPathsTo(Cave current, Cave dest, ref int pathIndex, string wayHere)
            {
                // Mark the current node and store it in path[]
                visited[current] += 1;
                var cameFrom = path[pathIndex] ??= current;
                path[pathIndex++] = current;
                // If current vertex is same as destination, then print
                // current path[]
                bool reached = false;

                var visitedPath = wayHere;
                visitedPath += (", " + current.Name);

                // If current vertex is not destination
                // Recur for all the vertices adjacent to current vertex
                foreach (var c in current.Linked)
                {
                    if (!(c.Big || !c.IsStart || c.Small && visited[c] == 0)) continue;
                    if (c == dest)
                    {
                        foundPaths.Add(visitedPath + ", Final " + dest.Name);
                        //  Console.WriteLine("---Path1: " + path.Take(pathIndex).Select(x => x.Name).StringJoin());
                        //  Console.WriteLine("Path: " + visitedPath);
                        reached = true;
                    }
                    else
                    {
                        var wayToGoal = CollectPathsTo(c, dest, ref pathIndex, visitedPath);
                        {
                            visitedPath += ", returned to " + current.Name;
                        }
                    }
                }

                // Remove current vertex from path[] and mark it as unvisited
                pathIndex--;
                visited[current] = 0;
                return reached;
            }
            void BreadthFirst()
            {
                while (nodesToAnalyse.Count > 0)
                {
                    var (current, wayHere, doubleVisit) = nodesToAnalyse.Dequeue();

                    var history = new ExpandingList<Cave>(wayHere);
                    history.Add(current);

                    if (current == end)
                    {
                        foundCaves.Add(history.ToList());
                    }
                    else
                    {
                        current.Linked.ForEach(c =>
                        {
                            var doubleLocal = doubleVisit;
                            var call = false;
                            if (c.Big)
                                call = true;
                            else if (c.IsEnd)
                                call = true;
                            else if (!c.IsStart)
                            {
                                var visitCount = history.Count(k => k == c);
                                if (visitCount == 0)
                                    call = true;
                                else if (doubleLocal == "" && visitCount == 1)
                                {
                                    doubleLocal = c.Name;
                                    call = true;
                                }
                            }

                            if (call)
                                nodesToAnalyse.Enqueue((c, history, doubleLocal));
                        });

                    }
                };
            }
        }

        internal static bool HasCave(string name)
        {
            return Caves.ContainsKey(name);
        }

        public static Cave GetCave(string name)
        {
            if (!HasCave(name))
                Caves[name] = new Cave(name);
            return Caves[name];
        }

        public static void AddLink(Cave stC, Cave enC)
        {
            if (!ReferenceEquals(stC, enC))
            {
                stC.AddLink(enC);
            }
        }
    }
}