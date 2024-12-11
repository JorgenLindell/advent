using System.Diagnostics;
using common;
/*
 
If the stone is engraved with the number 0, it is replaced by a stone engraved with the number 1.

If the stone is engraved with a number that has an even number of digits, it is replaced by two stones. 
The left half of the digits are engraved on the new left stone, and the right half of the digits are engraved 
on the new right stone. (The new numbers don't keep extra leading zeroes: 1000 would become stones 10 and 0.)
   
If none of the other rules apply, the stone is replaced by a new stone; 
the old stone's number multiplied by 2024 is engraved on the new stone.
 */

var data = StreamUtils.GetLines();
//data = @"
//125 17
//    ".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
var list = data[0]
    .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.ToLong() ?? 0)
    .ToList();

var cache = new Dictionary<(long stone, int step), long>();

var sw = new Stopwatch();
sw.Start();
int stepLimit = 75;

long cnt = list.Count + list.Sum(stone => CalcStone(stone, 0));


Console.WriteLine(cnt);
Console.WriteLine(cache.Count);
Console.WriteLine(sw.ElapsedMilliseconds + " ms");
return;

long CalcStone(long stone, int step)
{
    if (step >= stepLimit)
        return 0;

    if (cache.ContainsKey((stone, step)))
        return cache[(stone, step)];

    var nextStep = step + 1;

    if (stone == 0)
    {
        return cache[(0, step)] = CalcStone(1, nextStep);
    }

    var lengthOfNumber = LengthOfNumber(stone);
    if (lengthOfNumber % 2 == 0)
    {
        var number = stone.ToString();
        var len = lengthOfNumber / 2;
        var new1 = number[..len].ToLong() ?? 0;
        var new2 = number[len..].ToLong() ?? 0;

        var result1 = CalcStone(new1, nextStep);
        var result2 = CalcStone(new2, nextStep);
        return cache[(stone, step)] = 1 + result1 + result2;
    }

    return cache[(stone, step)] = CalcStone(2024 * stone, nextStep);
}

static int LengthOfNumber(long num)
{
    if (num == 0)
        return 1;
    return (int)Math.Floor(Math.Log10(Math.Abs(num)) + 1);
}


