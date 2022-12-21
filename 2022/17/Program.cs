using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using common;
using common.SparseMatrix;


//https://adventofcode.com/2022/day/17

public class Rock
{
    public string Name { get; }
    public int Order { get; }
    public static int WallL;
    public static int WallR;
    public Rock(int order, string name, List<string> definition)
    {
        Name = name;
        Order = order;
        var lines = definition.Count;
        var topLine = lines;
        var width = 0;
        var occupied = new List<(int x, int y)>();
        foreach (var line in definition)
        {
            topLine--;
            var line1 = topLine;
            line.ForEach((c, i) =>
            {
                if (c == '#')
                {
                    occupied.Add((x: i, y: line1));
                    if (i > width) width = i;
                }
            });
            Width = width + 1;
            Height = lines;
            Cells = occupied;
        }
    }

    public List<(int x, int y)> Cells { get; }
    public long X { get; set; }
    public long Y { get; set; }
    public int Width { get; }
    public int Height { get; }

    public bool Check(SparseMatrix<char> map, (int x, int y) offset)
    {
        if (Y + offset.y < 0)
            return false;

        foreach ((int x, int y) pos in Cells)
        {
            var testX = X + offset.x + pos.x;
            var testY = Y + offset.y + pos.y;
            if (testX <= WallL || testX >= WallR)
                return false;
            if (!map.IsEmpty(testX, testY))
            {
                return false;
            }
        }

        return true;
    }

    public void Move((int x, int y) offset)
    {
        X += offset.x;
        Y += offset.y;
    }

    public void Store(SparseMatrix<char> map)
    {

        foreach ((int x, int y) pos in Cells)
        {
            var testX = X + pos.x;
            var testY = Y + pos.y;
            map.Value(testX, testY, 'x');
        }
    }
}

internal class Program
{
    private static string stones =
        @"####

.#.
###
.#.

..#
..#
###

#
#
#
#

##
##"
            .Replace("\r\n", "\n");

    private static string _testData =
        @">>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>"
            .Replace("\r\n", "\n");

    private static List<Rock> _rocks;

    private static void Main(string[] args)
    {
        IEnumerable<Rock> GetRocks()
        {
            var rockLines = (stones + '\n').Split('\n');
            var rockNames = new[] { "-", "+", "⅃", "|", "◾" };
            var currentRock = new List<string>();
            var order = 0;
            foreach (var rockLine in rockLines)
            {
                if (rockLine.Trim() == "" && currentRock.Count > 0)
                {
                    var rock = new Rock(order, rockNames[order], currentRock);
                    order++;
                    currentRock = new List<string>();
                    yield return rock;
                }
                else if (rockLine.Trim() != "")
                {
                    currentRock.Add(rockLine);
                }
            }
        }

        _rocks = GetRocks().ToList();

        var debug = false;
        FirstPart(GetDataStream(debug));
        SecondPart(GetDataStream(debug));
    }

    private static TextReader GetDataStream(bool debug) =>
        debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");


    private static void SecondPart(TextReader stream)
    {

    }


    private static void FirstPart(TextReader stream)
    {
        var map = new SparseMatrix<char>();
        var jetstream = stream.ReadToEnd().Replace("\n", "");
        var jetpos = 0;
        Rock.WallL = 0;
        Rock.WallR = 8;
        for (int i = Rock.WallL; i <= Rock.WallR; i++)
        {
            map.Value(i, -1, 'x');
        }
        var targetCountFallen = 1000000000000;

        var topOfRocks = DropRocks(jetstream, map, targetCountFallen);

        Debug.WriteLine("Height=" + (topOfRocks + 1));

    }

    private static long DropRocks(string jetStream, SparseMatrix<char> map, long targetCountFallen)
    {
        var fallingRock = _rocks[0];
        fallingRock.X = 3;
        fallingRock.Y = 3;

        var topOfRocks = 0L;
        var rockResults = new Dictionary<(int rockType, int jetIndex, string topo), (long sten, long height)>();
        var addedRocks = 0L;
        var addedHeight = 0L;
        int jetPos = 0;
        long numberOfRock = 1;
        while (true)
        {
            var wind = jetStream[jetPos];
            if (!wind.In("<>"))
            {
                throw new InvalidDataException("Wrong wind");
            }

            var sideMove = (x: 0, y: 0);

            if (wind == '>')
                sideMove.x++;
            else if (wind == '<')
                sideMove.x--;
            else
                throw new InvalidDataException("Wrong wind");

            var wouldMove = fallingRock.Check(map, sideMove);
            if (wouldMove)
            {
                fallingRock.Move(sideMove);
            }

            var wouldFall = fallingRock.Check(map, (x: 0, y: -1));
            if (wouldFall)
            {
                fallingRock.Move((x: 0, y: -1));
            }
            else
            {
                fallingRock.Store(map);
                topOfRocks = Math.Max(fallingRock.Y + fallingRock.Height - 1, topOfRocks);

                var profile = "";
                for (int i = 1; i < Rock.WallR; i++)
                {
                    int j = 0;
                    for (; map.Value(i, topOfRocks - j) != 'x'; j++) { }
                    profile += (char)('a' + j);
                }

                var resultKey = (rockType: fallingRock.Order, jetIndex: jetPos, topo: profile);
                if (!rockResults.ContainsKey(resultKey))
                    rockResults[resultKey] = (sten: numberOfRock, height: topOfRocks);
                else
                {
                    var last = rockResults[resultKey];
                    var diffCount = numberOfRock - last.sten;
                    var diffHeight = topOfRocks - last.height;
                    var missingRocks = targetCountFallen - (addedRocks + numberOfRock);
                    var chunks = missingRocks / diffCount;
                    addedRocks += chunks * diffCount;
                    addedHeight += chunks * diffHeight;
                    rockResults.Clear();
                }

                if ((numberOfRock + addedRocks) >= targetCountFallen)
                    break;

                //new rock
                numberOfRock++;
                fallingRock = _rocks[(fallingRock.Order + 1) % _rocks.Count];
                fallingRock.X = 3;
                fallingRock.Y = topOfRocks + 4;
            }

            jetPos = (jetPos + 1) % jetStream.Length;
        }

        return topOfRocks + addedHeight;
    }
}
