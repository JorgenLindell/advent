using System.Diagnostics;
using System.Net.Security;
using System.Text;
using _22;
using common;
using common.SparseMatrix;
using Direction = common.SparseMatrix.Direction;


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
        Side.ResetSeq();
        MonkeyMap.UseCubeCoordinates = true; //flag for part 1 or part 2
        var map = MonkeyMap.Load(GetDataStream);
        // calculate sides
        // Basic assumption:
        // if two sides of the map are connected in a 90 degrees angle via a third, the they go together.
        //   ie if you miss a connection south, then turn sideways, go next page, turn the other way, go nex an repeat that turn
        //   if you now is on a sid missing connection north, than that is it, connect them.
        // repeat that process to use the just connected ones
        map.Sides.Values.ForEach((s, _) => s.MakeDirectConnections());

        while (map.Sides.Values.Sum(s => s.CheckMissing()) > 0)
        { }

        Debug.WriteLine("");

        // diagnostic: All connected sides
        map.Sides.Values.ForEach((s, _) => s.PrintSides());

        PrintOut(map);

        // walk the walk
        Walker walker = new Walker(map.StartPos!, Direction.E, map);
        foreach (var instr in map.Instructions)
        {
            walker.Execute(instr);
        }


        PrintOut(map, walker);

        //calc result
        PrintResult(walker, 2);
    }

    private static void PrintResult(Walker walker, int task)
    {
        var walkerPos = walker.Pos;
        Debug.WriteLine($"Walker at = {walkerPos}");
        var row = (walkerPos.Y * -1) + 1;
        var col = walkerPos.X + 1;
        var faces = new[] { 3, 0, 1, 2, }; // translate facing
        var face = faces[(int)walker.Direction];
        Debug.WriteLine($"Final answer {task} {row} {col} {face}: {(1000 * row + 4 * col + face)}");
    }


    private static void FirstPart()
    {
        var map = MonkeyMap.Load(GetDataStream);
        PrintOut(map);



        Walker me = new Walker(map.StartPos!, Direction.E, map);
        foreach (var instr in map.Instructions)
        {
            me.Execute(instr);
        }


        PrintResult(me, 1);
    }

    private static void PrintOut(MonkeyMap map, Walker? walker = null)
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
        var incr =  1;
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
                    if (tile.Typ == Tile.Types.Edge && MonkeyMap.UseCubeCoordinates)
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
                                var c = $"{conn.Side.Name}{going.Turn(conn.Turn).Invert()}".PadRight(
                                    reserveForConnection);
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
                                sb.Append($"{conn.Side.Name}{direction.Turn(conn.Turn).Invert()}");
                            }
                        }
                        else
                        {
                            // both is null, this is a horizontal edge
                            var edgeLength = map.SideWidth;
                            var northSide = map.GlobalToSide(position.PosNorth);
                            var southSide = map.GlobalToSide(position.PosSouth);
                            prevSide = map.GlobalToSide(position - (edgeLength, 0));
                            var egdeChar = tile.Symbol[0];

                            string AddedLabel(Side side, Direction going1)
                            {
                                var addedLabel = "";
                                if (side.Connections.ContainsKey(going1))
                                {
                                    var conn1 = side.Connections[going1];
                                    addedLabel =
                                        $"{conn1.Side.Name}{going1.Turn(conn1.Turn).Invert()}";
                                }
                                else
                                    addedLabel = $"NA";

                                var padLeft = (edgeLength + addedLabel.Length) / 2;
                                addedLabel = addedLabel.PadLeft(padLeft, egdeChar);
                                addedLabel = addedLabel.PadRight(edgeLength, egdeChar);
                                return addedLabel;
                            }

                            if (northSide.Side != null)
                            {
                                var addedLabel = AddedLabel(northSide.Side, Direction.S);
                                sb.Append(addedLabel);
                                x += addedLabel.Length - 1;
                            }
                            else if (southSide.Side != null)
                            {
                                if (prevSide.Side != null)
                                    sb.Remove(sb.Length - 2, 2);
                                var addedLabel = AddedLabel(southSide.Side, Direction.N);
                                sb.Append(addedLabel);
                                x += addedLabel.Length - 1;
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
                else
                {
                    sb.Append(" ");
                }
            }

            sb.Append("\n");
        }
        Debug.WriteLine(sb.ToString());
    }
}