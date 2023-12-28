using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace common
{
    public static class PathFinders
    {
        public static Dictionary<T, (T parent, int distance)> BfsToAll<T>(T start, Func<T, IEnumerable<T>> getNeighbors)
            where T : notnull
        {
            Queue<T> queue = new();
            queue.Enqueue(start);
            Dictionary<T, (T, int)> parentsDistances = new();
            parentsDistances[start] = (start, 0);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in getNeighbors(current))
                {
                    if (!parentsDistances.ContainsKey(next))
                    {
                        parentsDistances[next] = (current, parentsDistances[current].Item2 + 1);
                        queue.Enqueue(next);
                    }
                }
            }

            return parentsDistances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="getNeighbors">return valid moves</param>
        /// <param name="isEnd">Test if at end</param>
        /// <param name="getCost">Should calculate:  current.cost + neighbor.cost + predictCost(neighbor , destination)</param>
        /// <param name="numberOfPaths">max</param>
        /// <returns></returns>
        public static List<List<AStarSearch<T>.Step>> AstarToEnd<T>(T start, Func<T, IEnumerable<T>> getNeighbors,
            Predicate<T> isEnd,
            Func<int, T, T, int> getCost, int numberOfPaths = 0)
            where T : IComparable<T>
        {
            var astar = new AStarSearch<T>(start, getNeighbors, isEnd, getCost);
            var path = astar.NextShortestPath();
            var res = new List<List<AStarSearch<T>.Step>>();
            while (path != null)
            {
                res.Add(path);
                Debug.WriteLine($"Found path:{res.Count}  {path.Last().Cost}");

                if (numberOfPaths != 0 && res.Count >= numberOfPaths)
                    return res;
                path = astar.NextShortestPath();
            }

            return res;
        }

        public static (int, IEnumerable<T>) BfsToEnd<T>(T start, Func<T, IEnumerable<T>> getNeighbors,
            Predicate<T> isEnd)
            where T : notnull
        {
            Queue<T> queue = new();
            queue.Enqueue(start);
            Dictionary<T, (T, int)> parentsDistances = new();
            parentsDistances[start] = (start, 0);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (isEnd(current))
                {
                    IEnumerable<T> GetSteps()
                    {
                        T cursor = current;
                        while (!Equals(cursor, start))
                        {
                            yield return cursor;
                            cursor = parentsDistances[cursor].Item1;
                        }
                    }

                    return (parentsDistances[current].Item2, GetSteps());
                }

                foreach (var next in getNeighbors(current))
                {
                    if (!parentsDistances.ContainsKey(next))
                    {
                        parentsDistances[next] = (current, parentsDistances[current].Item2 + 1);
                        queue.Enqueue(next);
                    }
                }
            }

            return (-1, Enumerable.Empty<T>());
        }

        public static Dictionary<T, (T parent, int distance)> DijkstraToAll<T>(T start,
            Func<T, IEnumerable<(T, int)>> getNeighbors)
            where T : notnull
        {
            PriorityQueue<T, int> queue = new();
            queue.Enqueue(start, 0);
            Dictionary<T, (T, int)> parentsDistances = new();
            parentsDistances[start] = (start, 0);
            while (queue.TryDequeue(out var current, out var currentDistance))
            {
                if (parentsDistances[current].Item2 < currentDistance)
                {
                    continue;
                }

                if (parentsDistances[current].Item2 > currentDistance)
                {
                    throw new Exception("?");
                }

                foreach (var (neighbor, distanceToNext) in getNeighbors(current))
                {
                    var nextDistance = currentDistance + distanceToNext;
                    if (!parentsDistances.TryGetValue(neighbor, out var distanceInPD) ||
                        nextDistance < distanceInPD.Item2)
                    {
                        parentsDistances[neighbor] = (current, nextDistance);
                        queue.Enqueue(neighbor, nextDistance);
                    }
                }
            }

            return parentsDistances;
        }

        public static (int distance, IEnumerable<T> path) DijkstraToEnd<T>(T start,
            Func<T, IEnumerable<(T, int)>> getNeighbors, Predicate<T> isEnd)
            where T : notnull
        {
            PriorityQueue<T, int> queue = new();
            queue.Enqueue(start, 0);
            Dictionary<T, (T parent, int distance)> parentsDistances = new();
            parentsDistances[start] = (start, 0);
            while (queue.TryDequeue(out var current, out var currentDistance))
            {
                if (parentsDistances[current].distance < currentDistance)
                {
                    continue;
                }

                if (parentsDistances[current].distance > currentDistance)
                {
                    throw new Exception("?");
                }

                if (isEnd(current))
                {
                    IEnumerable<T> GetSteps()
                    {
                        T cursor = current;
                        while (!Equals(cursor, start))
                        {
                            yield return cursor;
                            cursor = parentsDistances[cursor].parent;
                        }
                    }

                    return (parentsDistances[current].distance, GetSteps());
                }

                foreach (var (neighbor, distanceToNext) in getNeighbors(current))
                {
                    var nextDistance = currentDistance + distanceToNext;
                    if (!parentsDistances.TryGetValue(neighbor, out var distanceInPD) ||
                        nextDistance < distanceInPD.distance)
                    {
                        parentsDistances[neighbor] = (current, nextDistance);
                        queue.Enqueue(neighbor, nextDistance);
                    }
                }
            }

            return (-1, Enumerable.Empty<T>());
        }

        public class AStarSearch<T>
            where T : IComparable<T>

        {
            private readonly Func<T, IEnumerable<T>> _getNeighbors;
            private readonly Predicate<T> _isEnd;
            private readonly Func<int, T, T, int> _getCost;

            private readonly SortedSet<Step?> _pending = new();

            public AStarSearch(
                T start,
                Func<T, IEnumerable<T>> getNeighbors,
                Predicate<T> isEnd,
                Func<int, T, T, int> getCost)
            {
                _getNeighbors = getNeighbors;
                _isEnd = isEnd;
                _getCost = getCost;
                _pending.Add(new Step(start, null, 0));
            }

            public List<Step>? NextShortestPath()
            {
                Step? current = _pending.FirstOrDefault();
                while (current != null)
                {
                    _pending.Remove(current);
                    if (_isEnd(current.Node))
                        return current.GeneratePath();
                    foreach (var neighbor in _getNeighbors(current.Node))
                    {
                        if (!current.Seen(neighbor))
                        {
                            Step nextStep =
                                new Step(neighbor, current, _getCost(current.Cost, current.Node, neighbor));
                            _pending.Add(nextStep);
                        }
                    }
                    //    Debug.Write("current was " + current.Node);
                    current = _pending.FirstOrDefault();
                    //   Debug.WriteLine(" went to " + (current==null ? "null" : current!.Node.ToString()));
                }

                return null;
            }


            public class Step(T node, Step? parent, int cost) : IComparable<Step>
            {
                public T Node { get; } = node;
                public Step? Parent { get; } = parent;
                public int Cost { get; } = cost;

                public bool Seen(T someNode)
                {
                    if (Node is null)
                        return false;
                    if (Node.Equals(someNode))
                        return true;
                    if (Parent == null)
                        return false;
                    return Parent.Seen(someNode);
                }
                public List<Step> GeneratePath()
                {
                    List<Step>? path;
                    path = Parent != null ? Parent.GeneratePath() : [];
                    path.Add(this);
                    return path;
                }

                public int CompareTo(Step? step)
                {
                    if (step == null)
                        return 1;
                    if (Cost != step.Cost)
                        return Cost.CompareTo(step.Cost);
                    if (!Node.Equals(step.Node))
                        return Node.CompareTo(step.Node);
                    if (Parent != null)
                        return Parent.CompareTo(step.Parent);
                    if (step.Parent == null)
                        return 0;
                    return -1;
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(Node, Parent, Cost);
                }
            }
        }
    }
}