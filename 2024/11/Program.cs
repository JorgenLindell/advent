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

long cnt = list.Count;
for (var i = 0; i < list.Count; i++)
{
    var stone = list[i];
    cnt += CalcStone(stone, 0, 75);
}


Console.WriteLine(cnt);
Console.WriteLine(cache.Count);
return;

long CalcStone(long stone, int step, int stepLimit)
{
    if (step >= stepLimit)
        return 0;

    var nextStep = step + 1;
    if (stone == 0)
    {
        var result = !cache.ContainsKey((1, nextStep)) ? cache[(1, nextStep)] = CalcStone(1, nextStep, stepLimit) : cache[(1, nextStep)];
        return result;
    }

    if (Math.Abs(stone).ToString().Length % 2 == 0)
    {
        var number = Math.Abs(stone).ToString();
        var len = number.Length / 2;
        var new1 = number[..len].ToLong() ?? 0;
        var new2 = number[len..].ToLong() ?? 0;

        var result1 = !cache.ContainsKey((new1, nextStep)) ? cache[(new1, nextStep)] = CalcStone(new1, nextStep, stepLimit) : cache[(new1, nextStep)];
        var result2 = !cache.ContainsKey((new2, nextStep)) ? cache[(new2, nextStep)] = CalcStone(new2, nextStep, stepLimit) : cache[(new2, nextStep)];

        return 1 + result1 + result2;
    }

    var newStone = 2024 * stone;
    var calcStone = !cache.ContainsKey((newStone, nextStep)) ? cache[(newStone, nextStep)] = CalcStone(newStone, nextStep, stepLimit) : cache[(newStone, nextStep)];
    return calcStone;
}


