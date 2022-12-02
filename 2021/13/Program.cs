using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _13
{
    class Program
    {
        private static string _testData1 = @"
6,10
0,14
9,10
0,3
10,4
4,11
6,0
6,12
4,1
0,13
10,12
3,4
3,0
8,4
1,10
2,14
8,10
9,0

fold along y=7
fold along x=5
";

        private static Matris<char> _matrix;

        private static readonly List<(int r, int c)> Dots = new List<(int r, int c)>();
        private static readonly List<(char direction, int number)> Folds = new List<(char direction, int number)>();
        private const char Bgnd = '.';
        private const char Dot = '#';

        static void Main()
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //var stream = StreamUtils.GetInputStream(testData: _testData1);
            LoadStream(stream);
            var maxC = Dots.Max(x => x.c) + 1;
            var maxR = Dots.Max(x => x.r) + 1;
            _matrix = new Matris<char>(maxR, maxC, x => Bgnd);
            Dots.ForEach(p =>
            {
                _matrix.Set((p.r, p.c), Dot);
            });
            Console.WriteLine("Initial");

            Console.WriteLine("Initial Visible dots: " + _matrix.Cells.Sum(cell => cell.Value == Dot ? 1 : 0));

            foreach (var fold in Folds)
            {
                _matrix.Fold(fold, Bgnd);
                Console.WriteLine("Visible dots: " + _matrix.Cells.Sum(cell => cell.Value == Dot ? 1 : 0));
            }

            Console.WriteLine("\nFinal");
            Console.WriteLine(_matrix.ToString("{0}{0}"));
        }
        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                if (inputLine is { } && inputLine != "")
                {
                    if (inputLine.StartsWith("fold"))
                    {
                        var (before, after, _) = inputLine.Split('=');
                        Folds.Add((before.EndsWith("x") ? 'c' : 'r', int.Parse(after)));
                    }
                    else
                    {
                        var (c, r, _) = inputLine.Split(',').Select(int.Parse);
                        Dots.Add((r, c));
                    }
                }
                inputLine = stream.ReadLine();
            }
        }
    }
}