namespace Mapper;

public class Node
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Point Point { get; set; }
    public List<Edge> ConnectionsOut { get; set; } = new();
    public List<Edge> ConnectionsIn { get; set; } = new();
    public double StraightLineDistanceToEnd { get; set; }
    public double StraightLineDistanceToStart{ get; set; }

    public double? MinCostToEnd { get; set; }
    public bool Visited { get; set; }
    public Node? CloserToEnd { get; set; }

    public void Reset()
    {
        CloserToEnd = null;
        MinCostToEnd = null;
        Visited = false;
    }

    public double StraightLineDistanceTo(Node end)
    {
        return Math.Sqrt(Math.Pow(Point.X - end.Point.X, 2) + Math.Pow(Point.Y - end.Point.Y, 2));
    }

    public double ManhattanDistanceTo(Node end)
    {
        return Math.Abs(Point.X-end.Point.X)+Math.Abs(Point.Y-end.Point.Y);
    }
    
    internal bool ToCloseToAny(List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            var d = Math.Sqrt(Math.Pow(Point.X - node.Point.X, 2) + Math.Pow(Point.Y - node.Point.Y, 2));
            if (d < 0.01)
                return true;
        }
        return false;
    }
    public override string ToString()
    {
        return Name;
    }
}