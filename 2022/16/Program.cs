
using common;


//https://adventofcode.com/2022/day/16
internal class Program
{
    private static string _testData =
        @"Valve AA has flow rate=0; tunnels lead to valves DD, II, BB
Valve BB has flow rate=13; tunnels lead to valves CC, AA
Valve CC has flow rate=2; tunnels lead to valves DD, BB
Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE
Valve EE has flow rate=3; tunnels lead to valves FF, DD
Valve FF has flow rate=0; tunnels lead to valves EE, GG
Valve GG has flow rate=0; tunnels lead to valves FF, HH
Valve HH has flow rate=22; tunnel leads to valve GG
Valve II has flow rate=0; tunnels lead to valves AA, JJ
Valve JJ has flow rate=21; tunnel leads to valve II"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        var debug = true;
        FirstPart(GetDataStream(debug), debug);
        SecondPart(GetDataStream(debug), debug);
    }

    private static TextReader GetDataStream(bool debug) =>
        debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");

    private static void SecondPart(TextReader stream, bool debug)
    {

    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        var nodes = LoadFile(stream);

        foreach (var node in nodes.Values)
        {
            foreach (var edge1 in node.Edges.Values)
            {
                var startNode = edge1.To;
                foreach (var edge2 in node.Edges.Values)
                {
                    var destNode = edge2.To;
                    if (edge1.To != edge2.To)
                    {
                        var ne = new Edge(startNode, destNode, edge1.Steps + edge2.Steps, node);
                        if (!startNode.Edges.ContainsKey(destNode.Name)
                            || startNode.Edges[destNode.Name].Steps > ne.Steps)
                        {
                            startNode.Edges[destNode.Name] = ne;
                        }
                    }

                }
            }
        }

        private static Dictionary<string, Node> LoadFile(TextReader stream)
        {
            // 0     1  2    3   4   5    6      7    8    9   0   1     2
            //Valve AA has flow rate=0; tunnels lead to valves DD, II, BB

            var nodeList = new Dictionary<string, Node>();
            while (stream.ReadLine() is { } inpLine)
            {
                var parts = inpLine.Split(" =,;".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var name = parts[1];
                var flow = parts[5].ToInt()!.Value;
                var connected = parts.Skip(10).ToList();
                nodeList[name] = new Node(name, flow, connected);

            }
            nodeList.Values.ForEach((n, i) =>
            {
                n.Connect(nodeList);
            });
            return nodeList;
        }
    }

    internal class Node
    {
        private readonly List<string> _connected;
        public string Name { get; }
        public int Flow { get; }
        public Dictionary<string, Edge> Edges { get; } = new();
        public bool IsOpen { get; set; }
        public int Visited { get; set; } = 0;

        public Node(string name, int flow, List<string> connected)
        {
            _connected = connected;
            Name = name;
            Flow = flow;
        }

        public void Connect(Dictionary<string, Node> nodelist)
        {
            _connected.ForEach(s =>
            {
                var dest = nodelist[s];
                Edges[dest.Name] = new Edge(this, dest, 1, dest);
            });
        }
    }

    internal class Edge
    {
        public Node From { get; }
        public Node To { get; }
        public int Steps { get; }
        public Node FirstNode { get; }

        public Edge(Node from, Node to, int steps, Node firstNode)
        {
            From = from;
            To = to;
            Steps = steps;
            FirstNode = firstNode;
        }
    }


