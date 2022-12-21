
using System.Diagnostics;
using System.Net.Sockets;
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
        var debug = false;
        FirstPart(GetDataStream(debug), debug);
        SecondPart(GetDataStream(debug), debug);
    }

    private static TextReader GetDataStream(bool debug) =>
        debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");

    private static void SecondPart(TextReader stream, bool debug)
    {
        Dictionary<string, Node> nodes;
        nodes = LoadFile(stream, out string start);
        var startNode = nodes["AA"];
        var cache = new Dictionary<string, int>();
        var productiveNodesCount = nodes.Values.Count(x => x.Flow > 0);

        var result = MaxFlow(startNode, new HashSet<string>(), "", 26, false);
        Debug.WriteLine("result=" + result);



        int MaxFlow(Node current, HashSet<string> opened, string openedText, int timeLeft, bool elephant)
        {
            if (opened.Count == productiveNodesCount)
                return 0;

            if (timeLeft <= 0)
            {
                if (!elephant)
                {
                    return MaxFlow(startNode, opened, openedText, 26, true);
                }
                return 0;
            }


            var key = $"{current.Name};{openedText};{((char)timeLeft)};{elephant}";
            if (cache.ContainsKey(key))
                return cache[key];


            var maxFlow = 0;
            if (current.Flow > 0 && !opened.Contains(current.Name))
            {
                var currentOpened = opened.Append(current.Name).ToHashSet();
                var currentOpenedText = string.Join(',', currentOpened.OrderBy(x=>x));
                maxFlow = (current.Flow * (timeLeft - 1)) + MaxFlow(current, currentOpened, currentOpenedText, timeLeft - 1, elephant);
            }
            foreach (var edge in current.Edges.Values)
            {
                maxFlow = Math.Max(maxFlow, MaxFlow(edge.To, opened, openedText, timeLeft - 1, elephant));
            }

            cache[key] = maxFlow;
            return maxFlow;
        }


    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        Dictionary<string, Node> nodes;
        nodes = LoadFile(stream, out string start);
        var startNode = nodes["AA"];
        var maxTime = 30;
        var cache = new Dictionary<string, int>();

        var result = MaxFlow(startNode, new List<string>(), maxTime);
        Debug.WriteLine("result=" + result);



        int MaxFlow(Node current, List<string> opened, int timeLeft)
        {
            if (timeLeft <= 0) return 0;

            var key = $"{current.Name};{string.Join(',', opened)};{((char)timeLeft)}";
            if (cache.ContainsKey(key))
                return cache[key];


            var maxFlow = 0;
            if (current.Flow > 0 && !opened.Contains(current.Name))
            {
                var currentOpened = opened.Append(current.Name).OrderBy(x => x).ToList();

                maxFlow = (current.Flow * (timeLeft - 1)) + MaxFlow(current, currentOpened, timeLeft - 1);
            }

            foreach (var edge in current.Edges.Values)
            {
                maxFlow = Math.Max(maxFlow, MaxFlow(edge.To, opened, timeLeft - 1));
            }

            cache[key] = maxFlow;
            return maxFlow;
        }

    }

    private static Dictionary<string, Node> LoadFile(TextReader stream, out string first)
    {
        // 0     1  2    3   4   5    6      7    8    9   0   1     2
        //Valve AA has flow rate=0; tunnels lead to valves DD, II, BB

        var nodeList = new Dictionary<string, Node>();
        first = "";
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Split(" =,;".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var name = parts[1];
            var flow = parts[5].ToInt()!.Value;
            var connected = parts.Skip(10).ToList();
            nodeList[name] = new Node(name, flow, connected);
            if (first == "") first = name;

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
    public int IsOpen { get; set; } = 0;
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

    public override string ToString() => $"{Name} [{Flow}]";
}

internal class Edge
{
    public Node From { get; }
    public Node To { get; }
    public int Steps { get; }
    public Node FirstNode { get; }
    public override string ToString() => $"{From.Name}->{To.Name} [{Steps}] ({To.Flow}) via {FirstNode.Name}";

    public Edge(Node from, Node to, int steps, Node firstNode)
    {
        From = from;
        To = to;
        Steps = steps;
        FirstNode = firstNode;
    }
}


