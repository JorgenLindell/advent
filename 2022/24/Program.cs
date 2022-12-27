using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks.Sources;
using common;
using common.SparseMatrix;


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
        var stream = getDataStream();
        var lines = Load(stream);

        var width = lines.First().Length - 2;
        var height = lines.Count - 2;


        var start = lines[0].IndexOf("#.##") ;
        var end = lines[height + 1].IndexOf("##.#") + 1;
        var startPos = new Position(start, -1);
        var endPos = new Position(end, height+1);
        var matrix = new Matrix(new(width - 1, height - 1));
        Blizzard.Matrix = Walker.Matrix = matrix;

        var blizzards = matrix.Blizzards;

        for (int y = 1; y < height + 1; y++)
        {
            for (int x = 1; x < width + 1; x++)
            {
                var c = lines[y][x];
                Blizzard? blizzard = c switch
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
        }
        var walker = new Walker(startPos, endPos.PosNorth);

        int minute = 0;
        int veryBest = int.MaxValue;
        veryBest = DoMoves(minute, veryBest) + 1;




        Debug.WriteLine($"Best = " + veryBest);

        int DoMoves(int nextMinute, int bestResult)
        {
            matrix.PrintOut(walker.Pos);
            if (walker.Pos != walker.EndPos)
            {
                nextMinute++;
                blizzards.ForEach(b => b.Move());
                var moves = walker.Moves();
                var myBest = bestResult;
                foreach (var position in moves)
                {
                    var walkerPos = walker.Pos;
                    walker.Pos = position;
                    var result = DoMoves(nextMinute, bestResult);
                    myBest = Math.Min(myBest, result);
                    walker.Pos = walkerPos;
                }
                blizzards.ForEach(b => b.MoveBack());
                return Math.Min(bestResult, myBest);
            }

            return Math.Min(bestResult, nextMinute);
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

internal class Matrix : DictionaryWithDuplicates<Position, Blizzard>
{
    public Position? Min { get; private set; }
    public Position? Max { get; private set; }

    public Position LimitMin { get; private set; }
    public Position LimitMax { get; private set; }
    public (Position min, Position max) Limits => (LimitMin, LimitMax);
    public List<Blizzard> Blizzards { get; private set; } = new();

    public Matrix(Position limitMin, Position limitMax)
    {
        LimitMin = limitMin;
        LimitMax = limitMax;
    }

    public Matrix(Position position)
    : this(new(0, 0), position)
    {
    }

    public override void Value(Position pos, Blizzard newValue)
    {
        var (x, y) = pos;
        if (Min == null)
            Min = new Position(x, y);
        if (Max == null)
            Max = new Position(x, y);
        Min.Y = Math.Min(y, Min.Y);
        Min.X = Math.Min(x, Min.X);
        Max.Y = Math.Max(y, Max.Y);
        Max.X = Math.Max(x, Max.X);
        base.Value(pos, newValue);
    }

    public void PrintOut(Position walkerPos)
    {
        var sb = new StringBuilder();
        if (Max.Y - Min.Y < 10)
        {
            sb.AppendLine("------------------------");
            for (long y = Min.Y; y <= Max.Y; y++)
            {
                for (long x = Min.X; x <= Max.X; x++)
                {
                    string value;
                    var position = new Position(x, y);
                    if (position == walkerPos)
                        value = "W";
                    else if (IsEmpty(position))
                        value = ".";
                    else
                    {
                        var hashSet = Value(position);
                        if (hashSet.Count > 1)
                            value = "" + hashSet.Count;
                        else
                        {
                            value = "" + hashSet.First().Symbol;
                        }
                    }

                    sb.Append(value);
                }

                sb.AppendLine();
            }
            Debug.WriteLine(sb.ToString());
        }
    }
}


internal class Walker
{
    public static Matrix? Matrix { get; set; }
    public Position StartPos { get; set; }
    public Position EndPos { get; set; }
    public Position Pos { get; set; }

    public Walker(Position startPos, Position endPos)
    {
        StartPos = startPos;
        EndPos = endPos;
        Pos = startPos;
    }

    public List<Position<Position>> Moves()
    {
        var allPos = NeighboursPositions(Pos);
        allPos.Add(Pos);

        return allPos.Where(p =>
        {
            var neighboursPositions = NeighboursPositions(p);
            var blizzards = neighboursPositions.SelectMany(n => Matrix!.Value(n)).ToList();
            return blizzards.All(b => b.NextPos() != p);
        }).ToList();

    }

    private List<Position<Position>> NeighboursPositions(Position position)
    {
        return Position.Offsets
            .Select(o => (position + o))
            .Where(p => !p.Outside(Matrix!.Limits))
            .ToList();
    }
}

internal class Blizzard
{
    public static Matrix Matrix { get; set; }
    public Offset Direction { get; }
    public Position Pos { get; set; }

    public Blizzard(int x, int y, Offset direction, char c)
    {
        Direction = direction;
        Pos = new Position(x, y);
        Symbol = c;
    }

    public char Symbol { get; set; }

    public void Move()
    {
        Matrix.Remove(Pos, this);
        Pos = NextPos();
        Matrix.Value(Pos, this);

    }
    public void MoveBack()
    {
        Matrix.Remove(Pos, this);
        Pos = PrevPos();
        Matrix.Value(Pos, this);
    }
    public Position NextPos()
    {
        var nextPos = Pos + Position.Offsets[(int)Direction];
        nextPos = nextPos.Wrap(Matrix.LimitMin, Matrix.LimitMax);
        return nextPos;
    }
    public Position PrevPos()
    {
        var nextPos = Pos - Position.Offsets[(int)Direction];
        nextPos = nextPos.Wrap(Matrix.LimitMin, Matrix.LimitMax);
        return nextPos;
    }

}

