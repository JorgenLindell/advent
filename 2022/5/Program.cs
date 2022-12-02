using System.Diagnostics;
using System.IO;
using common;

namespace _5;
//https://adventofcode.com/2022/day/5

internal class Program
{
    private static string _testData =
        @""
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

        }
        Debug.WriteLine($"result1 :{sum} ");

    }

    private static void SecondPart(TextReader stream)
    {
        var sum = 0L;
        while (stream.ReadLine() is { } inpLine)
        {

        }
        Debug.WriteLine($"result2 :{sum} ");
    }

   
}