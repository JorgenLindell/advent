using common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace _15
{
    static class StringPairsExtension
    {
        public static IEnumerable<string> Pairs(this string full)
        {
            for (int i = 1; i < full.Length; i++)
            {
                yield return full.Substring(i - 1, 2);
            }
        }
    }

    class Program
    {
        private static string _testData1 = @"
1163751742
1381373672
2136511328
3694931569
7463417111
1319128137
1359912421
3125421639
1293138521
2311944581
";

        private static string template;
        static void Main()
        {
            int Calc(int r, int c, int i)
            {
                int calc = ((i-1 + r + c) % 9)+1;
                return calc;
            }



            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //var stream = StreamUtils.GetInputStream(testData: _testData1);
            var inputLines = LoadStream(stream).ToList();
            var columnsCount = inputLines.Max(x => x.Length);
            var rowsCount = inputLines.Count;
            var size = 5;
            var propagate = (r: size, c: size);
            var matrix = new Matris<DjikstraNode>(inputLines.Count * propagate.r, columnsCount * propagate.c, x => new DjikstraNode());
            var lastCell = (0, 0);
            for (int r = 0; r < rowsCount * propagate.r; r++)
            {
                for (int c = 0; c < columnsCount * propagate.c; c++)
                {
                    lastCell = (r, c);
                    matrix.Set(lastCell, new DjikstraNode(lastCell, Calc(r / rowsCount, c / columnsCount, inputLines[r % rowsCount][(c % columnsCount)] - '0'), matrix));
                }
            }

            Console.WriteLine(matrix.ToString((cell,c) => $" {c,1}",0));

            var pathfinder = new DjikstraPathFinder(matrix, (0, 0), lastCell);
            var results = pathfinder.Run();
            Console.WriteLine();
            var best = results
                 .GroupBy(x => x.Result)
                 .OrderBy(x => x.Key)
                 .First();
            foreach (var path in best)
            {
                Console.WriteLine($"{path}");
            }

        }


        private static IEnumerable<string> LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                if (inputLine is { } && inputLine != "")
                {
                    yield return inputLine;
                }
                inputLine = stream.ReadLine();
            }
        }
    }
}