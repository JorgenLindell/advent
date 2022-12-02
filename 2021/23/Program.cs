using System.Diagnostics;
using System.Reflection;
using common;

namespace _22;

public static class TestData1
{
    public static string Data =
        @"
";
}

public static class TestData0
{
    public static string Data =
        @"
#############
#...........#
###B#C#B#D###
  #A#D#C#A#
  #########";
}

public static class TestData2
{
    public static string Data =
        @"
#############
#...........#
###B#C#B#D###
  #D#C#B#A#
  #D#B#A#C#
  #A#D#C#A#
  #########";
}

internal class Program
{
    private static void Main()
    {
        var stream = StreamUtils.GetInputStream(file: "input2.txt");
        //var stream = StreamUtils.GetInputStream(testData: TestData2.Data);
        var sw = new Stopwatch();
        sw.Start();
        var house = House2.LoadStream(stream);
        house.StartPlay();

        Console.WriteLine("");
        Console.WriteLine(sw.Elapsed);
    }
}