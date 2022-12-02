using common;
using System;
using System.IO;

namespace _12
{
    class Program
    {
        private static string testData1 = @"start-A
start-b
A-c
A-b
b-d
A-end
b-end
";
        private static string testData2 = @"dc-end
HN-start
start-kj
dc-start
dc-HN
LN-dc
HN-end
kj-sa
kj-HN
kj-dc
";
        private static string testData3 = @"
start-DX
fs-DX
fs-end
he-DX
fs-he
pj-DX
end-zg
zg-sl
zg-pj
pj-he
RW-he
pj-RW
zg-RW
start-pj
he-WI
zg-he
pj-fs
start-RW
";


        static void Main(string[] args)
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //  var stream = StreamUtils.GetInputStream(testData: testData3);
            LoadStream(stream);
            var stC = CaveSystemSolver.GetCave("start");
            var enC = CaveSystemSolver.GetCave("end");


            var result = CaveSystemSolver.GetPaths(stC, enC, true);
            Console.WriteLine($"Found {result.Count} paths:");
            result.ForEach(p =>
            {
                Console.WriteLine("Path: " + p);
            });
            Console.WriteLine($"Found {result.Count} paths:");
        }
        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            var r = 0;
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                if (!inputLine.IsNullOrEmpty())
                {
                    var (start, end, _) = inputLine.Split('-');
                    var stC = CaveSystemSolver.GetCave(start.Trim());
                    var enC = CaveSystemSolver.GetCave(end.Trim());
                    Console.WriteLine($"{stC.Name}-{enC.Name}");
                    CaveSystemSolver.AddLink(stC, enC);
                }
                inputLine = stream.ReadLine();
                ++r;
            }
        }

    }
}
