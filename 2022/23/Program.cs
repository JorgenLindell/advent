using System.Diagnostics;
using common;
using common.SparseMatrix;
using Direction = common.SparseMatrix.Direction;


//https://adventofcode.com/2022/day/23
internal class Program
{
    private static readonly string _testData =
        @"....#..
..###.#
#...#.#
.#...##
#.###..
##.#.##
.#..#.."
            //@""
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        var debug = false;
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
        var elves = Load(stream).ToDictionary(x => x.Name, x => x);
        Elf.Elves = elves;
        var rulesIndex = (int)Direction.N;
        var round = 0;
        while (true)
        {
            round++;
         // Debug.WriteLine($"Round {round} ========================");
            var requests = elves.Values
                .Select(elf => elf.WantToGo(rulesIndex))
                .Where(x=>x.Pos != x.Elf.Pos)// want to stay put
                .ToList();

            var requested = requests
                .GroupBy(x => x.Pos)
                .Select(x => x.ToList())
                .ToList();

            var toMove = requested.Where(x => x.Count == 1).ToList();
            if (toMove.Count == 0)
                break;

            // requested.Where(x => x.Count > 1).ToList()
            //     .ForEach(g =>
            // {
            //     g.ForEach(r => Debug.WriteLine($"cont {r.Pos} {r.Elf} "));
            // });

            toMove.ForEach((x, _) => x.First().Elf.Move(x.First().Pos));

            if (round == 10)
            {
                var (minY, maxY, minX, maxX) = MinMaxElves(elves);
                var area = ((maxX - minX + 1) * (maxY - minY + 1)) - elves.Count;
                Debug.WriteLine("Solution 1, area= " + area);
            }

            PrintOut(round, elves);
            rulesIndex = ++rulesIndex % 4;
        }
        Debug.WriteLine("Stopped after round= " + round);
    }

    private static void PrintOut(int round, Dictionary<string, Elf> elves)
    {
        return;
        var (minY, maxY, minX, maxX) = MinMaxElves(elves);
        Debug.WriteLine($"Round {round}   ({minX},{minY}) -> ({maxX},{maxY})");
        for (long y = maxY; y >= minY; y--)
        {
            Debug.Write($"{y:#0} ");
            for (long x = minX; x <= maxX; x++)
            {
                if (Elf.IsOccupied((Position)(x, y)))
                {
                    Debug.Write("#");
                }
                else
                {
                    Debug.Write(".");
                }
            }
            Debug.WriteLine("");
        }
    }

    private static (long minY, long maxY, long minX, long maxX) MinMaxElves(Dictionary<string, Elf> elves)
    {
        var firstElf = elves.Values.First().Pos;
        var minY = firstElf.Y;
        var maxY = firstElf.Y;
        var minX = firstElf.X;
        var maxX = firstElf.X;
        foreach (var elf in elves.Values)
        {
            minY = Math.Min(minY, elf.Pos.Y);
            maxY = Math.Max(maxY, elf.Pos.Y);
            minX = Math.Min(minX, elf.Pos.X);
            maxX = Math.Max(maxX, elf.Pos.X);
        }

        return (minY, maxY, minX, maxX);
    }

    private static List<Elf> Load(TextReader stream)
    {
        var list = new List<Elf>();
        var lines = new List<string>();
        while (stream.ReadLine() is { } inpLine)
            lines.Add(inpLine);
        Debug.WriteLine("Read lines=" + lines.Count);
        Debug.WriteLine("Total cells==" + lines.Count * lines.First().Length);

        for (var lineIx = 0; lineIx < lines.Count; lineIx++)
        {
            var line = lines[lineIx];
            for (var index = 0; index < line.Length; index++)
            {
                Position pos = (Position)(index, lines.Count - lineIx);
                var c = line[index];
                if (c == '#')
                {
                    var elf = new Elf(pos);
//                    Debug.WriteLine($"Adding {elf.Name} at {elf.Pos}");
                    list.Add(elf);
                }
            }
        }
        Debug.WriteLine("Total Elves=" + list.Count);

        return list;
    }
}

internal class Elf
{
    private static int _elfSequence;
    private static Dictionary<string, Elf> _elves;
    private static Dictionary<Position, Elf> _elvesPositions;

    public Elf(Position pos)
    {
        Pos = pos;
        Name = GetNewName();
    }

    public Position Pos { get; set; }
    public string Name { get; set; }

    public static Dictionary<string, Elf> Elves
    {
        get => _elves;
        set
        {
            _elves = value;
            _elvesPositions = value.Values.ToDictionary(x => x.Pos, x => x);
        }
    }

    private static string GetNewName()
    {
        var bas = new int[3];
        _elfSequence %= (26 * 26 * 26);
        bas[2] += _elfSequence;
        if (bas[2] > 25)
            bas[1] += bas[2] / 26;
        if (bas[1] > 25)
            bas[0] += bas[1] / 26;
        for (var i = 0; i < bas.Length; i++)
        {
            bas[i] %= 26;
            bas[i] += 'A';
        }

        _elfSequence++;
        return string.Join("", bas.Select(x => (char)x).ToArray());
    }

    public override string ToString()
    {
        return $"{Name} at {Pos}";
    }

    public (Position Pos, Elf Elf) WantToGo(int rulesIndex)
    {
        if (Position.AllDirections.All(o=> IsOccupied(Pos + o)==false))
        {
       //     Debug.WriteLine($"No action at {Pos}");
            return (Pos, this);
        }

        var rule = (Direction)((rulesIndex) % 4);
//        Debug.WriteLine($"Prefering {rule} at {Pos}");
        var pos = Pos;
        for (var i = 0; i < 4; i++)
        {
            rule = (Direction)((i + rulesIndex) % 4);
//            Debug.WriteLine($"  {Name} testing {rule}");
            switch (rule)
            {
                case Direction.N:
                    if (IsOccupied(Pos.PosNorthEast) || IsOccupied(Pos.PosNorth) || IsOccupied(Pos.PosNorthWest))
                        continue;
                    pos = Pos.PosNorth;
                    break;
                case Direction.S:
                    if (IsOccupied(Pos.PosSouthEast) || IsOccupied(Pos.PosSouth) || IsOccupied(Pos.PosSouthWest))
                        continue;
                    pos = Pos.PosSouth;
                    break;
                case Direction.W:
                    if (IsOccupied(Pos.PosNorthWest) || IsOccupied(Pos.PosWest) || IsOccupied(Pos.PosSouthWest))
                        continue;
                    pos = Pos.PosWest;
                    break;
                case Direction.E:
                    if (IsOccupied(Pos.PosNorthEast) || IsOccupied(Pos.PosEast) || IsOccupied(Pos.PosSouthEast))
                        continue;
                    pos = Pos.PosEast;
                    break;
            }

            if (Pos != pos) break;
        }

        return (pos, this);
    }

    public static bool IsOccupied(Position pos)
    {
        return _elvesPositions.ContainsKey(pos);
    }

    public void Move(Position pos)
    {
        if (_elvesPositions.ContainsKey(Pos))
            _elvesPositions.Remove(Pos);
        Pos = pos;
        _elvesPositions[Pos] = this;
    }
}