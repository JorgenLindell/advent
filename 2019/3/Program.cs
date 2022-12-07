using System.Diagnostics;
using common;

//https://adventofcode.com/2019/day/3

internal class Program
{
    private static string _testData =
        @"R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51
U98,R91,D20,R16,D67,R40,U7,R15,U6,R7" //159
.Replace("\r\n", "\n");



    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }
    private static void SecondPart(TextReader stream)
    {
        Dictionary<(int r, int c), int> crossings = RunLines(stream);

        var orderedEnumerable = crossings
            .OrderBy(x => x.Value).ToList();
        var closest = orderedEnumerable.First().Value;

        Debug.WriteLine($"result2:{closest} ");

    }


    private static void FirstPart(TextReader stream)
    {
        Dictionary<(int r, int c), int> crossings = RunLines(stream);

        var orderedEnumerable = crossings
            .OrderBy(x => x.Key.ManhattanDistance((0, 0))).ToList();
        var closest = orderedEnumerable.First().Key
            .ManhattanDistance((0, 0));

        Debug.WriteLine($"result1:{closest} ");
    }

    private static Dictionary<(int r, int c), int> RunLines(TextReader stream)
    {
        var crossings = new Dictionary<(int r, int c), int>();
        DictionaryWithDefault<(int r, int c), int> visited = new(x => 0);

        while (stream.ReadLine() is { } inpLine)
        {
            DictionaryWithDefault<(int r, int c), int> thisVisited = new(x => 0);
            var current = (0, 0);
            var steps = 0;
            inpLine
                .Split(',').ToList()
                .ForEach(
                    (mov) =>
                    {
                        var cells = Move.Moves(current, mov).ToList();
                        cells.ForEach(cell =>
                        {
                            steps++;
                            current = cell;
                            if (thisVisited[cell] != 0)
                                return;

                            thisVisited[cell] = steps;
                            if (visited[cell] > 1)
                            {
                                crossings[cell] = visited[cell] + steps;
                            }
                            else
                            {
                                visited[cell] = steps;
                            }
                        });
                    });
        }

        return crossings;
    }
}

public static class Move
{
    internal static Dictionary<char, (int r, int c)> Directions = new()
    {
        ['U'] = (1, 0),
        ['D'] = (-1, 0),
        ['R'] = (0, 1),
        ['L'] = (0, -1),

    };
    public static IEnumerable<(int r, int c)> Moves((int r, int c) start, string move)
    {
        var delta = Directions[move[0]];
        var length = (new string(move.Skip(1).ToArray())).ToInt()!.Value;
        for (int i = 0; i < length; i++)
        {
            start.r += delta.r;
            start.c += delta.c;
            yield return start;
        }
        yield break;
    }

    public static int ManhattanDistance(this (int r, int c) start, (int r, int c) end)
    {
        return (Math.Abs(start.r - end.r) + Math.Abs(start.c - end.c));
    }
}