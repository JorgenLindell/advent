using System.Diagnostics;
using common;
using common.SparseMatrix;


//https://adventofcode.com/2022/day/21
internal class Program
{
    private static readonly string _testData =
        @"2,2,2
1,2,2
3,2,2
2,1,2
2,3,2
2,2,1
2,2,3
2,2,4
2,2,6
1,2,5
3,2,5
2,1,5
2,3,5"
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
        var matrix = Load(stream);
        var (min, max) = matrix.MinMax;
        var midTop = (x: (min.x + max.x) / 2, y: (min.y + max.y) / 2, z: max.z + 1);
        while (matrix.IsEmpty(midTop)) midTop.z--;
        midTop.z++;
        var drop = new Drop(Drop.Gas);
        matrix.Value(midTop, drop);
        var gasDrops = new List<Drop> { drop };
        var didAdd = true;

        while (didAdd)
        {
            didAdd = false;
            var drops = gasDrops.ToList();

            drops.ForEach(x =>
            {
                var lookedAt = new Stack<(long x, long y, long z)>();
                CheckForEmptyAdjacent(x.Coord, 2, lookedAt);
            });
            didAdd = drops.Count != gasDrops.Count;
        }
        var sumOfExposed = matrix.Values.Where(x=>x.Typ==Drop.DropType.Lava).Sum(x => x.AdjacentDrops.Count(y => y.Typ==Drop.DropType.Gas));

        Debug.WriteLine("Exposed outer sides=" + sumOfExposed);

        // Just curious: How many contained empty cells are there in total?
        // check for any lava next to empty

        var nextToHoles = matrix.Values
            .Where(x => x.Typ == Drop.Lava && x.AdjacentDrops.Count < 6)
            .Select(x => x.Coord);

        Debug.WriteLine("Exposed to inside (for curiosity) =" + matrix.Values
            .Where(x => x.Typ == Drop.Lava && x.AdjacentDrops.Count < 6).Sum(x=>6- x.AdjacentDrops.Count)
        );

        // and their positions?
        var holesSet = new HashSet<(long x, long y, long z)>();

        foreach (var nextToHole in nextToHoles)
        {
            var holes = matrix.GetAdjacentHoles(nextToHole);
            holes.ForEach((x,_)=>holesSet.Add(x));
        }

        var anyNew = true;
        while (anyNew)
        {
            anyNew = false;
            foreach (var hole in holesSet.ToList())
            foreach (var newHole in matrix.GetAdjacentHoles(hole))
                anyNew = holesSet.Add(newHole) || anyNew;
        }

        Debug.WriteLine("Inside holes (for curiosity)  =" + holesSet.Count);


        // Local ========================
        bool CheckForEmptyAdjacent((long x, long y, long z) coord, int level, Stack<(long x, long y, long z)> lookedAt)
        {
            if (level < 1)
                return false;

            lookedAt.Push(coord);
            var addedGas = false;
            var holes = matrix.GetAdjacentHoles(coord)
                .Where(x => !lookedAt.Contains(x))
                .ToList();

            foreach (var hole in holes)
            {
                var adjacentToEmpty = matrix.GetAdjacent(hole);
                var nextToLava = adjacentToEmpty.Any(y => y.Typ == Drop.DropType.Lava);

                var addedBelow = false;
                if (level > 1)
                {
                    addedBelow = CheckForEmptyAdjacent(hole, level - 1, lookedAt);
                    addedGas = addedBelow || addedGas;
                }

                if (matrix.IsEmpty(hole) && (nextToLava || addedBelow))
                {
                    var added = matrix.Value(hole, new Drop(Drop.DropType.Gas))!;
                    gasDrops.Add(added!);
                }
            }

            lookedAt.Pop();
            return addedGas;
        }
    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        var matrix = Load(stream);
        var sumOfExposed = matrix.Values.Sum(x => 6 - x.AdjacentDrops.Count);

        Debug.WriteLine("Exposed sides=" + sumOfExposed);
    }

    private static Matrix Load(TextReader stream)
    {
        var matrix = new Matrix();
        while (stream.ReadLine() is { } inpLine)
        {
            var drop = new Drop(inpLine);
            matrix.Value(drop.Coord, drop);
        }

        return matrix;
    }
}

internal class Drop
{
    public enum DropType
    {
        Lava,
        Gas
    }

    public const DropType Gas = DropType.Gas;
    public const DropType Lava = DropType.Lava;
    public HashSet<Drop> AdjacentDrops = new();

    public Drop(string inpLine, DropType type = Lava)
    {
        Typ = type;
        var parts = inpLine.Split(',');
        Coord = (parts[0].ToLong()!.Value, parts[1].ToLong()!.Value, parts[2].ToLong()!.Value);
    }

    public Drop(DropType typ)
    {
        Typ = typ;
    }

    public Drop(DropType typ, (long x, long y, long z) coord)
    {
        Typ = typ;
        Coord = coord;
    }

    public DropType Typ { get; set; }

    public (long x, long y, long z) Coord { get; set; }
}

internal class Matrix : SparseMatrix<(long x, long y, long z), Drop>
{
    public static (int x, int y, int z)[] Adjacency =
    {
        (0, 0, -1), (0, 0, +1),
        (0, -1, 0), (0, +1, 0),
        (-1, 0, 0), (+1, 0, 0)
    };

    public new ((long x, long y, long z) Min, (long x, long y, long z) Max) MinMax => 
        base.MinMax((point, acc) =>
    {
        acc.Min.x = Math.Min(acc.Min.x, point.x);
        acc.Min.y = Math.Min(acc.Min.y, point.y);
        acc.Min.z = Math.Min(acc.Min.z, point.z);
        acc.Max.x = Math.Max(acc.Max.x, point.x);
        acc.Max.y = Math.Max(acc.Max.y, point.y);
        acc.Max.z = Math.Max(acc.Max.z, point.z);
        return acc;
    });

    public override Drop Value((long x, long y, long z) key, Drop drop)
    {
        drop.Coord = key;
        base.Value(key, drop);
        UpdateAdjecency(drop);
        return drop;
    }

    public void UpdateAdjecency(Drop drop)
    {
        drop.AdjacentDrops.Clear();
        foreach (var offset in Adjacency)
        {
            var p = Offset(drop.Coord, offset);
            if (!IsEmpty(p))
            {
                var adjDrop = Value(p);
                if (adjDrop == null) 
                    continue;
                drop.AdjacentDrops.Add(adjDrop);
                adjDrop.AdjacentDrops.Add(drop);
            }
        }
    }

    public List<Drop> GetAdjacent((long x, long y, long z) coord)
    {
        var adjacent=new List<Drop>();
        foreach (var offset in Adjacency)
        {
            var p = Offset(coord, offset);
            if (!IsEmpty(p)) adjacent.Add(Value(p)!);
        }
        return adjacent;
    }

    public List<(long x, long y, long z)> GetAdjacentHoles((long x, long y, long z) coord)
    {
        var empty = new List<(long x, long y, long z)>();
        foreach (var offset in Adjacency)
        {
            var p = Offset(coord, offset);
            if (IsEmpty(p))
                //p is one empty
                empty.Add(p);
        }
        return empty;
    }

    public static (long x, long y, long z) Offset(
        (long x, long y, long z) point,
        (long x, long y, long z) offset)
    {
        return (point.x + offset.x, point.y + offset.y, point.z + offset.z);
    }
}