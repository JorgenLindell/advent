using common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _5
{
    class Program
    {
        static void Main(string[] args)
        {

            var lines = new List<((int r, int c) from, (int r, int c) to)>();
            //var s = StreamUtils.GetInputStream(file: "input.txt");
            var s = StreamUtils.GetInputStream(testData: testData);
            while (true)
            {
                var c1 = s.ReadInt();
                if (c1 == null) break;
                var r1 = s.ReadInt();
                var c2 = s.ReadInt();
                var r2 = s.ReadInt();
                lines.Add(((r1.Value, c1.Value), (r2.Value, c2.Value)));
                //Console.WriteLine($"{r1.Value},{c1.Value} - {r2.Value},{c2.Value}");
            }
            using (var measure = new Measure())
            {

                var maxR = lines.Max(l => Math.Max(l.from.r, l.to.r));
                var maxC = lines.Max(l => Math.Max(l.from.c, l.to.c));
                var matris = new Matris<int>(maxR + 1, maxC + 1, (cell) => 0);

                var overlappingPoints = new Dictionary<(int r, int c), int>();
                foreach (var line in lines)
                {
                    foreach (var cell in line.Cells().ToList())
                    {
                        var val = matris.Value(cell);
                        matris.Set(cell, val + 1);
                        if (val > 0)
                        {
                            overlappingPoints[cell] = val + 1;
                        }
                    }
                }

                Console.WriteLine(matris.ToString((cell,v) => v == 0 ? " . " : $" {v} "));

                Console.WriteLine("Number of overlapping =" + overlappingPoints.Keys.Count);
            }


        }




        private static string testData = @"
0,9 -> 5,9
8,0 -> 0,8
9,4 -> 3,4
2,2 -> 2,1
7,0 -> 7,4
6,4 -> 2,0
0,9 -> 2,9
3,4 -> 1,4
0,0 -> 8,8
5,5 -> 8,2
";

    }
}
