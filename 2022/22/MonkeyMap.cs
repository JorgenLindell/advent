using System.Diagnostics;
using common;
using common.SparseMatrix;

internal class MonkeyMap : SparseMatrix<GlobalPosition, Tile, Position<GlobalPosition>>
{
    public GlobalPosition StartPos { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SideWidth { get; set; } = 50;


    public Dictionary<string, (int Face, int ColumnMove, int Rotate)> SidePositions = new();

    public Dictionary<int, Dictionary<Offset, int>> StdCube = new()
    {
        { 4, new Dictionary<Offset, int> {
                { Offset.N, 5 },
                { Offset.E, 1 },
                { Offset.S, 2 },
                { Offset.W, 6 }
            } },
        { 3, new Dictionary<Offset, int>
            {
                { Offset.N, 5 },
                { Offset.E, 6 },
                { Offset.S, 2 },
                { Offset.W, 1 }
            } },

        { 1, new Dictionary<Offset, int> {
                { Offset.N, 5 },
                { Offset.E, 3 },
                { Offset.S, 2 },
                { Offset.W, 4 }
            } },
        {2, new Dictionary<Offset, int> {
                { Offset.N, 1 },
                { Offset.E, 3 },
                { Offset.S, 6 },
                { Offset.W, 4 }
            } },
        {6, new Dictionary<Offset, int>
            {
                { Offset.N, 2 },
                { Offset.E, 3 },
                { Offset.S, 5 },
                { Offset.W, 4 }
            } },
        { 5, new Dictionary<Offset, int>
            {
                { Offset.N, 6 },
                { Offset.E, 3 },
                { Offset.S, 1 },
                { Offset.W, 4 }
            } }
    };

    private int[][] StdSidePositions =
    {
        new[] { 4, 1, 3 },
        new[] { 0, 2, 0 },
        new[] { 0, 6, 0 },
        new[] { 0, 5, 0 }
    };

    public List<Instruction> Instructions { get; } = new();
    public Dictionary<GlobalPosition, (GlobalPosition Horizontal, GlobalPosition Vertical)> EdgeConnections { get; } = new();

    public static MonkeyMap Load(Func<TextReader> getDataStream)
    {
        var stream = getDataStream();
        var map = Tile.Map = new MonkeyMap();
        var startingLineIndex = 0;

        AnalyseStreamForSizes(stream, map);


        stream = getDataStream();
        int startOfCont;
        int endOfCont;

        var lineIx = startingLineIndex;
        lineIx++;
        while (stream.ReadLine() is { } inpLine)
        {
            if (inpLine == "") break;

            lineIx--;

            // load content line
            LoadLine(map, inpLine, lineIx);
        }

        // add horizontal edges
        AddHorizEdges(map, startingLineIndex);


        // calculate sides
        map.CalculateSides();


        Debug.WriteLine($"Read lines={-lineIx - 1}");
        // PrintOut(map);

        //Read walking instructions
        LoadInstructions(stream, map);

        return map;


        //Local 
        void LoadLine(MonkeyMap map, string inpLine, int currLineIx)
        {
            (startOfCont, endOfCont) = FindContent(inpLine);

            if (currLineIx == startingLineIndex)
                // first loop
                map.StartPos = new GlobalPosition(startOfCont, currLineIx);

            var lastChar = ' ';
            Enumerable.Range(startOfCont, endOfCont - startOfCont + 1).ForEach(
                (p, _) =>
                {
                    GlobalPosition pos;
                    if (inpLine[p] != '.')
                    {
                        pos = new GlobalPosition(p, currLineIx);
                        map.Value(pos, new Tile(Tile.EWall, pos));
                        return;
                    }

                    if ((inpLine[p] == ' ') & (lastChar != ' '))
                    {
                        pos = new GlobalPosition(p, currLineIx);
                        map.Value(pos, new Tile(Tile.EEdge, pos));
                    }

                    if ((inpLine[p] != ' ') & (lastChar == ' '))
                    {
                        pos = new GlobalPosition(p, currLineIx);
                        map.Value(pos.PosWest, new Tile(Tile.EEdge, pos.PosWest));
                    }

                    lastChar = inpLine[p];

                    pos = new GlobalPosition(p, currLineIx);
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
            startOfCont = (int)map.Values.Min(x => x.Pos.X);
            endOfCont = (int)map.Values.Max(x => x.Pos.X);
            var last = (int)map.Values.Min(x => x.Pos.Y);
            for (var x = startOfCont + 1; x < endOfCont; x++)
            {
                //for each column, search downward for content
                var startPos1 = new GlobalPosition(x, startingLineIndex1 + 1);
                while (map.IsEmpty(startPos1.PosSouth)
                       || map.Value(startPos1.PosSouth)!.Typ == Tile.Types.Edge)
                    startPos1 = startPos1.PosSouth;

                //for each column, search upward for content
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
            var instructionLine = textReader.ReadLine();
            var str = "";
            foreach (var c in "" + instructionLine)
                if (c.In('R', 'L'))
                {
                    monkeyMap.Instructions.Add(new Instruction(str.ToInt()!.Value, c));
                    str = "";
                }
                else
                {
                    str += c;
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
            map1.SideWidth = sideWidth;
            map1.Width = mapWidth;
            map1.Height = count;
        }
    }

    private void CalculateSides()
    {
        var tempPos = StartPos;
        var side = Side(tempPos);
        var loadedSidePos = side.SidePos;
        SidePosition stdSidePos = FacePosition(1);
        int columnMove = 0;
        int rotate = 0;
        if (loadedSidePos != stdSidePos)
        {
            var diff = (stdSidePos - loadedSidePos) ;
            columnMove = (int)diff.Y;
            rotate = (int) diff.X;
        }
        SidePositions[side.Name] = (Face: 1, ColumnMove: columnMove, Rotate: rotate);

    }

    private SidePosition FacePosition(int face)
    {
        for (var r = 0; r < 4; r++)
        {
            for (var c = 0; c < 5; c++)
            {
                if (StdSidePositions[r][c] == face)
                {
                    return new SidePosition(c,r);
                }
            }
        }

        throw new KeyNotFoundException("No face called " + face);
    }

    public (string Name, SidePosition SidePos, GlobalPosition StartSide, LocalPosition Pos) Side(GlobalPosition pos)
    {
        var mapSidesWidth = Width / SideWidth;
        var sidey = (-pos.Y - 1) / SideWidth;
        var sidex = pos.X / SideWidth;
        var name = "" + (char)('a' + sidey * mapSidesWidth + sidex);
        var sidePos = new SidePosition(sidex, sidey);
        var (startSide, localPos) = PosInSide(pos);
        return (Name: name, SidePos: sidePos, StartSide: startSide, Pos: localPos);
    }

    public GlobalPosition GlobalPos(string side, LocalPosition pos)
    {
        var mapSidesWidth = Width / SideWidth;
        var nameNum = side[0] - 'a';
        var sideY = nameNum / mapSidesWidth;
        var sideX = nameNum * mapSidesWidth;
        var posY = -(sideY * SideWidth + 1);
        var posX = -(sideY * SideWidth + 1);
        return new GlobalPosition(posX, posY);
    }


    public (GlobalPosition StartSide, LocalPosition Pos) PosInSide(GlobalPosition pos)
    {
        var adjY = pos.Y;
        var sideWidth = SideWidth;
        var startY = adjY / sideWidth * sideWidth - 1;
        var startX = pos.X / sideWidth * sideWidth;

        var posInSide = (StartSide: new GlobalPosition(startX, startY),
            Pos: new LocalPosition(pos.X % sideWidth, adjY % sideWidth + 1));
        return posInSide;
    }


    public LocalPosition FlipCoordinates(Offset fromDir, Offset toDir, LocalPosition pos)
    {
        return FlipCoordinates(fromDir, toDir, pos, out _);
    }

    public LocalPosition FlipCoordinates(Offset fromDir, Offset toDir, LocalPosition pos, out int rot)
    {
        rot = toDir - fromDir;
        var newPos2 = RotSidePos(pos, rot);

        return newPos2;
    }

    public LocalPosition RotSidePos(LocalPosition pos, int r)
    {
        LocalPosition Recurse(LocalPosition position, int i1)
        {
            var resPos1 = position;
            for (var i = 0; i < Math.Abs(i1); i++) resPos1 = RotSidePos(resPos1, Math.Sign(i1));
            return resPos1;
        }

        var sw = SideWidth;
        var (x, y) = pos;
        var resPos = r switch
        {
            0 => new LocalPosition(pos),
            -1 => new LocalPosition(y, -sw - x - 1),
            1 => new LocalPosition(-sw - y - 1, x),
            _ => Recurse(pos, r)
        };

        return resPos;
    }

    public void AddEdge(GlobalPosition pos, GlobalPosition endPos)
    {
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
}