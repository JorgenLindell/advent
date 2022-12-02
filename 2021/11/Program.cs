using common;
using System;
using System.Collections.Generic;
using System.IO;

namespace _11
{
    class Program
    {
        private static string testData = @"5483143223
2745854711
5264556173
6141336146
6357385478
4167524645
2176841721
6882881134
4846848554
5283751526
";

        private static Matris<Octo> _matrix;

        static void Main(string[] args)
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //  var stream = StreamUtils.GetInputStream(testData: testData);
            LoadStream(stream);
            var flashCount = 0;
            var loopCount = 1000;
            for (int i = 0; i < loopCount; i++)
            {
                var triggered = new Queue<(int r, int c)>();
                var flashed = new HashSet<(int r, int c)>();
                _matrix.EachCell((cell, octo) =>
                 {
                     octo.value += 1;
                     if (octo.value > 9)
                     {
                         triggered.Enqueue(cell);
                     }
                 });
                while (triggered.Count > 0)
                {
                    var cell = triggered.Dequeue();
                    if (!flashed.Contains(cell))
                    {
                        flashCount++;
                        var adjacent = cell.GetAdjacentAll(_matrix);
                        foreach (var nextCell in adjacent)
                        {
                            var octo = _matrix.Value(nextCell);
                            octo.value += 1;

                            if (octo.value > 9 && !flashed.Contains(nextCell))
                            {
                                triggered.Enqueue(nextCell);
                            }
                        }
                        flashed.Add(cell);
                    }
                }
                foreach (var cell in flashed)
                {
                    _matrix.Value(cell).value = 0;
                }

                if (flashed.Count >= 100)
                {
                    Console.Write($"MegaFlash at {i + 1}");
                }
                Console.WriteLine($"Flash at {i + 1}: {flashed.Count}");
            }
            Console.WriteLine($"LoopCount: {loopCount}    Flashes: {flashCount}");
        }


        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            _matrix = new Matris<Octo>(1, inputLine.Length, c => new Octo(-999));
            var r = 0;
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                var c = 0;
                Console.WriteLine(inputLine);
                foreach (var readChar in inputLine)
                {
                    int val = (readChar - '0');
                    _matrix.Set((r, c), new Octo(val));
                    ++c;
                }

                inputLine = stream.ReadLine();
                ++r;
            }
        }
    }

    public class Octo
    {
        public int value;

        public Octo(int value)
        {
            this.value = value;
        }
    }
}
