using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using common;

namespace _19
{
    public static class Program
    {
        public static void Main()
        {
            var swt = new Stopwatch();
            swt.Start();

            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //var stream = StreamUtils.GetInputStream(testData: TestDataSet1.Data);
            var sw = new Stopwatch();
            sw.Start();
            var scanners = ScannerFactory.ParseScanners(stream);
            scanners[0].Solved = true;
            while (scanners.Any(s => s.Solved == false))
            {
                Console.WriteLine($" repeat Solver");
                foreach (var scanner1 in scanners)
                {
                    foreach (var scanner2 in scanners)
                    {
                        if (scanner1.Solved && !scanner2.Solved)
                        {
                            var matchDistances = scanner1.MatchKeyPointDistances(scanner2);
                            if (matchDistances.Transform >= 0)
                            {
                                Debug.WriteLine($"{matchDistances} scanner: {scanner2.Number}");
                                scanner2.Transform = matchDistances.Transform;
                                scanner2.Offset = (matchDistances.Offset);
                                scanner2.Solved = true;
                            }
                        }
                    }
                }
            }
            sw.Stop();
            var totalPoints = new Dictionary<Vector3, int>();
            scanners.ForEach(s =>
            {
                Console.WriteLine($"\n---Scanner {s.Number} solved:{s.Solved}");
                Console.WriteLine($" offset:{s.Offset}");
                Console.WriteLine($" transform:{s.Transform}");
                s.CurrentBeacons.ForEach(b =>
                {
                    Console.WriteLine($" {b}");
                    if (!totalPoints.ContainsKey(b))
                    {
                        totalPoints[b] = 0;
                    }

                    totalPoints[b]++;
                });
            });
            Console.WriteLine("\n\nCommon Points");
            foreach (var totalPoint in totalPoints
                         .OrderByDescending(x=>x.Value)
                         .ThenByDescending(x=>x.Key.LengthSquared()))
            {
                Console.WriteLine($" {totalPoint.Key,-23}  seen by {totalPoint.Value} scanners");
            }

            Console.WriteLine($"Solve time= {sw.Elapsed:g}");
            Console.WriteLine("Total count of points= " + totalPoints.Count);
            int max = 0;
            
            foreach (var scanner1 in scanners)
            {
                foreach (var scanner2 in scanners)
                {
                    var diff = scanner1.Offset - scanner2.Offset;
                    var m = Vector3.Abs(diff);
                    var mx = m.X + m.Y + m.Z;
                    max=(int)Math.Max(max, mx);  
                }
            }
            Console.WriteLine("Max dist= " + max);
            Console.WriteLine($"Solve time= {swt.Elapsed:g}");

        }
    }
}

