using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using common;
using common.SparseMatrix;


//https://adventofcode.com/2022/day/22
internal class Program
{
    private static readonly string _testData =
        @"        ...#
        .#..
        #...
        ....
...#.......#
........#...
..#....#....
..........#.
        ...#....
        .....#..
        .#......
        ......#.

10R5L5R10L4R5L5"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        var debug = true;
        FirstPart(GetDataStream(debug), debug);
        SecondPart(GetDataStream(debug), debug);
    }

    private static TextReader GetDataStream(bool debug)
    {
        return debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");
    }

    private static void SecondPart(TextReader stream, bool debug)
    {
    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        var map = Load(stream);
        Debug.WriteLine("Stopped after round= ");
    }

    private static void PrintOut(MonkeyMap map)
    {
        var (minY, maxY, minX, maxX) = MinMaxElves(map);
        Debug.WriteLine($"Printout  ({minX},{minY}) -> ({maxX},{maxY})  StartPos: {map.StartPos}");
        for (long y = maxY; y >= minY; y--)
        {
            Debug.Write($"{y,4} ");
            for (long x = minX; x <= maxX; x++)
            {
                Position position = (x, y);
                if (!map.IsEmpty(position))
                {
                    var tile = map.Value(position);
                    Debug.Write(tile!.Symbol);
                }
                else
                {
                    Debug.Write(" ");
                }
            }
            Debug.WriteLine("");
        }
    }

    private static (long minY, long maxY, long minX, long maxX) MinMaxElves(MonkeyMap map)
    {
        var firstElf = map.Values.First().Pos;
        var minY = firstElf.Y;
        var maxY = firstElf.Y;
        var minX = firstElf.X;
        var maxX = firstElf.X;
        foreach (var elf in map.Values)
        {
            minY = Math.Min(minY, elf.Pos.Y);
            maxY = Math.Max(maxY, elf.Pos.Y);
            minX = Math.Min(minX, elf.Pos.X);
            maxX = Math.Max(maxX, elf.Pos.X);
        }

        return (minY, maxY, minX, maxX);
    }

    private static MonkeyMap Load(TextReader stream)
    {
        var map = Tile.Map = new MonkeyMap();
        var lineIx = 0;
        var mapWidth = 0;
        int start;
        int end;
        while (stream.ReadLine() is { } inpLine)
        {
            if (inpLine == "") break;
            mapWidth = Math.Max(mapWidth, inpLine.Length);
            lineIx--;
            var ix = lineIx;
            start = 0;
            end = inpLine.Length;
            int i = 0; for (; i < inpLine.Length && inpLine[i] == ' '; i++) { }

            if (i < inpLine.Length)
            {
                start = i;
            }
            i = inpLine.Length - 1; for (; i > 0 && inpLine[i] == ' '; i--) { }
            end = i;
            if (ix == -1)
                map.StartPos = new Position(start, ix);

            Enumerable.Range(start, end - start + 1).ForEach(
                (p, _) =>
                {
                    Position pos;
                    if (inpLine[p] != '.')
                    {
                        pos = new Position(p, ix);
                        map.Value(pos, new Tile(Tile.EWall, pos));
                        return;
                    }

                    pos = new Position(p, ix);
                    map.Value(pos, new Tile(Tile.EFree, pos));
                });

            var startPos = new Position(start - 1, ix);
            while (map.IsEmpty(startPos.PosEast))
                startPos = startPos.PosEast;

            var endPos = new Position(end + 1, ix);
            while (map.IsEmpty(endPos.PosWest))
                endPos = endPos.PosWest;

            map.Value(startPos, new Tile(Tile.EEdge, startPos, endPos));
            map.Value(endPos, new Tile(Tile.EEdge, endPos, startPos));
        }
        start = (int)map.Values.Min(x => x.Pos.X);
        end = (int)map.Values.Max(x => x.Pos.X);
        var last = (int)map.Values.Min(x => x.Pos.Y);
        for (int x = start + 1; x < end; x++)
        {
            var startPos = new Position(x, 0);
            while (map.IsEmpty(startPos.PosSouth)
                   ||map.Value(startPos.PosSouth)!.Typ==Tile.Types.Edge)
                startPos = startPos.PosSouth;
            var endPos = new Position(x, last - 1);
            while (map.IsEmpty(endPos.PosNorth)
                   || map.Value(endPos.PosNorth)!.Typ == Tile.Types.Edge)
                endPos = endPos.PosNorth;
            map.Value(startPos, new Tile(Tile.EEdge, startPos, endPos));
            map.Value(endPos, new Tile(Tile.EEdge, endPos, startPos));
        }
        Debug.WriteLine($"Read lines={(-lineIx) - 1}");
        PrintOut(map);
        var instructionLine = stream.ReadLine();
        var str = "";
        foreach (var c in "" + instructionLine)
        {
            if (c.In('R', 'L'))
            {
                map.Instructions.Add(new Instruction(str.ToInt()!.Value, c));
                str = "";
            }
            else
            {
                str += c;
            }

        }
        return map;
    }
}

internal class Instruction
{
    public enum Rotate{ Left=-1, Right=+1}
    public int Steps { get; }
    public Rotate Turn { get; }

    public Instruction(int steps, char turn)
    {
        Steps = steps;
        Turn = turn == 'L' ? Rotate.Left : Rotate.Right;
    }

    public override string ToString() => $"{Steps} {Turn}";
}

internal class Tile
{

    public static MonkeyMap Map { get; set; }
    public Position Pos { get; private set; }
    public Types Typ { get; }

    public Tile(Types typ, Position pos, Position endPos)
    {
        Pos = pos;
        Typ = typ;

        Map.AddEdge(pos, endPos);
    }
    public Tile(Types typ, Position pos)
    {
        Pos = pos;
        Typ = typ;
    }

    public enum Types
    {
        None,
        Edge,
        Wall
    }
    public static Types EEdge => Types.Edge;
    public static Types EWall => Types.Wall;
    public static Types EFree => Types.None;

    public string Symbol => Typ == EEdge ? "E" : Typ == EWall ? "#" : Typ == EFree ? "." : "?";
}

internal class MonkeyMap : SparseMatrix<Position, Tile>
{
    public List<Instruction> Instructions { get; } = new();
    public Dictionary<Position, (Position Horizontal, Position Vertical)> EdgeConnections { get; } = new();
    public Position StartPos { get; set; }

    public void AddEdge(Position pos, Position endPos)
    {
        if (!EdgeConnections.ContainsKey(pos))
            EdgeConnections[pos] = default;
        var connect = EdgeConnections[pos];
        if (pos.X == endPos.X)
        {
            connect.Horizontal = endPos;
        }
        else if (pos.Y == endPos.Y)
        {
            connect.Vertical = endPos;
        }
        else
        {
            throw new InvalidDataException("Inconsistent edge");
        }
        EdgeConnections[pos] = connect;
    }
}