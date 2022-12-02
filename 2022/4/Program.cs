using System.Diagnostics;
using System.IO;
using common;

namespace _4;
//https://adventofcode.com/2022/day/4

internal class Program
{
    private static string _testData =
        @"2-4,6-8
2-3,4-5
5-7,7-9
2-8,3-7
6-6,4-6
2-6,4-8"
            .Replace("\r\n", "\n");


    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }

    private static void FirstPart(TextReader stream)
    {
        var sum = 0L;
        while (stream.ReadLine() is { } inpLine)
        {
            ParseLine(inpLine, out var r1, out var r2);
            sum += r1.Contains(r2) || r2.Contains(r1) ? 1 : 0;
        }

        Debug.WriteLine($"result1 :{sum} ");
    }

    private static void SecondPart(TextReader stream)
    {
        var sum = 0L;
        while (stream.ReadLine() is { } inpLine)
        {
            ParseLine(inpLine, out var r1, out var r2);
            sum += r1.Intersects(r2) ? 1 : 0;
        }

        Debug.WriteLine($"result2 :{sum} ");
    }

    private static void ParseLine(string inpLine, out Assignment r1, out Assignment r2)
    {
        var ranges = inpLine.Split(',');
        r1 = new Assignment(ranges[0]);
        r2 = new Assignment(ranges[1]);
    }

    private readonly struct Assignment
    {
        private readonly Limits<int> _limits;

        public Assignment(string range)
        {
            var split = range.Split('-');
            _limits = new Limits<int>(split[0].ToInt()!.Value, split[1].ToInt()!.Value);
        }

        public bool Contains(Assignment other)
        {
            return _limits.Contains(other._limits);
        }

        public bool Intersects(Assignment other)
        {
            return _limits.Intersects(other._limits)
                ;
        }
    }
}