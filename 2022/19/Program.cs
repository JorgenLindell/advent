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

    private static readonly bool _debug = false;

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
        var lines = Load(getDataStream());

        var blueprints = new List<Blueprint>();
        lines.ForEach(x => blueprints.Add(Blueprint.Parse(x)));

        Debug.WriteLine($"=============== Part 2: =================");

        List<long> results = new();
        foreach (var blueprint in blueprints.Take(3))
        {
            var geodes = blueprint.Evaluate(32, out var builtRobots);
            results.Add(geodes);
            Debug.WriteLine($"Blueprint {blueprint.Id}= {geodes}  quality= {geodes * blueprint.Id}");
            foreach (var builtRobot in builtRobots.Take(3))
            {
                //              Debug.WriteLine($"{builtRobot.Item1} {builtRobot.Item2}");
            }
        }
        Debug.WriteLine($"Product of 3 first of all blueprints {results[0] * results[1] * results[2]}");

    }


    private static void FirstPart(Func<TextReader> getDataStream)
    {
        var lines = Load(getDataStream());

        var blueprints = new List<Blueprint>();
        lines.ForEach(x => blueprints.Add(Blueprint.Parse(x)));



        Debug.WriteLine($"=============== Part 1: =================");
        List<long> results = new();

        foreach (var blueprint in blueprints)
        {
            var geodes = blueprint.Evaluate(24, out var builtRobots);
            results.Add(geodes * blueprint.Id);
            Debug.WriteLine($"Blueprint {blueprint.Id}= {geodes}  quality= {geodes * blueprint.Id}");
            foreach (var builtRobot in builtRobots)
            {
                //             Debug.WriteLine($"{builtRobot.Item1} {builtRobot.Item2}");
            }
        }
        Debug.WriteLine($"Sum of all blueprints {results.Sum()}");

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

