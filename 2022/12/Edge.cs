namespace Mapper;

public class Edge
{
    public double Length { get; set; }
    public double Cost { get; set; }
    public Node ConnectedNode { get; set; }
    public Edge Reverse { get; set; }

    public override string ToString()
    {
        return "-> " + ConnectedNode.ToString();
    }
}