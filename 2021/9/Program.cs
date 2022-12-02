using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _9
{
    class Program
    {
        private static Matris<byte> _matrix;
        private static string testData = @"2199943210
3987894921
9856789892
8767896789
9899965678
";


        static void Main(string[] args)
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            // var stream = StreamUtils.GetInputStream(testData: testData);
            LoadStream(stream);

            var lowPoints = FindLowPoints();
            var totRisk = lowPoints.Sum(p =>
              {
                  Console.WriteLine($"{p.r},{p.c}={_matrix.Value(p)}");
                  return _matrix.Value(p) + 1;
              });
            Console.WriteLine("Total risk= " + totRisk);

            var basins = new List<Basin>();
            lowPoints.ForEach(
              action: p =>
              {
                  var basin = new Basin(p, _matrix);
                  basins.Add(basin);
                  Console.WriteLine($"Basin for ({p.r},{p.c}): Size= {basin.Size}");
              });
            var top3 = basins.OrderByDescending(x => x.Size);
            var (t1, t2, t3, _) = top3;
            Console.WriteLine($"Product of top 3 = {t1.Size * t2.Size * t3.Size}");

        }

        private static List<(int r, int c)> FindLowPoints()
        {
            var lowPoints = new List<(int r, int c)>();
            _matrix.EachCell(
              (c, v) =>
              {
                  if (_matrix.Value(c.Up()) > v &&
                _matrix.Value(c.Down()) > v &&
                _matrix.Value(c.Left()) > v &&
                _matrix.Value(c.Right()) > v)
                  {
                      lowPoints.Add(c);
                  }
              });
            return lowPoints;
        }

        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            _matrix = new Matris<byte>(1, inputLine.Length, c => 10);
            var r = 0;
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                var c = 0;
                Console.WriteLine(inputLine);
                foreach (var readChar in inputLine)
                {
                    byte val = (byte)(readChar - '0');
                    _matrix.Set((r, c), val);
                    ++c;
                }

                inputLine = stream.ReadLine();
                ++r;
            }
        }
    }
}
