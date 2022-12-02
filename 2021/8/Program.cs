using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _8
{
    internal class Program
    {
        private static readonly LedDigit[] Digits = LedDigit.Digits
          .Select(x => new LedDigit(x))
          .OrderBy(x => x.Value)
          .ToArray();

        // acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab | cdfeb fcadb cdfeb cdbaf

        private static string testData = @"
be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | fdgacbe cefdb cefbgd gcbe
edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | fcgedb cgb dgebacf gc
fgaebd cg bdaec gdafb agbcfd gdcbef bgcad gfac gcb cdgabef | cg cg fdcagb cbg
fbegcd cbd adcefb dageb afcb bc aefdc ecdab fgdeca fcdbega | efabcd cedba gadfec cb
aecbfdg fbg gf bafeg dbefa fcge gcbea fcaegb dgceab fcbdga | gecf egdcabf bgf bfgea
fgeab ca afcebg bdacfeg cfaedg gcfdb baec bfadeg bafgc acf | gebdcfa ecba ca fadegcb
dbcfg fgd bdegcaf fgec aegbdf ecdfab fbedc dacgb gdcebf gf | cefg dcbef fcge gbcadfe
bdfegc cbegaf gecbf dfcage bdacg ed bedf ced adcbefg gebcd | ed bcgafe cdgba cbgef
egadfb cdbfeg cegd fecab cgb gbdefca cg fgcdab egfdb bfceg | gbdfcae bgc cg cgb
gcafb gcf dcaebfg ecagb gf abcdeg gaef cafbge fdbac fegbdc | fgae cfgab fg bagce
";

        private static void Main(string[] args)
        {
            // var stream = StreamUtils.GetInputStream(testData: testData);
            var stream = StreamUtils.GetInputStream("input.txt");
            var lines = new List<InputLine>();
            var inputLine = InputLine.Create(stream);
            while (inputLine != null)
            {
                lines.Add(inputLine);
                inputLine = InputLine.Create(stream);
            }

            //find distinct
            var distinctDigits = Digits.Where(d => d.Value.In("1478")).ToDictionary(x => x.Value, x => x);
            CountDistinctDigitsInWiresAll(lines, distinctDigits);

            Console.WriteLine("DiffMatrix:");
            ListDiffMatrix();

            Console.WriteLine();
            Console.WriteLine("AndXor tests:");
            ListSubtractXor();

            // Actual solve
            var summed = 0uL;
            for (var i = 0; i < lines.Count; i++)
            {
                var res = lines[i].SolveLine();
                if (lines[i].DisplayValue < 0) throw new InvalidDataException("Dit not solve line " + i);
                Console.WriteLine("Solved Line: " + i + " Display: " + lines[i].DisplayValue + "\n");
                summed += (ulong)lines[i].DisplayValue;
            }

            Console.WriteLine("Total: " + summed);
        }

        private static void ListSubtractXor()
        {
            var lengthOrdered = Digits.GroupBy(x => x.Length).OrderBy(x => x.Count()).SelectMany(x => x).ToList();

            int r;
            var lista = new Dictionary<(int, int), List<(LedDigit, LedDigit)>>();
            r = 0;
            foreach (var ledDigit1 in lengthOrdered)
            {
                var c = 0;
                foreach (var ledDigit2 in lengthOrdered)
                {
                    var key = InputLine.AndXor(ledDigit1.Set, ledDigit2.Set);
                    if (!lista.ContainsKey(key))
                        lista[key] = new List<(LedDigit, LedDigit)>();

                    lista[key].Add((ledDigit1, ledDigit2));
                    ++c;
                }

                ++r;
            }

            foreach (var listItem in lista)
            {
                Console.WriteLine(listItem.Key.Item1 + "," + listItem.Key.Item2 + " (counts) ");
                foreach (var digitTuple in listItem.Value)
                {
                    var res1 = digitTuple.Item1.SubtractSegments(digitTuple.Item2);
                    var res2 = digitTuple.Item1.XorSegments(digitTuple.Item2);
                    Console.WriteLine(
                      $"    Numbers:{digitTuple.Item1.Value}({digitTuple.Item1.Set})[{digitTuple.Item1.Set.Length}], {digitTuple.Item2.Value}({digitTuple.Item2.Set})[{digitTuple.Item2.Set.Length}]   =>  {res1.Set}  {res2.Set}");
                }
            }
        }

        private static void ListDiffMatrix()
        {
            var lengthOrdered = Digits.GroupBy(x => x.Length).OrderBy(x => x.Count()).SelectMany(x => x).ToList();
            var matris = new Matris<SegmentSet>(lengthOrdered.Count, lengthOrdered.Count, x => SegmentSet.Empty);

            var r = 0;
            foreach (var ledDigit1 in lengthOrdered)
            {
                var c = 0;
                foreach (var ledDigit2 in lengthOrdered)
                {
                    var res = ledDigit1.Set & ~ledDigit2.Set;
                    matris.Set((ledDigit1.Value - '0', ledDigit2.Value - '0'), res);
                    ++c;
                }

                ++r;
            }

            var matr = matris.ToString((cell, x) => $" {x,9}")
              .Replace("\r", "")
              .Split('\n');
            Console.Write("".PadLeft(21));
            for (var c = 0; c < 10; c++) Console.Write(" {0,9:#}", lengthOrdered[c].Set);

            Console.WriteLine();
            Console.WriteLine("".PadRight(125, '-'));

            Console.Write("".PadLeft(21));
            for (var c = 0; c < 10; c++) Console.Write(" {0,9}", "" + lengthOrdered[c].Value);

            Console.WriteLine();
            for (r = 0; r < 10; r++)
            {
                Console.Write(" {0,9:#}", lengthOrdered[r].Set);
                Console.Write(" {0,5}", "" + lengthOrdered[r].Value);
                Console.Write(" {0,4:0}", lengthOrdered[r].Length);
                Console.WriteLine(matr[r]);
            }

            Console.WriteLine();
        }

        private static void CountDistinctDigitsInWiresAll(List<InputLine> lines, Dictionary<char, LedDigit> distinctDigits)
        {
            var cnt = 0;
            foreach (var line in lines)
                foreach (var hashSet in line.Wires)
                    if (distinctDigits.Values.Any(d => d.Length == hashSet.Length))
                        cnt++;

            Console.WriteLine($"Distinct chars={cnt}");
        }
    }
}