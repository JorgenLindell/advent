using System.Diagnostics;
using common;

namespace _22;

public static class TestData1
{
    public static string Data =
        @"
inp w
add z w
mod z 2
div w 2
add y w
mod y 2
div w 2
add x w
mod x 2
div w 2
mod w 2";
}

public static class TestData2
{
    public static string Data =
        @"
";
}

internal class Program
{
    private static void Main()
    {
        //var stream = StreamUtils.GetInputStream(file: "input2.txt");
        var stream = StreamUtils.GetInputStream(testData: AluCode.Statements);
        //var stream = StreamUtils.GetInputStream(testData: TestData1.Data);
        var sw = new Stopwatch();
        sw.Start();
        //var p = new AluProgram(stream, "");
        //p.Translate(0, p.Statements.Count);
        var all = stream.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var r = Run(all);
        Console.WriteLine(r.Item1);
        Console.WriteLine(r.Item2);
    }

    public static (long, long) Run(string[] input)
    {
        var instructionGroups = input.Chunk(18).ToList();
        var constantInstructions = instructionGroups
            .SelectMany(instr =>
                new[] { instr[4], instr[5], instr[15] }
            ).ToList();

        var constantGroups = constantInstructions
            .Select(c => Convert.ToInt32(c.Substring(6))).Chunk(3);
        
        var sets = constantGroups
            .Select(c => ( addX: c[1], addY: c[2]))
            .ToArray();

        var nums = Enumerable.Range(1, 9);

        IEnumerable<long> Check(int[] constants, int pos, int z)
        {

            if (constants.Length == sets.Length)
                return z == 0
                    ? new[] { long.Parse(string.Join("", constants)) }
                    : Array.Empty<long>();

            return sets[pos].addX < 0
                ? Sub(nums.Where(n => z % 26 + sets[pos].addX == n),
                    w => z / 26)
                : Sub(nums, w => z * 26 + w + sets[pos].addY);
  
            
            IEnumerable<long> Sub(IEnumerable<int> range, Func<int, int> z)
            {
                return range.SelectMany(
                    w => Check(constants.Append(w).ToArray(), pos + 1, z(w)));
            }
        }

        var serials = Check(Array.Empty<int>(), 0, 0);
        return (serials.Min(), serials.Max());
    }


}