using System.Diagnostics;
using common;
using Mapper;

namespace Mapper;

public class Map
{
    //   public static Map Randomize(int nodeCount, int branching, int seed, bool randomWeights)
    //   {
    //       var rnd = new Random(seed);
    //       var map = new Map();
    //
    //       for (int i = 0; i < nodeCount; i++)
    //       {
    //           var newNode = Node.GetRandom(rnd, i.ToString());
    //           if (!newNode.ToCloseToAny(map.Nodes))
    //               map.Nodes.Add(newNode);
    //       }
    //
    //       foreach (var node in map.Nodes)
    //           node.ConnectClosestNodes(map.Nodes, branching, rnd, randomWeights);
    //       //map.StartNode = map.Nodes.OrderBy(n => n.Point.X + n.Point.Y).First();
    //       //map.EndNode = map.Nodes.OrderBy(n => n.Point.X + n.Point.Y).Last();
    //       map.EndNode = map.Nodes[rnd.Next(map.Nodes.Count - 1)];
    //       map.StartNode = map.Nodes[rnd.Next(map.Nodes.Count - 1)];
    //
    //       foreach (var node in map.Nodes)
    //       {
    //           Debug.WriteLine($"{node}");
    //           foreach (var cnn in node.Connections)
    //           {
    //               Debug.WriteLine($"{cnn}");
    //           }
    //       }
    //       return map;
    //   }

    public List<Node> Nodes { get; set; } = new();

    public Node StartNode { get; set; }

    public Node EndNode { get; set; }

    public List<Node> ShortestPath { get; set; } = new();
    public Dictionary<string, Node> NodesLookup { get; } = new();

    public static Map ConstructMap(char[,] cells, (int r, int c) start, (int r, int c) end)
    {

        var map = new Map();
        var maxR = cells.GetUpperBound(0);
        var maxC = cells.GetUpperBound(1);
        for (int r = 0; r < maxR + 1; r++)
        {
            for (int c = 0; c < maxC + 1; c++)
            {
                var point = new Point(c, r);
                var node = new Node()
                {
                    Point = point,
                    Id = Guid.NewGuid(),
                    Name = point.ToString(),
                    StraightLineDistanceToEnd = Math.Abs(end.r - r) + Math.Abs(end.c - c),
                    StraightLineDistanceToStart = Math.Abs(start.r - r) + Math.Abs(start.c - c)
                };
                if (start.Equals((r, c)))
                    map.StartNode = node;
                if (end.Equals((r, c)))
                    map.EndNode = node;

                map.Nodes.Add(node);
                map.NodesLookup[node.Name] = node;
            }
        }

        for (int r = 0; r < maxR + 1; r++)
            for (int c = 0; c < maxC + 1; c++)
            {
                var nodePoint = new Point(x: c, y: r);
                var myHeight = cells[r, c];

                if (map.NodesLookup.ContainsKey(nodePoint.ToString()))
                {
                    var node = map.NodesLookup[nodePoint.ToString()];
                    AddConnection(c, 1, r, 0, map, myHeight, node);
                    AddConnection(c, -1, r, 0, map, myHeight, node);
                    AddConnection(c, 0, r, 1, map, myHeight, node);
                    AddConnection(c, 0, r, -1, map, myHeight, node);

                }
            }

        return map;
        void AddConnection(int c, int c2, int r, int r2, Map map1, char myHeight, Node node)
        {
            var destPoint = new Point(x: c + c2, y: r + r2);
            if (map1.NodesLookup.ContainsKey(destPoint.ToString()))
            {
                var destNode = map1.NodesLookup[destPoint.ToString()];
                var destHeight = cells[r + r2, c + c2];
                var cost = (destHeight <= myHeight + 1) ? 1 : int.MaxValue;

                var edge = new Edge()
                {
                    ConnectedNode = destNode,
                    Cost = cost,
                    Length = cost
                };
                node.ConnectionsOut.Add(edge);

                cost = cost == 1 ? int.MaxValue : 1;
                var edgeIn = new Edge()
                {
                    ConnectedNode = node,
                    Cost = cost,
                    Length = cost
                };
                edge.Reverse = edgeIn;
                edgeIn.Reverse = edge;
                destNode.ConnectionsIn.Add(edgeIn);
            }
        }
    }
}