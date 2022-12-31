using System.Diagnostics;
using System.Reflection.Emit;
using _24;
using common;


//https://adventofcode.com/2022/day/23
internal class Program
{
    private static readonly string _testData =
        @"#.######
#>>.<^<#
#.<..<<#
#>v.><>#
#<^v^^>#
######.#"
            //@""
            .Replace("\r\n", "\n");

    private static readonly bool _debug = false;

    private static void Main(string[] args)
    {
        PositionBase.NorthIsNegative = true;
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
        var (startPos, endPos) = Setup(getDataStream, out var matrix);

        matrix.PrintOut(0, startPos, new List<Position>(), 0);

        // walk 3 "legs" back and forth
        var walker = new Walker(startPos, endPos, 3);
        var best1 = walker.DoMoves(0, bestResult: 100000, startPos,leg: 1, startPos, endPos,level: 0);

        Console.WriteLine($" 2 Best = {best1}");
    }

    private static void FirstPart(Func<TextReader> getDataStream)
    {
        var (startPos, endPos) = Setup(getDataStream, out var matrix);

        matrix.PrintOut(0, startPos, new List<Position>(), 0);

        var walker = new Walker(startPos, endPos, 1);
        var veryBest = walker.DoMoves(0, bestResult:100000, startPos, leg:1, startPos, endPos,level: 0);

        Console.WriteLine(" 1 Best = " + veryBest);

        // Local 
    }

    private static (Position start, Position end) Setup(Func<TextReader> getDataStream, out Matrix matrix)
    {
        var stream = getDataStream();
        var lines = Load(stream);

        var width = lines.First().Length - 2;
        var height = lines.Count - 2;


        var start = lines[0].IndexOf("#.##", StringComparison.Ordinal);
        var end = lines[height + 1].IndexOf("##.#", StringComparison.Ordinal) + 1;
        var startPos = new Position(start, -1);
        Position endPos = new Position(end, height);
        matrix = new Matrix(new Position(width - 1, height - 1));
        Blizzard.Matrix = Walker.Matrix = matrix;

        var blizzards = matrix.Blizzards;

        for (var y = 1; y < height + 1; y++)
            for (var x = 1; x < width + 1; x++)
            {
                var c = lines[y][x];
                var blizzard = c switch
                {
                    '^' => new Blizzard(x - 1, y - 1, Offset.N, c),
                    '>' => new Blizzard(x - 1, y - 1, Offset.E, c),
                    'v' => new Blizzard(x - 1, y - 1, Offset.S, c),
                    '<' => new Blizzard(x - 1, y - 1, Offset.W, c),
                    _ => null
                };
                if (blizzard != null)
                {
                    blizzards.Add(blizzard);
                    matrix.Value(blizzard.Pos, blizzard);
                }
            }

        return (startPos, endPos);
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