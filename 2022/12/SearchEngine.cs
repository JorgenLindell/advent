using System.Security.Cryptography.X509Certificates;

namespace Mapper;

public class SearchEngine
{
    public Map Map { get; set; }
    public Node Start { get; set; }
    public Node End { get; set; }
    public int NodeVisits { get; private set; }
    public double ShortestPathLength { get; set; }
    public double ShortestPathCost { get; private set; }

    public SearchEngine(Map map)
    {
        Map = map;
        End = map.EndNode;
        Start = map.StartNode;
    }




    public void Custom()
    {
        NodeVisits = 0;
        End.MinCostToEnd = 0;
        var prioQueue = new List<Node>
        {
            End
        };
        do
        {
            prioQueue = prioQueue.OrderBy(x => x.MinCostToEnd + x.StraightLineDistanceToEnd).ToList();
            var node = prioQueue.First();
            prioQueue.Remove(node);
            NodeVisits++;
            foreach (var outEdge in node.ConnectionsIn.OrderBy(x => x.Cost))
            {
                var connectedSource = outEdge.ConnectedNode;
                var cnn = outEdge.Reverse;
                if (cnn.Cost > int.MaxValue - 1)
                    continue;
                if (connectedSource.Visited)
                    continue;
                if (connectedSource.MinCostToEnd == null ||
                    node.MinCostToEnd + cnn.Cost < connectedSource.MinCostToEnd)
                {
                    connectedSource.MinCostToEnd = node.MinCostToEnd + cnn.Cost;
                    connectedSource.CloserToEnd = node;
                    if (!prioQueue.Contains(connectedSource))
                        prioQueue.Add(connectedSource);
                }
            }
            node.Visited = true;
            if (node == Start)
                return;
        } while (prioQueue.Any());
    }

}