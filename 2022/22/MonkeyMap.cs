using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using common;
using common.SparseMatrix;

namespace _22;

internal class MonkeyMap : SparseMatrix<GlobalPosition, Tile, Position<GlobalPosition>>
{
    public GlobalPosition? StartPos { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SideWidth { get; set; } = 50;
    public bool Trace = true;

    public SparseMatrix<SidePosition, Side, Position<SidePosition>> Sides { get; } = new();
    public List<Instruction> Instructions { get; } = new();
    public Dictionary<GlobalPosition, (GlobalPosition Horizontal, GlobalPosition Vertical)> EdgeConnections { get; } = new();
    public static bool UseCubeCoordinates { get; set; }

    public static MonkeyMap Load(Func<TextReader> getDataStream)
    {
        var stream = getDataStream();
        var map = Tile.Map = Side.Map = new MonkeyMap();
        var startingLineIndex = 0;

        AnalyseStreamForSizes(stream, map);


        stream = getDataStream();
        int startOfCont;
        int endOfCont;

        var lineIx = startingLineIndex;
        lineIx++;
        var countOfLines = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            if (inpLine == "") break;
            countOfLines++;

            lineIx--;

            // load content line
            LoadLine(map, inpLine, lineIx);
        }
        Debug.WriteLine($"Read lines={countOfLines}");

        // add horizontal edges
        AddHorizEdges(map, startingLineIndex);



        // PrintOut(map);

        //Read walking instructions
        LoadInstructions(stream, map);

        return map;


        //Local 
        void LoadLine(MonkeyMap map, string inpLine, int currLineIx)
        {
            // map is represented by a "SparseMatrix" with values for different cell
            // types Edge, Free and  Wall, outside map is empty

            (startOfCont, endOfCont) = FindContent(inpLine);

            if (currLineIx == startingLineIndex)
                // first loop
                map.StartPos = new GlobalPosition(startOfCont, currLineIx);

            var lastChar = ' ';

            Enumerable.Range(startOfCont, endOfCont - startOfCont + 1).ForEach(
                (p, _) =>
                {
                    var pos = new GlobalPosition(p, currLineIx);
                    var inPageData = map.GlobalToSide(pos);
                    if (map.Sides.IsEmpty(inPageData.SidePosition))
                    {
                        map.Sides.Value(inPageData.SidePosition, new Side(inPageData.SidePosition, inPageData.SideStart));
                    }
                    if (inpLine[p] != '.')
                    {
                        map.Value(pos, new Tile(Tile.EWall, pos));
                        return;
                    }

                    lastChar = inpLine[p];

                    map.Value(pos, new Tile(Tile.EFree, pos));
                });
            // set edge markers at start and end
            var startPos = new GlobalPosition(startOfCont - 1, currLineIx);
            while (map.IsEmpty(startPos.PosEast))
                startPos = startPos.PosEast;

            var endPos = new GlobalPosition(endOfCont + 1, currLineIx);
            while (map.IsEmpty(endPos.PosWest))
                endPos = endPos.PosWest;

            map.Value(startPos, new Tile(Tile.EEdge, startPos, endPos));
            map.Value(endPos, new Tile(Tile.EEdge, endPos, startPos));

            return;

            // local
            (int startOfCont, int endOfCont) FindContent(string s)
            {
                // find filled in part
                startOfCont = 0;
                endOfCont = s.Length;

                //start pos skip leading blanks
                var i = 0;
                for (; i < s.Length && s[i] == ' '; i++)
                {
                }

                if (i < s.Length) startOfCont = i;

                //end pos, skip trailing blanks
                i = s.Length - 1;
                for (; i > 0 && s[i] == ' '; i--)
                {
                }

                endOfCont = i;
                return (startOfCont, endOfCont);
            }
        }

        void AddHorizEdges(MonkeyMap map, int startingLineIndex1)
        {
            // vertical edges have been set for each line, now add horizontal

            startOfCont = (int)map.Values.Min(x => x.Pos.X);
            endOfCont = (int)map.Values.Max(x => x.Pos.X);
            var last = (int)map.Values.Min(x => x.Pos.Y);
            for (var x = startOfCont + 1; x < endOfCont; x++)
            {
                //for each column, search downward for actual content
                var startPos1 = new GlobalPosition(x, startingLineIndex1 + 1);
                while (map.IsEmpty(startPos1.PosSouth)
                       || map.Value(startPos1.PosSouth)!.Typ == Tile.Types.Edge)
                    startPos1 = startPos1.PosSouth;

                //for each column, search upward for actual content
                var endPos1 = new GlobalPosition(x, last - 1);
                while (map.IsEmpty(endPos1.PosNorth)
                       || map.Value(endPos1.PosNorth)!.Typ == Tile.Types.Edge)
                    endPos1 = endPos1.PosNorth;

                // set edge tiles
                map.Value(startPos1, new Tile(Tile.EEdge, startPos1, endPos1));
                map.Value(endPos1, new Tile(Tile.EEdge, endPos1, startPos1));
            }
        }

        void LoadInstructions(TextReader textReader, MonkeyMap monkeyMap)
        {

            var instructionLine = textReader.ReadLine() + "S"; // add an end marker to force last move to be evaluated.
            var str = "";
            foreach (var c in "" + instructionLine)
            {
                var i = str.ToInt().HasValue ? str.ToInt()!.Value : 0;
                if (c.In('R', 'L'))
                {
                    // create separate instructions for turns and moves as we aren't sure turns can't be sequential without move
                    if (i > 0)
                        monkeyMap.Instructions.Add(new Instruction(i, 'N')); // N designates a 'no turn'
                    monkeyMap.Instructions.Add(new Instruction(0, c)); //just the turn
                    str = "";
                }
                else if (c == 'S')
                {
                    monkeyMap.Instructions.Add(new Instruction(i, 'S')); //just the very last move if no trailing turn.
                    str = "";
                }
                else
                {
                    str += c; // accumulate string as long as not L,R or S
                }
            }
        }

        void AnalyseStreamForSizes(TextReader stream1, MonkeyMap map1)
        {
            var mapWidth = 0;
            var count = 0;
            var sideWidth = 100000;
            while (stream1.ReadLine() is { } inpLine)
            {
                if (inpLine.Length == 0) break;
                mapWidth = Math.Max(mapWidth, inpLine.Length);
                sideWidth = Math.Min(sideWidth, inpLine.Replace(" ", "").Length);
                count++;
            }

            var verify = count % sideWidth == mapWidth % sideWidth;
            if (verify == false || count % sideWidth != 0 || mapWidth % sideWidth != 0)
                throw new Exception("Can't figure out sidewidth");
            map1.SideWidth = sideWidth;
            map1.Width = mapWidth;
            map1.Height = count;
        }
    }

    public (Side? Side, GlobalPosition SideStart, SidePosition SidePosition, LocalPosition LocalPosition, long SideId) 
        GlobalToSide(GlobalPosition pos)
    {
        var sideWidth = SideWidth;
        var (sidePosition, sideStart) = CalcSidePos(pos, sideWidth);
        var side = Sides.Value(sidePosition);
        var number = sidePosition.Y * sideWidth + sidePosition.X;

        var posInSide = (
            Side: side,
            SideStart: sideStart,
            SidePosition: sidePosition,
            LocalPosition: new LocalPosition(pos - sideStart),
            SideId: number);
        return posInSide;
    }

    public static (SidePosition sidePosition, GlobalPosition sideStart) CalcSidePos(GlobalPosition pos, int sideWidth)
    {
        var sidePosition = new SidePosition(pos.X / sideWidth, pos.Y / sideWidth);
        // adjust for x < 0 and y>0. Having a y that goes 0 and downwards was not the best decision..
        if (pos.Y > 0)
        {
            sidePosition.Y++;
        }
        if (pos.X < 0)
        {
            sidePosition.X--;
        }
        var sideStart = new GlobalPosition(sidePosition * sideWidth);
        return (sidePosition, sideStart);
    }

    public GlobalPosition SideToGlobal(Side side, LocalPosition localPosition)
        => new(side.StartSide + new GlobalPosition(localPosition));

    public void AddEdge(GlobalPosition pos, GlobalPosition endPos)
    {
        //Edge connections are used for part 1 the wrap-around map.
        if (!EdgeConnections.ContainsKey(pos))
            EdgeConnections[pos] = default;
        var connect = EdgeConnections[pos];
        if (pos.X != endPos.X)
            connect.Horizontal = endPos;
        else if (pos.Y != endPos.Y)
            connect.Vertical = endPos;
        else
            throw new InvalidDataException("Inconsistent edge");
        EdgeConnections[pos] = connect;
    }

    public static (long minY, long maxY, long minX, long maxX) MinMaxMap(MonkeyMap map)
    {
        var first = map.Values.First().Pos;
        var minY = first.Y;
        var maxY = first.Y;
        var minX = first.X;
        var maxX = first.X;
        foreach (var tile in map.Values)
        {
            minY = Math.Min(minY, tile.Pos.Y);
            maxY = Math.Max(maxY, tile.Pos.Y);
            minX = Math.Min(minX, tile.Pos.X);
            maxX = Math.Max(maxX, tile.Pos.X);
        }

        return (minY, maxY, minX, maxX);
    }

    public void Log(string msg)
    {
        if (Trace)
            Debug.WriteLine(msg);
    }
}