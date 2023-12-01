
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
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

    // ReSharper disable once InconsistentNaming
    private const bool _debug = false;
    public static long NumberOfCalls { get; set; }
    public static long SavedByCache { get; set; }

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream);
        SecondPart(GetDataStream);
    }

    private static TextReader GetDataStream() =>
        _debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");

    private static void SecondPart(Func<TextReader> stream)
    {
        // got an elephant to help
        Dictionary<string, Node> nodes = LoadFile(stream());

        var startNode = nodes["AA"];
        var productiveNodesCount = nodes.Values.Count(x => x.Flow > 0); // don't spend time trying to open those that have no flow
        var cache = new Dictionary<(int, ulong, int, bool), (int flow, long calls)>();
        var sw = new Stopwatch();
        sw.Start();
        Program.NumberOfCalls = 0;
        Program.SavedByCache = 0;
        var result = MaxFlow(startNode, 0ul, 26, false, true, productiveNodesCount, startNode, cache);
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed:g}  result={result}");
        Console.WriteLine($"{NumberOfCalls} {SavedByCache}  {NumberOfCalls-SavedByCache}");
    }



    private static void FirstPart(Func<TextReader> stream)
    {
        // no helper available, work for 30 minutes

        Dictionary<string, Node> nodes = LoadFile(stream());
        var startNode = nodes["AA"];
        var productiveNodesCount = nodes.Values.Count(x => x.Flow > 0); // don't spend time trying to open those that have no flow
        var cache = new Dictionary<(int, ulong, int, bool), (int flow, long calls)>();
        var sw = new Stopwatch();
        sw.Start();
        Program.NumberOfCalls = 0;
        Program.SavedByCache = 0;
        var result = MaxFlow(startNode, 0ul, 30, false, false,  productiveNodesCount, startNode, cache);
        sw.Stop();
        Console.WriteLine($"{sw.Elapsed:g}  result={result}");
        Console.WriteLine($"{NumberOfCalls} {SavedByCache}  {NumberOfCalls-SavedByCache}");
    }


    private static int MaxFlow(Node current, ulong openedBm, int timeLeft, bool elephant, bool useElephant, int productiveNodesCount, Node startNode, Dictionary<(int, ulong, int, bool), (int flow, long calls)> cache)
    {
        Program.NumberOfCalls++;
        // calculate max flow in a subtree of the solution space

        if (BitCounter.CountSetBits(openedBm) == productiveNodesCount)
            return 0; // we are done with the ones meaningful to open.

        if (timeLeft <= 0)
        {
            if (!elephant && useElephant)
            {
                // if we have an elephant helper, let it work with those nodes I couldn't open in time
                // this is done at the end of EACH of my tries, so it will find those where we work best in "tandem".
                return MaxFlow(startNode, openedBm, 26, true, useElephant, productiveNodesCount, startNode, cache);
            }
            return 0; // no elephant or the elephant is at end of time, nothing more can be produced.
        }

        var numberOfCallsToHere = Program.NumberOfCalls;
        var key = (current.Number,openedBm,timeLeft,elephant);
        if (cache.ContainsKey(key))
        {
            var cached = cache[key];
            Program.SavedByCache += cached.calls;
            Program.NumberOfCalls += cached.calls;
            return cached.flow; // we have been at this state before, no need to calculate subtree again.
        }

        var maxFlow = 0;
        if (current.Flow > 0 && openedBm.IsBitClear(current.Number))
        {
            maxFlow = current.Flow * (timeLeft - 1); // this node will produce 
            maxFlow += MaxFlow(current, openedBm.SetBit(current.Number), timeLeft - 1, elephant, useElephant, productiveNodesCount, startNode, cache);
        }
        foreach (var edge in current.Edges.Values)
        {
            var flow = MaxFlow(edge.To, openedBm, timeLeft - 1, elephant, useElephant, productiveNodesCount, startNode, cache);
            maxFlow = Math.Max(maxFlow, flow);
        }

        var numberOfCallsBelow = Program.NumberOfCalls - numberOfCallsToHere;

        cache[key] = (flow:maxFlow,calls:numberOfCallsBelow);
        return maxFlow;
    }



    private static Dictionary<string, Node> LoadFile(TextReader stream)
    {
        // 0     1  2    3   4   5    6      7    8    9   0   1     2
        //Valve AA has flow rate=0; tunnels lead to valves DD, II, BB

        var nodeList = new Dictionary<string, Node>();
        var i = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Split(" =,;".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var name = parts[1];
            var flow = parts[5].ToInt()!.Value;
            var connected = parts.Skip(10).ToList();
            nodeList[name] = new Node(name, flow, connected,i);
            i++;
        }
        nodeList.Values.ForEach((n, _) =>
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
    public int Number { get; set; }
    public int Flow { get; }
    public Dictionary<string, Edge> Edges { get; } = new();

    public Node(string name, int flow, List<string> connected, int number)
    {
        _connected = connected;
        Name = name;
        Flow = flow;
        Number = number;
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

public static class BitCounter
{
    private static int[] BitsSetTable256 { get; } = new int[256];
    static BitCounter()
    {
        BitsSetTable256[0] = 0;
        for (var i = 0; i < 256; i++)
        {
            BitsSetTable256[i] = ((i & 1) + BitsSetTable256[i / 2]);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(ulong bm)
    {
        var bytes = BitConverter.GetBytes(bm);
   //     return bytes.Sum(b => BitsSetTable256[b]);
      return BitsSetTable256[bytes[0]]
             + BitsSetTable256[bytes[1]]
             + BitsSetTable256[bytes[2]]
             + BitsSetTable256[bytes[3]];

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitClear(this ulong openedBm, int bit)
    {
        return (openedBm & (1ul << bit)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SetBit(this ulong openedBm, int bit)
    {
        return (openedBm | (1ul << bit));
    }
}


