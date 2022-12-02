using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using common;

namespace _3;

internal class Program
{
    private static string _testData =
        @"vJrwpWtwJgWrhcsFMMfFFhFp
jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL
PmmdzqPrVvPwwTWBwg
wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn
ttgJtRGJQctTZtZT
CrZsJsPPZsGzwwsLwLmpwMDw";

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }


    private static void FirstPart(TextReader stream)
    {
        var sum = 0L;
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Chunk(inpLine.Length / 2).ToArray();
            var c = parts[0]
                .FirstOrDefault(x => parts[1]
                    .FirstOrDefault(y => y == x) != 0);
            var p = Prio(c);
            sum += p;
        }

        Debug.WriteLine($"result1 :{sum} ");

    }

    private static void SecondPart(TextReader stream)
    {
        var sum = 0L;
        while (stream.ReadLine() is { } line1)
        {
            var line2 = stream.ReadLine()?.ToCharArray() ?? Array.Empty<char>();
            var line3 = stream.ReadLine()?.ToCharArray() ?? Array.Empty<char>();

            var c = line1
                .FirstOrDefault(x =>
                    line2
                        .FirstOrDefault(y => y == x) != 0
                    &&
                    line3
                        .FirstOrDefault(y => y == x) != 0
                );
            var p = Prio(c);
            sum += p;
        }

        Debug.WriteLine($"result2 :{sum} ");
    }


    private static char Prio(char c)
    {
        return (char)
            (c is >= 'A' and <= 'Z'
            ? c - 'A' + 27
            : c - 'a' + 1);
    }
}