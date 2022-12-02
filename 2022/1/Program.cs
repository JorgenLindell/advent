using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using common;
namespace _1
{
    internal class Program
    {
        private static string testData =
@"1000
2000
3000

4000

5000
6000

7000
8000
9000

10000";
        private static void Main(string[] args)
        {
            var eol = new Regex(@"^(\r|\r\n|\n)");
            var number = new Regex(@"^[0-9]+");
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //var stream = StreamUtils.GetInputStream(testData: testData);
            var eoln = stream.ReadWhileMatch(eol);
            var tomte = 1;
            long? inp;
            var results = new Dictionary<int, long>();
            do
            {
                var sum = 0L;
                while ((inp = stream.ReadWhileMatch(number).ToLong()) != null)
                {
 //                   Debug.WriteLine($"{inp.Value}");

                    sum += inp.Value;
                    eoln = stream.ReadWhileMatch(eol);
                }
                Debug.WriteLine($"Tomte {tomte}: {sum}");
                results[tomte] = sum;
                ++tomte;
                eoln = stream.ReadWhileMatch(eol);
            } while (eoln != "");

            var sorted=results.OrderByDescending(x => x.Value).ToList();
            Debug.WriteLine($"Vinnare Tomte {sorted[0].Key}: {sorted[0].Value}");
            Debug.WriteLine($"Top 3: {sorted.Take(3).Sum(x=>x.Value)}");
        }
    }
}
