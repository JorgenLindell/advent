using System.Diagnostics;
using System.Text;
using common;


//https://adventofcode.com/2022/day/22
internal class Program
{
    private static readonly string _testData =
@"
        ...#
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
            .Replace("\r\n", "\n").Substring(1);
    static bool _debug = true;

    private static void Main(string[] args)
    {
        FirstPart();
        SecondPart();
    }

    private static TextReader GetDataStream()
    {
        return _debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");
    }

    private static void SecondPart()
    {
        var map = MonkeyMap.Load(GetDataStream);
        PrintOut(map, false);
        var gPos = new GlobalPosition(99, -55);
        var lpos = map.Side(gPos);

        var pos = lpos.Pos.PosEast;
        Offset wasGoing = Offset.E;
        Offset to = Offset.N;

        var newLPos = map.FlipCoordinates(wasGoing, to, pos, out int rot);
        gPos = map.GlobalPos("c", newLPos);
        Debug.WriteLine($"{pos} {wasGoing}->{to} {rot} = {newLPos}  {gPos}");
        //   foreach (var from in Enum.GetValues<Offset>())
        //   {
        //       foreach (var to in Enum.GetValues<Offset>())
        //       {
        //           Debug.WriteLine($"{pos} {from}->{to}={map.FlipCoordinates(from, to, pos)}");
        //       }
        //   }


        var x = map.Value(new GlobalPosition(59, -100));
        var t1 = x.Pos;
        var t2 = x.PosInSide;

        TransformEdges(map);

        Walker me = new Walker(map.StartPos, Offset.E, map);
        foreach (var instr in map.Instructions)
        {
            me.Execute(instr);
        }

        Debug.WriteLine($"Walker at = {me.Pos}");
        //   me.Track.ForEach(
        //       x => Debug.WriteLine(x));
        var row = me.Pos.Y * -1;
        var col = me.Pos.X + 1;
        var faces = new[] { 3, 0, 1, 2, };
        var face = faces[(int)me.Offset];

        Debug.WriteLine($"Final answer 1 {row} {col} {face}: {(1000 * row + 4 * col + face)}");

    }

    private static void TransformEdges(MonkeyMap map)
    {
        PrintOut(map, true);
        foreach (var (key, (horizontal, vertical)) in map.EdgeConnections)
        {

        }
    }


    private static void FirstPart()
    {
        var map = MonkeyMap.Load(GetDataStream);

        Walker me = new Walker(map.StartPos, Offset.E, map);
        foreach (var instr in map.Instructions)
        {
            me.Execute(instr);
        }

        Debug.WriteLine($"Walker at = {me.Pos}");
        //PrintOut(map);
        //   me.Track.ForEach(
        //       x => Debug.WriteLine(x));
        var row = me.Pos.Y * -1;
        var col = me.Pos.X + 1;
        var faces = new[] { 3, 0, 1, 2, };
        var face = faces[(int)me.Offset];

        Debug.WriteLine($"Final answer 1 {row} {col} {face}: {(1000 * row + 4 * col + face)}");
    }

    private static void PrintOut(MonkeyMap map, bool small = false)
    {
        void IndexLine(long from, long to, int i)
        {
            var sb = new StringBuilder("     ");
            for (long x = from; x <= to; x += i)
                sb.Append($"{Math.Abs(x) % 10}");
            Debug.WriteLine(sb.ToString());
        }

        var (minY, maxY, minX, maxX) = MonkeyMap.MinMaxMap(map);
        Debug.WriteLine($"Printout  ({minX},{minY}) -> ({maxX},{maxY})  StartPos: {map.StartPos}");
        var incr = small ? 5 : 1;
        for (long y = maxY; y >= minY; y -= incr)
        {
            if ((y % map.SideWidth) == 0)
                IndexLine(minX, maxX, incr);
            var sb = new StringBuilder($"{y,4} ");

            for (long x = minX; x <= maxX; x += incr)
            {
                var position =  new GlobalPosition(x, y);
                if (!map.IsEmpty(position))
                {
                    var tile = map.Value(position);
                    if (small && tile.Typ != Tile.Types.Edge)
                    {

                        var symbol = tile.Side.Name;

                        sb.Append(symbol);
                    }
                    else
                    {
                        sb.Append(tile!.Symbol);
                    }
                }
                else
                {
                    sb.Append(" ");
                }

                if (small) sb.Append(" ");
            }
            Debug.WriteLine(sb.ToString());
        }
    }
}