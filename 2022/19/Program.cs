using System.Diagnostics;
using _19;
using common;


//https://adventofcode.com/2022/day/23
internal class Program
{
    private static readonly string _testData =
        @"Blueprint 1: Each ore robot costs 4 ore. Each clay robot costs 2 ore. Each obsidian robot costs 3 ore and 14 clay. Each geode robot costs 2 ore and 7 obsidian.
Blueprint 2:Each ore robot costs 2 ore.  Each clay robot costs 3 ore.  Each obsidian robot costs 3 ore and 8 clay.  Each geode robot costs 3 ore and 12 obsidian."
            //@""
            .Replace("\r\n", "\n");

    private static readonly bool _debug = true;

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream);
        SecondPart(GetDataStream);
    }

    private static TextReader GetDataStream()
    {
        return _debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");
    }

    private static void SecondPart(Func<TextReader> getDataStream)
    {
    }


    private static void FirstPart(Func<TextReader> getDataStream)
    {
        var lines = Load(getDataStream());

        var blueprints = new List<Blueprint>();
        lines.ForEach(x => blueprints.Add(Blueprint.Parse(x)));


        foreach (var blueprint in blueprints)
        {
            var geodes = blueprint.Evaluate(24);
            Debug.WriteLine($"Blueprint {blueprint.Id}={geodes} ");

        }
    }

   


    private static List<string> Load(TextReader stream)
    {
        var lines = new List<string>();
        while (stream.ReadLine() is { } inpLine)
            lines.Add(inpLine);
        Debug.WriteLine("Read lines=" + lines.Count);
        return lines;
    }
}

