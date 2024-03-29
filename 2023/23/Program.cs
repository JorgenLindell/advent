﻿using System.Diagnostics;
using System.IO;
using common;

namespace _23
{

    internal class Program
    {

        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        static readonly string xinput = @"
#.#####################
#.......#########...###
#######.#########.#.###
###.....#.>.>.###.#.###
###v#####.#v#.###.#.###
###.>...#.#.#.....#...#
###v###.#.#.#########.#
###...#.#.#.......#...#
#####.#.#.#######.#.###
#.....#.#.#.......#...#
#.#####.#.#.#########v#
#.#...#...#...###...>.#
#.#.#v#######v###.###v#
#...#.>.#...>.>.#.###.#
#####v#.#.###v#.#.###.#
#.....#...#...#.#.#...#
#.#########.###.#.#.###
#...###...#...#...#.###
###.###.#.###v#####v###
#...#...#.#.>.>.#.>.###
#.###.###.#.###.#.#v###
#.....###...###...#...#
#####################.#";

        private const char OUTSIDE = '\0';

        private static Grid map;

        private static Dictionary<VectorRc, List<Adjecency>> juncPerPos;

        static async Task Main(string[] args)
        {
            var lines = input.Replace("\r", "");
            map = new Grid(lines, OUTSIDE);

            var junc = map.Cells
                .Where(t => t.Value != OUTSIDE && t.Value != '#')
                .SelectMany(c => c.Pos.NextFour()
                        .Select(next => new { c, next, nextTile = map.Get(next) })
                        .Where(t => t.nextTile != OUTSIDE && t.nextTile != '#')
                        .ToList())
                .Select(x => new Adjecency(x.c.Pos, x.c.Value, x.next, x.nextTile, 1))
                .ToList();

            juncPerPos = junc.GroupBy(x => x.Pos).ToDictionary(x => x.Key, x => x.ToList());


            Debug.WriteLine(Part());
        }

        private static void TrimGraph(Dictionary<VectorRc, List<Adjecency>> juncPerPos, bool useSlopes)
        {
            var twiceConnected = juncPerPos.Where(x => x.Value.Count == 2).ToList();
            var prevLength = 0;

            while (twiceConnected.Count > 0 && prevLength != juncPerPos.Count)
            {
                prevLength = juncPerPos.Count;
                foreach (var kvp in twiceConnected)
                {
                    var previous = kvp.Value.First();
                    var next = kvp.Value.Last();
                    var prevPos = previous.Next;
                    var nextPos = next.Next;
                    var j0 = juncPerPos[kvp.Key];
                    var j1 = juncPerPos[prevPos];
                    var j2 = juncPerPos[nextPos];
                        var s = "" + j1.First().Value + j0.First().Value + j2.First().Value;
                    if (useSlopes &&
                        (j0.First().Value != '.' ||
                         j1.First().Value != '.' ||
                         j2.First().Value != '.'))
                    {
                        continue;
                    }

                    var a1 = j1.FirstOrDefault(x => x.Next == kvp.Key);
                    var a2 = j2.FirstOrDefault(x => x.Next == kvp.Key);
                    if (a1 == null || a2 == null) continue;
                    var a1Dist = a1.Dist;
                    var a2Dist = a2.Dist;
                    a1.Dist += a2Dist;
                    a2.Dist += a1Dist;
                    a2.Next = a1.Pos;
                    a2.NextValue = a1.Value;
                    a1.Next = a2.Pos;
                    a1.NextValue = a2.Value;
                    j0.Clear();
                    juncPerPos.Remove(kvp.Key);
                    Debug.WriteLine($"Removed {kvp.Key} {s} {juncPerPos.ContainsKey(kvp.Key)}");
                }
                twiceConnected = juncPerPos.Where(x => x.Value.Count == 2).ToList();
            }
        }


        internal class Adjecency
        {
            public VectorRc Pos { get; set; }
            public char Value { get; set; }
            public VectorRc Next { get; set; }
            public char NextValue { get; set; }
            public int Dist { get; set; }

            public Adjecency(VectorRc pos, char value, VectorRc next, char nextValue, int dist)
            {
                Pos = pos;
                Value = value;
                Next = next;
                NextValue = nextValue;
                Dist = dist;
            }
        }

        public static List<List<PathFinders.AStarSearch<Node>.Step>> Part()
        {

            var useSlopes = true;
            TrimGraph(juncPerPos, useSlopes);

            VectorRc startPos = new VectorRc(0, 1);
            VectorRc endPos = new VectorRc(map.Height - 1, map.Width - 2);

            Node start = new(new[] { startPos, startPos });

            List<List<PathFinders.AStarSearch<Node>.Step>> result;
            using (new Measure())
            {
                result = PathFinders.AstarToEnd(start, GetNeighbors, IsSafelyAtEnd, GetCost);
            }



            Debug.WriteLine("Paths:" + result.Count());
            foreach (var path in result)
            {
                Debug.WriteLine(path.Last().Cost);
            }



            start = new(new[] { startPos, startPos });
            useSlopes = false;
            TrimGraph(juncPerPos, useSlopes);

            using (new Measure())
            {
                result = PathFinders.AstarToEnd(start, GetNeighbors, IsSafelyAtEnd, GetCost);
                foreach (var path in result)
                {
                    var pathSum = 0;
                    // var prev = juncPerPos[path.First().Node.CurrentPos];
                    // for (int nod = 1; nod < path.Count(); nod++)
                    // {
                    //     pathSum += juncPerPos[path[nod].CurrentPos].Dist;
                    //     prev = juncPerPos[path[nod].CurrentPos];
                    // }

                    // pathSum += path.Count - 1;
                    Debug.WriteLine("Path length:" + path.Last().Cost);
                }
            }

            var lastPath = result.Last();


            return result;

            IEnumerable<Node> GetNeighbors(Node node)
            {
                List<Node> candidates = [];
                var current = node.CurrentPos;
                {
                    if (useSlopes && map[current].In("><^v".ToCharArray()))
                    {
                        VectorRc next =
                            map[current] switch
                            {
                                '>' => VectorRc.Right,
                                '<' => VectorRc.Left,
                                '^' => VectorRc.Up,
                                'v' => VectorRc.Down,
                                _ => throw new ArgumentOutOfRangeException()
                            };
                        var nextTile = current + next;
                        if (map.Get(nextTile) == OUTSIDE) throw new InvalidDataException("Sloping out");
                        candidates.Add(new Node(current, nextTile));
                    }
                    else
                    {
                        var nextSteps = juncPerPos[current].Select(x => x.Next)
                            .Where(d => d != node.Steps[^2])
                            .Select(t => new Node(current, t))
                            .ToList();

                        candidates.AddRange(nextSteps);
                    }
                }
                return candidates;
            }
            bool IsSafelyAtEnd(Node node)
            {
                if (node.Steps[^1] != endPos)
                {
                    return false;
                }

                return true;
            }

            int GetCost(int currentCost, Node current, Node next) =>
                currentCost + juncPerPos[current.CurrentPos].First(x => x.Next == next.CurrentPos).Dist;
        }

        private static Dictionary<VectorRc, (int sum, VectorRc currentLineStart)> MeasureLinks(
            Dictionary<VectorRc, (int, VectorRc)> currentSplit,
            Dictionary<VectorRc, Dictionary<VectorRc, (int, VectorRc)>> splits,
            VectorRc prev)
        {
            var newLinks = new Dictionary<VectorRc, (int sum, VectorRc currentLineStart)>();
            foreach (var currentLineStart in currentSplit.Keys)
            {
                var prevCell = prev;
                var sum = 0;
                var current = currentLineStart;
                while (!splits.ContainsKey(current))
                {
                    sum++;
                    var nextSteps = current.NextFour()
                        .Where(d => d != prevCell)
                        .Select(next => new { next, nextTile = map.Get(next) })
                        .Where(t => t.nextTile != OUTSIDE && t.nextTile != '#')
                        .Select(t => t.next)
                        .ToList();
                    prevCell = current;
                    current = nextSteps.FirstOrDefault();
                }

                if (splits.ContainsKey(current) && sum != 0)
                {
                    newLinks[current] = (sum, currentLineStart);
                }
            }

            splits[prev] = newLinks;
            return newLinks;
        }


        internal readonly struct Node : IEquatable<Node>, IComparable<Node>
        {
            public static bool EqualOnlyLast = true;

            public readonly List<VectorRc> Steps;
            public Node(IEnumerable<VectorRc> steps)
            {
                Steps = steps.ToList();
            }

            public Node(VectorRc startPos)
            {
                Steps = new List<VectorRc>() { startPos };
            }

            public Node(VectorRc startPos, VectorRc next) : this(startPos)
            {
                Steps = new List<VectorRc>() { startPos, next };
            }

            public VectorRc CurrentPos => Steps.Last();

            public bool Equals(Node other)
            {
                if (EqualOnlyLast)
                    return CompareTo(other) == 0;
                return Steps.SequenceEqual(other.Steps);
            }
            public override int GetHashCode()
            {
                if (EqualOnlyLast)
                    return CurrentPos.GetHashCode();
                return Steps.Aggregate(0, (acc, val) => (acc << 1) + val.GetHashCode());
            }

            // Generated IEquatable implementation via Quick Actions and Refactorings
            public override bool Equals(object? obj)
            {
                return obj is Node node && Equals(node);
            }
            public static bool operator ==(Node left, Node right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(Node left, Node right)
            {
                return !(left == right);
            }

            public int CompareTo(Node other)
            {
                if (ReferenceEquals(null, other)) return 1;
                return Steps.LastOrDefault().CompareTo(other.Steps.LastOrDefault());
            }

            public override string ToString()
            {
                return Steps.Last().ToString();
            }
        }

        private static readonly string input = @"
#.###########################################################################################################################################
#...#.....#.....................#...#.........###...###...###...#...#.......#...###.....#...#.......###.........#...........#.............###
###.#.###.#.###################.#.#.#.#######.###.#.###.#.###.#.#.#.#.#####.#.#.###.###.#.#.#.#####.###.#######.#.#########.#.###########.###
#...#...#...###...#.............#.#.#.......#.....#...#.#.#...#...#.#.....#.#.#...#...#...#...#...#.#...#.......#.........#.#.#.........#...#
#.#####.#######.#.#.#############.#.#######.#########.#.#.#.#######.#####.#.#.###.###.#########.#.#.#.###.###############.#.#.#.#######.###.#
#.......###...#.#.#...............#...#.....#.........#.#...#.......#.....#...#...###.....#...#.#...#...#.#...###...###...#.#.#.#...###.....#
###########.#.#.#.###################.#.#####.#########.#####.#######.#########.#########.#.#.#.#######.#.#.#.###.#.###.###.#.#.#.#.#########
#.........#.#...#.....................#.....#.###.....#.....#...#...#.......#...#...#####...#.#.#...###.#.#.#.....#...#.#...#.#...#.........#
#.#######.#.###############################.#.###.###.#####.###.#.#.#######.#.###.#.#########.#.#.#.###.#.#.#########.#.#.###.#############.#
#.......#...#.....###.....#...###.....###...#...#.#...#...#.#...#.#.....#...#...#.#.###...#...#...#.#...#.#.#.....#...#.#...#.#.............#
#######.#####.###.###.###.#.#.###.###.###.#####.#.#.###.#.#.#.###.#####.#.#####.#.#.###.#.#.#######.#.###.#.#.###.#.###.###.#.#.#############
#.....#.....#.#...#...#...#.#...#...#...#.#...#...#...#.#.#.#...#.....#.#.#.....#.#.###.#.#...#.....#...#.#...#...#.###.#...#.#.......#...###
#.###.#####.#.#.###.###.###.###.###.###.#.#.#.#######.#.#.#.###.#####.#.#.#.#####.#.###.#.###.#.#######.#.#####.###.###.#.###.#######.#.#.###
#...#.......#.#...#...#...#.#...###...#.#.#.#.........#.#.#.#...#...#.#.#.#.###...#.#...#.....#.###...#.#.#...#...#...#.#...#.#.......#.#.###
###.#########.###.###.###.#.#.#######.#.#.#.###########.#.#.#.###.#.#.#.#.#.###.###.#.#########.###.#.#.#.#.#.###.###.#.###.#.#.#######.#.###
###.....#...#.#...###...#.#.#.#.>.>...#.#.#...#.......#.#.#.#.###.#.#.#.#.#.###.#...#.........#...#.#.#.#...#.>.>.#...#...#.#.#.....#...#...#
#######.#.#.#.#.#######.#.#.#.#.#v#####.#.###.#.#####.#.#.#.#.###.#.#.#.#.#.###.#.###########.###.#.#.#.#######v###.#####.#.#.#####.#.#####.#
#...###...#...#.#.....#.#.#.#.#.#.#...#...#...#...#...#.#.#.#.###.#.#.#.#.#.###.#.#...###...#...#.#.#.#.#.....#.#...#...#.#.#.#.....#...#...#
#.#.###########v#.###.#.#.#.#.#.#.#.#.#####.#####.#.###.#.#.#.###.#.#.#.#.#.###.#.#.#.###.#.###.#.#.#.#.#.###.#.#.###.#.#.#.#.#.#######.#.###
#.#.....#...###.>.###.#.#.#.#.#.#.#.#...###.....#.#.#...#.#.#.>.>.#.#.#.#.#...#.#.#.#.>.>.#...#.#.#.#...#...#.#.#...#.#.#.#.#.#...###...#...#
#.#####.#.#.###v#####.#.#.#.#.#.#.#.###.#######.#.#.#.###.#.###v###.#.#.#.###.#.#.#.###v#####.#.#.#.#######.#.#.###.#.#.#.#.#.###.###.#####.#
#.#...#...#.#...###...#.#.#.#.#.#.#.#...#.....#.#.#.#...#.#...#...#...#.#.###.#.#.#.#...#...#...#...###.....#.#...#.#.#.#.#.#.###...#.#.....#
#.#.#.#####.#.#####.###.#.#.#.#.#.#.#.###.###.#.#.#.###.#.###.###.#####.#.###.#.#.#.#.###.#.###########.#####.###.#.#.#.#.#.#.#####.#.#.#####
#...#.....#.#.....#.#...#.#.#.#.#.#.#.....#...#...#...#.#.#...###...#...#...#...#...#...#.#.###...#...#.....#...#.#.#.#.#.#...###...#.#.....#
#########.#.#####.#.#.###.#.#.#.#.#.#######.#########.#.#.#.#######.#.#####.###########.#.#.###.#.#.#.#####.###.#.#.#.#.#.#######.###.#####.#
###...###.#.#...#.#.#.#...#.#.#.#...###...#.....#.....#.#.#.#.......#.......#.....#.....#.#.#...#...#...#...###.#.#.#.#...###...#...#.#.....#
###.#.###.#.#.#.#.#.#.#.###.#.#.#######.#.#####.#.#####.#.#.#.###############.###.#.#####.#.#.#########.#.#####.#.#.#.#######.#.###.#.#.#####
#...#.#...#...#...#...#.#...#...#.......#.......#.......#...#.....#.......###...#.#.......#.#.........#...#...#.#.#.#.....#...#.....#.#.#...#
#.###.#.###############.#.#######.###############################.#.#####.#####.#.#########.#########.#####.#.#.#.#.#####.#.#########.#.#.#.#
#...#.#...............#...#...###.....#.......#.....#.....#.....#...#.....#.....#.........#.#...#...#.#...#.#.#...#.#...#.#...........#...#.#
###.#.###############.#####.#.#######.#.#####.#.###.#.###.#.###.#####.#####.#############.#.#.#.#.#.#v#.#.#.#.#####.#.#.#.#################.#
#...#...#...#.....#...#...#.#.###...#...#.....#...#.#.#...#...#.#.....#####...........###.#.#.#...#.>.>.#...#...###...#...###...#.........#.#
#.#####.#.#.#.###.#.###.#.#.#.###.#.#####.#######.#.#.#.#####.#.#v###################.###.#.#.#######v#########.#############.#.#.#######.#.#
#.#...#...#.#.###...#...#...#...#.#.#...#.......#.#...#...#...#.>.>.....#.......#...#...#.#.#.......#.....#.....#...#...#.....#.#.......#.#.#
#.#.#.#####.#.#######.#########.#.#.#.#.#######.#.#######.#.#####v#####.#.#####.#.#.###.#.#.#######.#####.#.#####.#.#.#.#.#####.#######.#.#.#
#.#.#.#...#...#...###.......#...#.#.#.#.###.....#.......#.#...#...#.....#.....#...#.....#...#...###...#...#...###.#...#.#.....#.........#.#.#
#.#.#v#.#.#####.#.#########.#.###.#.#.#.###.###########.#.###.#.###.#########.###############.#.#####.#.#####.###.#####.#####.###########.#.#
#.#.#.>.#.###...#...#...#...#...#.#.#.#...#.....#...#...#.....#...#.#.....###.............#...#.......#...###.#...#.....#...#...........#.#.#
#.#.#v###.###.#####.#.#.#.#####.#.#.#.###.#####.#.#.#.###########.#.#.###.###############.#.#############.###.#.###.#####.#.###########.#.#.#
#.#.#...#...#.....#.#.#.#.#.....#.#.#...#.#.....#.#...#.......#...#.#.#...#...#...#.......#.............#...#...###.#...#.#.#...#.......#...#
#.#.###.###.#####.#.#.#.#.#.#####.#.###.#.#v#####.#####.#####.#.###.#.#.###.#.#.#.#.###################.###.#######.#.#.#.#.#.#.#.###########
#.#.###...#.....#.#.#.#.#.#.#...#.#...#.#.>.>...#.......#.....#...#.#.#.###.#.#.#.#...###...#...#.....#...#...#.....#.#.#.#.#.#.#...#...#...#
#.#.#####.#####.#.#.#.#.#.#.#.#.#.###.#.###v###.#########.#######.#.#.#.###.#.#.#.###v###.#.#.#.#.###.###.###.#.#####.#.#.#.#.#.###v#.#.#.#.#
#...#.....#...#...#...#.#.#.#.#.#...#.#.###...#.#...#...#...###...#...#...#.#.#.#...>.>.#.#.#.#.#.###.....#...#.....#.#.#.#.#.#.#.>.#.#...#.#
#####.#####.#.#########.#.#.#.#.###.#.#.#####.#.#.#.#.#.###.###.#########.#.#.#.#####v#.#.#.#.#.#.#########.#######.#.#.#.#.#.#.#.#v#.#####.#
#.....#...#.#.#.......#.#.#...#.#...#...#.....#...#...#.#...#...#...#####...#.#.###...#.#.#.#.#.#.#.......#.......#.#.#.#.#...#...#...#...#.#
#.#####.#.#.#.#.#####.#.#.#####.#.#######.#############.#.###.###.#.#########.#.###.###.#.#.#.#.#.#.#####.#######.#.#.#.#.#############.#.#.#
#.......#...#...#.....#...#.....#.###...#...........###.#.#...#...#.........#...#...###...#.#.#.#.#.....#.......#.#...#...#.............#...#
#################.#########.#####.###.#.###########.###.#.#.###.###########.#####.#########.#.#.#.#####.#######.#.#########.#################
#.............#...#...#...#.#.....#...#...#.........#...#.#...#.#.......#...###...#.......#.#.#.#.#...#.......#...#.......#.................#
#.###########.#.###.#.#.#.#.#.#####.#####.#.#########.###.###.#.#.#####.#.#####.###.#####.#.#.#.#.#.#.#######.#####.#####.#################.#
#...........#...###.#.#.#.#...#...#.....#...#...#...#...#...#...#.....#.#.#.....###.#.....#...#.#.#.#.........###...#...#...#...............#
###########.#######.#.#.#.#####.#.#####.#####.#.#.#.###.###.#########.#.#.#.#######.#.#########.#.#.#############.###.#.###.#.###############
#...........#.......#...#.#...#.#.#...#.......#...#...#.#...###.......#...#.........#...###...#...#.............#...#.#.....#...........#...#
#.###########.###########.#.#.#.#.#.#.###############.#.#.#####.#######################.###.#.#################.###.#.#################.#.#.#
#...#.........#...........#.#.#.#.#.#.#...#.........#.#...#...#.........#.............#...#.#.#...#.........#...###.#.....#...#...###...#.#.#
###.#.#########.###########.#.#.#.#.#.#.#.#.#######.#.#####.#.#########.#.###########.###.#.#.#.#.#.#######.#.#####.#####.#.#.#.#.###v###.#.#
###.#.#.....#...#...#.....#.#.#.#.#.#.#.#...#...###...#...#.#.#...#.....#...........#.#...#.#...#.#.....#...#.....#...#...#.#.#.#.#.>.###.#.#
###.#.#.###.#.###.#v#.###.#.#.#.#.#.#.#.#####.#.#######.#.#.#.#.#.#.###############.#.#.###.#####.#####.#.#######.###.#.###.#.#.#.#.#v###.#.#
#...#.#...#...#...#.>.#...#.#.#.#.#.#.#.......#.....#...#.#.#...#...#.......#.......#...#...#.....###...#.........#...#.#...#.#.#.#.#.....#.#
#.###.###.#####.###v###.###.#.#.#.#.#.#############.#.###.#.#########.#####.#.###########.###.#######.#############.###.#.###.#.#.#.#######.#
#.....###.......#...###...#.#.#.#.#.#.###...........#...#.#...........#.....#...#.....###...#.#.....#.#...#.....#...#...#...#...#...#.....#.#
#################.#######.#.#.#.#.#.#.###.#############.#.#############.#######.#.###.#####.#.#.###.#.#.#.#.###.#.###.#####.#########.###.#.#
#.................###.....#.#.#.#.#.#...#...#.........#.#...#...........#...###.#.#...#...#.#.#.#...#...#...#...#...#.#.....#...#.....#...#.#
#.###################.#####.#.#.#.#.###.###v#.#######.#.###.#.###########.#.###v#.#.###.#.#.#.#.#.###########.#####.#.#.#####.#.#.#####.###.#
#...................#.......#.#.#.#...#...>.>.#.......#.#...#.......#...#.#.#.>.>.#.###.#.#.#.#.#.###...#...#.###...#.#...#...#.#.....#...#.#
###################.#########.#.#.###.#####v###.#######.#.#########v#.#.#.#.#.#v###.###.#.#.#.#.#.###.#.#.#.#v###.###.###.#.###.#####.###.#.#
#...................###.....#.#.#.#...#.....###.#.....#.#.......#.>.>.#.#.#.#.#...#.#...#.#.#...#.....#...#.>.>.#...#.#...#...#.#...#...#.#.#
#.#####################.###.#.#.#.#.###.#######.#.###.#.#######.#.#v###.#.#.#.###.#.#.###.#.#################v#.###.#.#.#####.#.#.#.###.#.#.#
#.....................#...#.#...#...###.#.....#.#.###.#.....###...#...#.#.#...###.#...###.#.......#.........#.#.#...#...#...#.#.#.#.#...#.#.#
#####################.###.#.###########.#.###.#.#.###.#####.#########.#.#.#######.#######.#######.#.#######.#.#.#.#######.#.#.#.#.#.#.###.#.#
#.....................#...#.#...#.....#...#...#.#.#...#.....#...#...#.#.#.#.......###...#.........#.......#.#.#...#.....#.#...#.#.#...#...#.#
#.#####################.###.#.#.#.###.#####.###.#.#.###.#####.#.#.#.#.#.#.#.#########.#.#################.#.#.#####.###.#.#####.#.#####.###.#
#.........#...#...#...#...#.#.#.#...#.#...#.###.#.#.....#.....#.#.#...#.#.#...........#.........#.......#.#...#...#.#...#.....#...#...#.....#
#########.#.#.#.#v#.#.###.#.#.#.###.#.#.#.#v###.#.#######.#####.#.#####.#.#####################.#.#####.#.#####.#.#.#.#######v#####.#.#######
#.......#...#...#.>.#...#.#.#.#.#...#.#.#.>.>.#...#...###...###.#.....#...#####.................#.....#...#...#.#.#.#.....#.>.#...#.#.#...###
#.#####.#########v#####.#.#.#.#.#.###.#.###v#.#####.#.#####.###.#####.#########.#####################.#####.#.#.#.#.#####.#.#v#.#.#.#.#.#.###
#.....#.###...#...#####.#.#...#.#...#...###.#...#...#.#...#...#.......###...#...#...###...#...###...#.###...#.#.#.#...#...#.#...#...#...#...#
#####.#.###.#.#.#######.#.#####.###.#######.###.#.###.#.#.###.###########.#.#.###.#.###.#.#.#.###.#.#v###.###.#.#.###.#.###.###############.#
#.....#.....#.#.......#.#.#.....#...#...#...#...#.#...#.#.#...###...#...#.#.#.....#.#...#...#.#...#.>.>.#.###.#.#...#.#.....#.......#.......#
#.###########.#######.#.#.#.#####.###.#.#.###.###.#.###.#.#.#####.#.#.#.#.#.#######v#.#######.#.#####v#.#.###.#.###.#.#######.#####.#.#######
#.......#...#.#.......#...#...#...#...#.#...#.#...#...#.#.#.#...#.#.#.#.#.#.#...#.>.>.#.....#.#...###.#...#...#...#.#.#.......#...#.#.....###
#######.#.#.#.#.#############.#.###.###.###.#.#.#####.#.#.#.#.#.#.#.#.#.#.#.#.#.#.#v###.###.#.###.###.#####.#####.#.#.#.#######.#.#.#####.###
#...###...#.#.#.............#...#...#...#...#.#...#...#.#.#.#.#.#.#.#.#...#.#.#.#.#.###...#.#...#.#...#...#.......#...#.........#.#.#...#...#
#.#.#######.#.#############.#####.###.###.###.###.#.###.#.#v#.#.#.#.#.#####.#.#.#.#.#####.#.###.#.#.###.#.#######################.#.#.#.###.#
#.#.........#...............###...###...#...#.....#.....#.>.>.#...#.#.....#...#...#.......#...#...#.....#.....#...................#...#.....#
#.#############################.#######.###.###############v#######.#####.###################.###############.#.#############################
#.........#.............#.......###...#.#...###...#...#...#.###...#...#...#.................#.#...............#.........#...............#...#
#########.#.###########.#.#########.#.#.#.#####.#.#.#.#.#.#.###.#.###.#.###.###############.#.#.#######################.#.#############.#.#.#
#.......#...#...........#...........#.#.#...#...#...#.#.#.#.....#...#...###...............#.#.#.#.............###.......#.#.............#.#.#
#.#####.#####.#######################.#.###.#.#######.#.#.#########.#####################.#.#.#.#.###########.###.#######.#.#############.#.#
#.....#.....#...........#.............#.....#.......#.#.#.#...#.....#...#.................#...#...###...#...#...#.....#...#...........#...#.#
#####.#####.###########.#.#########################.#.#.#.#.#.#.#####.#.#.###########################.#.#.#.###.#####.#.#############.#.###.#
#...#.....#.#...#...#...#.............#...###.......#...#.#.#.#...#...#.#.....#.....#.....#...#.......#...#.#...#.....#.#...#.........#.#...#
#.#.#####.#.#.#.#.#.#.###############.#.#.###.###########.#.#.###.#.###.#####.#.###.#.###.#.#.#.###########.#.###.#####.#.#.#.#########.#.###
#.#.......#...#...#...#.......#.......#.#...#...........#.#.#.#...#.#...#...#...###...#...#.#.#...........#...###...#...#.#...#.........#...#
#.#####################.#####.#.#######.###.###########.#.#.#.#.###.#.###.#.###########.###.#.###########.#########.#.###.#####.###########.#
#...#...#...#.....#...#.....#.#.......#.#...#...###.....#...#...#...#...#.#...#.......#.#...#.###...#.....#.....###...#...#.....#...........#
###.#.#.#.#.#.###.#.#.#####.#.#######.#.#.###.#.###.#############.#####.#.###.#.#####.#.#.###.###.#.#.#####.###.#######.###.#####.###########
###.#.#.#.#...###...#.#.....#.#.......#.#...#.#.#...#...........#...#...#.#...#.....#...#.#...#...#...#####...#...#...#.....#...#...........#
###.#.#.#.###########.#.#####.#v#######.###.#.#.#.###.#########.###.#.###.#.#######.#####.#.###.#############.###.#.#.#######.#.###########.#
###.#.#.#.#...........#.....#.>.>.......#...#.#.#.....#.........###.#...#.#.#...###...###.#.###...........#...#...#.#.#.......#.#.....#...#.#
###.#.#.#.#.###############.###v#########.###.#.#######.###########.###.#.#.#.#.#####.###.#.#############.#.###.###.#.#.#######.#.###.#.#.#.#
###...#...#.......#...#####.#...#.........#...#...#...#...#.......#...#...#.#.#.......#...#.#...#.........#...#...#.#.#.......#...###...#...#
#################.#.#.#####.#.###.#########.#####.#.#.###.#.#####.###.#####.#.#########.###.#.#.#.###########.###.#.#.#######.###############
###...#...###...#...#.###...#...#.#####...#.....#.#.#.#...#.#.....#...#.....#.........#...#.#.#.#.....###...#...#.#.#.###.....#.......#.....#
###.#.#.#.###.#.#####.###.#####.#.#####.#.#####.#.#.#.#v###.#.#####.###.#############.###.#.#.#.#####v###.#.###.#.#.#.###.#####.#####.#.###.#
#...#...#...#.#.#.....#...###...#...#...#.#...#.#.#.#.>.>...#...###...#.......#.......#...#.#.#.#...>.>.#.#.#...#.#.#...#.......#.....#...#.#
#.#########.#.#.#.#####.#####.#####.#.###.#.#.#.#.#.###v#######.#####.#######.#.#######.###.#.#.#.###v#.#.#.#.###.#.###.#########.#######.#.#
#.........#.#.#.#...#...#...#.#...#...###.#.#...#.#.###.....#...#...#.#.......#.....#...###...#...###.#.#.#.#...#.#.#...#...#...#...#...#.#.#
#########.#.#.#.###.#.###.#.#.#.#.#######.#.#####.#.#######.#.###.#.#.#.###########v#.###############.#.#.#.###.#.#.#.###.#.#v#.###.#.#.#.#.#
###...#...#.#.#.#...#.....#.#.#.#.#...###.#.....#.#.#.......#...#.#.#.#...#.......>.>.#...............#...#.....#.#.#...#.#.>.#.....#.#...#.#
###.#.#.###.#.#.#v#########.#.#.#.#.#.###.#####.#.#.#.#########.#.#.#.###.#.#######v###.#########################.#.###.#.###v#######.#####.#
#...#.#...#.#.#.#.>.#.....#.#.#.#...#...#.......#...#.........#...#...#...#.#.......###.....#.....#...#.........#.#.#...#.#...###...#.#.....#
#.###.###.#.#.#.#v#.#.###.#.#.#.#######.#####################.#########.###.#.#############.#.###.#.#.#.#######.#.#.#.###.#.#####.#.#.#.#####
#...#.....#.#.#.#.#.#...#.#.#.#.#...###.........#.............#.....###.....#.......###...#...#...#.#.#.#.......#...#.....#...#...#...#.....#
###.#######.#.#.#.#.###.#.#.#.#.#.#.###########.#.#############.###.###############.###.#.#####.###.#.#.#.###################.#.###########.#
###...#...#.#.#.#.#.....#...#...#.#...#.........#...#...#.......#...#...###...#...#...#.#.....#.....#...#.#...........#.......#.###.........#
#####.#.#.#.#.#.#.###############.###.#.###########.#.#.#.#######.###.#.###.#.#.#.###.#.#####.###########.#.#########.#.#######.###.#########
#.....#.#.#...#...###.....#...###...#...###...#####...#...#.......#...#.#...#...#.....#.....#.#...#...###...#.....#...#...#...#...#.........#
#.#####.#.###########.###.#.#.#####.#######.#.#############.#######.###.#.#################.#.#.#.#.#.#######.###.#.#####.#.#.###.#########.#
#.......#.#...#...#...#...#.#.....#.......#.#...#...#...###.......#.#...#.#.....###...#...#.#...#.#.#.###...#...#...#...#.#.#.....#.........#
#########.#.#.#.#.#.###.###.#####.#######.#.###.#.#.#.#.#########.#.#.###.#.###.###.#.#.#.#.#####.#.#.###.#.###v#####.#.#.#.#######.#########
#.........#.#.#.#.#...#.###.#.....#...#...#.#...#.#.#.#.#...#...#.#.#.###...###...#.#.#.#.#.....#...#...#.#.#.>.>...#.#.#...#...###.......###
#.#########.#.#.#.###.#.###.#.#####.#.#v###.#.###.#.#.#.#.#.#.#.#v#.#.###########.#.#.#.#.#####.#######.#.#.#.#####.#.#.#####.#.#########v###
#.........#.#.#.#...#.#...#.#...#...#.>.>...#...#.#.#.#.#.#.#.#.>.>.#.#...#...#...#.#.#.#.#...#.....###.#.#.#...#...#.#...#...#.#.....#.>.###
#########.#.#.#.###.#.###.#.###.#.#############.#.#.#.#.#.#.#.#######.#.#.#.#.#.###.#.#.#.#.#.#####.###.#.#.###.#.###.###.#.###.#.###.#.#v###
#.........#.#...#...#...#.#...#.#.#...#.........#.#.#.#...#.#.#.....#...#...#.#.....#.#.#.#.#.#...#...#.#.#.#...#.#...#...#.#...#...#...#...#
#.#########.#####.#####.#.###.#.#.#.#.#.#########.#.#.#####.#.#.###.#########.#######.#.#.#.#.#.#.###.#.#.#.#.###.#.###.###.#.#####.#######.#
#.........#.....#.#...#.#.#...#.#...#.#...#.....#.#.#.#.....#.#...#...........#.......#.#...#...#.#...#...#.#...#.#...#...#.#...#...#.......#
#########.#####.#.#.#.#.#.#.###.#####.###.#.###.#.#.#.#.#####.###.#############.#######.#########.#.#######.###.#.###.###.#.###.#.###.#######
###.......#...#.#...#...#.#.###.....#.#...#.###.#.#.#.#...#...###.......#.....#.###...#.......#...#.......#.#...#.#...#...#.#...#.###.......#
###.#######.#.#.#########.#.#######.#.#.###.###.#.#.#.###.#.###########.#.###.#v###.#.#######.#.#########.#.#.###.#.###.###.#.###.#########.#
#...#...#...#.#...#...#...#.......#...#.....#...#.#...#...#.....###.....#.###.>.>.#.#.#.......#.....#...#.#.#.#...#.#...#...#...#...#.......#
#.###.#.#.###.###.#.#.#.#########.###########.###.#####.#######.###.#####.#######.#.#.#.###########.#.#.#.#.#.#.###.#.###.#####.###.#.#######
#.#...#.#.###...#...#.#.#...#...#.###...#.....#...#.....#.....#.#...#...#.#.......#.#.#...#.......#.#.#...#.#.#.#...#.#...#.....#...#.......#
#.#.###.#.#####.#####.#.#.#.#.#.#.###.#.#.#####.###.#####.###.#.#.###.#.#.#.#######.#.###.#.#####.#.#.#####.#.#.#.###.#.###.#####.#########.#
#.#.#...#.....#...#...#.#.#.#.#.#.#...#...#...#...#.#...#.#...#.#...#.#.#.#.....#...#...#...#...#.#.#.#.....#.#.#.#...#.#...#...#.#.........#
#.#.#.#######.###.#.###.#.#.#.#.#.#.#######.#.###.#.#.#.#.#.###.###.#.#.#.#####.#.#####.#####.#.#.#.#.#.#####.#.#.#.###.#.###.#.#.#.#########
#...#.........###...###...#...#...#.........#.....#...#...#.....###...#...#####...#####.......#...#...#.......#...#.....#.....#...#.........#
###########################################################################################################################################.#";

    }
}

