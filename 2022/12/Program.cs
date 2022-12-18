using System.Diagnostics;
using System.Text;
using common;
using Mapper;
using Microsoft.VisualBasic;


//https://adventofcode.com/2022/day/12
internal class Program
{
    private static string _testData =
        @"Sabqponm
abcryxxl
accszExk
acctuvwj
abdefghi"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }

    private static void SecondPart(TextReader stream)
    {
    }


    private static void FirstPart(TextReader stream)
    {
        char[,] cells = null!;

        var lineIndex = 0;
        (int r, int c) start = (0, 0);
        (int r, int c) end = (0, 0);
        while (stream.ReadLine() is { } inpLine)
        {
            if (cells == null!)
            {
                var size = inpLine.Length;
                cells = new char[size, size];
            }

            int i = 0;
            foreach (var t in inpLine.ToCharArray())
            {
                cells[lineIndex, i] = t;
                if (t == 'S')
                {
                    start = (lineIndex, i);
                    cells[lineIndex, i] = 'a';
                }

                if (t == 'E')
                {
                    end = (lineIndex, i);
                    cells[lineIndex, i] = 'z';
                }

                ++i;
            }
            ++lineIndex;
        }

        var map = Map.ConstructMap(cells, start, end);
        var se = new SearchEngine(map);
        se.Custom();

        var n = map.StartNode;
        var path = new HashSet<Node>();
        while (n!=map.EndNode)
        {
            path.Add(n);
            n = n.CloserToEnd;
        }
        var maxR = cells.GetUpperBound(0);
        var maxC = cells.GetUpperBound(1);
        for (int r = 0; r < maxR + 1; r++)
        {
            for (int c = 0; c < maxC + 1; c++)
            {
                var name = (new Point(c, r)).ToString();
                if (map.NodesLookup.ContainsKey(name))
                {
                    var node = map.NodesLookup[name];
                    if (cells[r, c] == 'a' && node.Visited)
                    {
                        Debug.WriteLine(node.MinCostToEnd);
                    }
                    if (path.Contains(node))
                        Console.Write(("" + cells[r, c]).ToUpper()+ "");
                    else
                        Console.Write(cells[r, c]);
                }
                else
                    Console.Write(cells[r, c]);
            }
            Console.WriteLine();
        }
        Debug.WriteLine(se.Start.MinCostToEnd);
    }
}



