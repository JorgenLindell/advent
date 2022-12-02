using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using common;

namespace _2;

internal class Program
{
    const int Win = 6;
    const int Draw = 3;
    const int Lose = 0;

    private static readonly Dictionary<string, int> WinsOver = new()
    {
        // rock paper scissors rules

        { "PR", Win },
        { "RS", Win },
        { "SP", Win },
        { "RR", Draw },
        { "PP", Draw },
        { "SS", Draw },
        { "RP", Lose },
        { "SR", Lose },
        { "PS", Lose },
    };

    private static readonly Dictionary<string, int> OwnValue = new()
    {
        { "R", 1 },
        { "P", 2 },
        { "S", 3 }
    };

    private static string _testData =
        @"A Y
B X
C Z";

    private static void Main(string[] args)
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: testData);
        var sum1 = 0L;
        var sum2 = 0L;
        while (stream.ReadLine() is { } inp)
        {
            var trimmed = inp.Replace(" ", "");
            sum1 += Evaluate(trimmed, false);
            sum2 += Evaluate(trimmed, true);
        }
        Debug.WriteLine($"result1 : {sum1}");
        Debug.WriteLine($"result2 : {sum2}");
    }

    private static int Evaluate(string inp, bool strategy2)
    {
        var mapped = inp.Map("ABCXYZ", "RPSRPS");

        var other = mapped.First();
        var own = mapped.Last();

        if (strategy2)
            switch (inp[1])
            {
                case 'X': // lose
                    own = WinsOver.First(v => v.Value == Lose
                                              && v.Key[1] == other).Key[0];
                    break;
                case 'Y': // draw
                    own = WinsOver.First(v => v.Value == Draw
                                              && v.Key[1] == other).Key[0];
                    break;
                case 'Z': // win
                    own = WinsOver.First(v => v.Value == Win
                                              && v.Key[1] == other).Key[0];
                    break;
            }

        return Result(own, other);

        int Result(char mineC,char otherC)
        {
            var s = mineC + "" + otherC;
            return WinsOver[s] + OwnValue["" + mineC];
        }
    }
}