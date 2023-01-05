using System.Diagnostics;
using System.Text;
using _22;
using common;
using common.SparseMatrix;


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
    static bool _debug = false;

    private static void Main(string[] args)
    {
        //  FirstPart();
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
        Side.ResetSeq();
        var map = MonkeyMap.Load(GetDataStream);
        // calculate sides
        map.Sides.Values.ForEach((s, _) => s.MakeDirectConnections());
        map.Sides.Values.Reverse().ForEach((s, _) => s.CheckMissing());
        map.Sides.Values.ForEach((s, _) => s.CheckMissing());
        Debug.WriteLine("");

        map.Sides.Values.ForEach((s, _) => s.PrintSides());

        MonkeyMap.UseCubeCoordinates = true;
        PrintOut(map, small: false);

        Walker me = new Walker(map.StartPos!, Direction.E, map);
        foreach (var instr in map.Instructions)
        {
            me.Execute(instr);
        }

        Debug.WriteLine($"Walker at = {me.Pos}");
        PrintOut(map, true,false,  me);
        var row = (me.Pos.Y * -1)+1 ;
        var col = me.Pos.X + 1;
        var faces = new[] { 3, 0, 1, 2, };
        var face = faces[(int)me.Direction];

        Debug.WriteLine($"Final answer 2 {row} {col} {face}: {(1000 * row + 4 * col + face)}");

    }


    private static void FirstPart()
    {
        var map = MonkeyMap.Load(GetDataStream);
        PrintOut(map, false);



        Walker me = new Walker(map.StartPos, Direction.E, map);
        foreach (var instr in map.Instructions)
        {
            me.Execute(instr);
        }


        Debug.WriteLine($"Walker at = {me.Pos}");
        //   me.Track.ForEach(
        //       x => Debug.WriteLine(x));
        var row = (me.Pos.Y * -1) ;
        var col = me.Pos.X + 1;
        var faces = new[] { 3, 0, 1, 2, };
        var face = faces[(int)me.Direction];


        Debug.WriteLine($"Final answer 1 {row} {col} {face}: {(1000 * row + 4 * col + face)}");
    }

    private static void PrintOut(MonkeyMap map, Boolean force = false, bool small = false, Walker? walker = null)
    {
        ILookup<GlobalPosition, Direction>? track = null;
        if (walker != null)
        {
            track = walker.Track.ToLookup(x => x.Item1, x => x.Item2);
        }
        int reserveForConnection = 2;
        int reserveForLine = 6;
        var reserveSpace = reserveForLine + reserveForConnection;
        void IndexLine(StringBuilder sb, long from, long to, int i)
        {
            sb.Append("".PadRight(reserveSpace));
            for (long x = from; x <= to; x += i)
                sb.Append($"{Math.Abs(x) % 10}");
            sb.Append("\n");
        }

        var (minY, maxY, minX, maxX) = MonkeyMap.MinMaxMap(map);
        Debug.WriteLine($"Printout  ({minX},{minY}) -> ({maxX},{maxY})  StartPos: {map.StartPos}");
        var incr = small ? 5 : 1;
        var sb = new StringBuilder();
        for (long y = maxY; y >= minY; y -= incr)
        {
            if ((y % map.SideWidth) == 0)
                IndexLine(sb, minX, maxX, incr);
            sb.Append($"{y,4} ".PadRight(reserveSpace));
            for (long x = minX; x <= maxX; x += incr)
            {
                var position = new GlobalPosition(x, y);
                if (!map.IsEmpty(position))
                {
                    var tile = map.Value(position);
                    if (small && tile.Typ != Tile.Types.Edge)
                    {
                        var symbol = tile.Side?.Name;

                        sb.Append(symbol);
                    }
                    else
                    {
                        if (tile.Typ == Tile.Types.Edge)
                        {
                            var increment = new GlobalPosition(incr, 0);
                            var prevSide = map.GlobalToSide(position - increment);
                            var nextSide = map.GlobalToSide(position + increment);
                            if (nextSide.Side != null)
                            {
                                if (prevSide.Side == null)
                                {
                                    var direction = increment!.ToDirection()!.Value;
                                    var going = direction.Invert();
                                    var conn = nextSide.Side.Connections[going];
                                    sb.Remove(sb.Length - reserveForConnection, reserveForConnection);
                                    var c = $"{conn.Side.Name}{going.Turn(conn.Turn)}".PadRight(reserveForConnection);
                                    sb.Append(c);
                                }
                                sb.Append(tile.Symbol);
                            }
                            else if (prevSide.Side != null)
                            {
                                sb.Append(tile.Symbol);
                                if (nextSide.Side == null)
                                {
                                    var direction = increment!.ToDirection()!.Value;
                                    var conn = prevSide.Side.Connections[direction];
                                    sb.Append($"{conn.Side.Name}{direction.Turn(conn.Turn)}");
                                }
                            }
                            else
                            {
                                // both is null, this is a horizontal edge
                                var edgeLength = map.SideWidth;
                                var northSide = map.GlobalToSide(position.PosNorth);
                                var southSide = map.GlobalToSide(position.PosSouth);
                                var egdeChar = tile.Symbol[0];

                                string AddedLabel(Side side, Direction going1)
                                {
                                    var addedLabel = "";
                                    if (side.Connections.ContainsKey(going1))
                                    {
                                        var conn1 = side.Connections[going1];
                                        addedLabel =
                                            $"e{conn1.Side.Name}{going1.Turn(conn1.Turn).Invert()}e";
                                    }
                                    else
                                        addedLabel = $"NA";
                                    addedLabel = addedLabel.PadLeft(edgeLength / 2, egdeChar);
                                    addedLabel = addedLabel.PadRight(edgeLength, egdeChar);
                                    return addedLabel;
                                }

                                if (northSide.Side != null)
                                {
                                    sb.Append(AddedLabel(northSide.Side, Direction.S));
                                    x += edgeLength;
                                }
                                else if (southSide.Side != null)
                                {
                                    sb.Append(AddedLabel(southSide.Side, Direction.N));
                                    x += edgeLength - 1;
                                }
                                else
                                    sb.Append(tile.Symbol);
                            }
                        }
                        else
                        {
                            if (track?.Contains(position) ?? false)
                            {
                                var p = track[position].Last();
                                sb.Append("^>v<"[(int)p]);
                            }
                            else
                                sb.Append(tile!.Symbol == "#" ? tile.Side?.Name ?? tile!.Symbol : tile!.Symbol);
                        }
                    }
                }
                else
                {
                    sb.Append(" ");
                }

                if (small) sb.Append(" ");
            }

            sb.Append("\n");
        }
        Debug.WriteLine(sb.ToString());
    }
}