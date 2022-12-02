using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _14
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
        private static string _testData1 = @"NNCB

CH -> B
HH -> N
CB -> H
NH -> C
HB -> C
HC -> B
HN -> C
NN -> C
BH -> H
NC -> B
NB -> B
BN -> B
BB -> N
BC -> B
CC -> N
CN -> C
";

        private static string template;
        private static Dictionary<string, char> rules = new Dictionary<string, char>();
        private static DictionaryWithDefault<string, int, char, ulong> sumPairs
          = new DictionaryWithDefault<string, int, char, ulong>(x => 0);
        private static DictionaryWithDefault<char, ulong> summation = new DictionaryWithDefault<char, ulong>(x => 0);

        private static double _lastreported = 0;
        static void Main()
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //var stream = StreamUtils.GetInputStream(testData: _testData1);
            LoadStream(stream);
            var steps = 40;

            foreach (var tc in template)
            {
                summation[tc]++;
            }
            Console.WriteLine(template);

            var pairs = template.Pairs().ToList();
            double reached = 0;
            foreach (var pair in pairs)
            {
                var results = new DictionaryWithDefault<char, ulong>(x => 0);
                var weight = 1 / (double)pairs.Count;
                ProcessOnePair(pair, steps, weight, ref reached, results);
                foreach (var (k, val) in results)
                {
                    summation[k] += val;
                }
            }

            var dict = new DictionaryWithDefault<char, ulong>(x => 0);
            foreach (var (key, value) in summation)
            {
                dict[key] = value;
            }
            WriteResult("", 0, dict);
            var sumCounts = summation.OrderByDescending(x => x.Value);
            var top = sumCounts.First().Value;
            var bottom = sumCounts.Last().Value;
            Console.WriteLine("\nfirst " + top);
            Console.WriteLine("last " + bottom);

            Console.WriteLine("Final " + (top - bottom));
            Console.WriteLine("max " + ulong.MaxValue);
        }

        public static void ProcessOnePair(string pair, int steps, double weight, ref double reached,
          DictionaryWithDefault<char, ulong> results)
        {
            if (steps == 0)
            {
                reached += weight;
                return;
            }
            if (steps == 1)
            {
                if (reached > _lastreported + 0.01)
                {
                    _lastreported = reached;
                    Console.WriteLine($"{100 * reached:##0.00}%");
                }
            }
            if (rules.ContainsKey(pair))
            {
                if (sumPairs[pair].ContainsKey(steps))
                {
                    reached += weight;
                    foreach (var (key, value) in sumPairs[pair][steps])
                    {
                        results[key] += value;
                    }
                    Console.WriteLine($"Bail at {steps} for {pair}");
                    if (reached > _lastreported + 0.01)
                    {
                        _lastreported = reached;
                        Console.WriteLine($"{100 * reached:##0.00}%");
                    }
                }
                else
                {
                    var insChar = rules[pair];

                    sumPairs[pair][steps][insChar] += 1;
                    results[insChar] += 1;

                    var nextWeight = weight * 0.5d;
                    var nextReached = reached;
                    var p1 = "" + pair[0] + insChar;
                    var p2 = "" + insChar + pair[1];
                    var nextres = new DictionaryWithDefault<char, ulong>(x => 0);
                    ProcessOnePair(p1, steps - 1, nextWeight, ref nextReached, nextres);
                    ProcessOnePair(p2, steps - 1, nextWeight, ref nextReached, nextres);
                    foreach (var (key, value) in nextres)
                    {
                        sumPairs[pair][steps][key] += value;
                        results[key] += value;
                    }
                    reached += weight;
                }
            }
            else
            {
                //no hit
                reached += weight;
            }
        }

        private static void WriteResult(string pair, int step, DictionaryWithDefault<char, ulong> results)
        {
            Console.Write($"{"".PadLeft(step)}From {pair} at level {step} was generated ");
            foreach (var (key, value) in results)
            {
                Console.Write($"{key}:{value}  ");
            }
            Console.WriteLine();
        }


        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                if (inputLine is { } && inputLine != "")
                {
                    if (template == null)
                        template = inputLine.Trim();
                    else
                    {
                        var (pair, insert, _) = inputLine.Split(" ->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        rules[pair] = insert.Trim()[0];
                    }
                }
                inputLine = stream.ReadLine();
            }
        }
    }
}