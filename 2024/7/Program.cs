using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using common;

var data = StreamUtils.GetLines();
//data =
//    @"
//190: 10 19
//3267: 81 40 27
//83: 17 5
//156: 15 6
//7290: 6 8 6 15
//161011: 16 10 13
//192: 17 8 14
//21037: 9 7 18 13
//292: 11 6 16 20
//".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

var totSum = 0L;
foreach (var line in data)
{
    var parts = line.Split(": ".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.ToLong() ?? 0).ToList() ?? [];
    totSum += Evaluate(parts);
}

Console.WriteLine(totSum);


return;

long Evaluate(List<long> parts)
{
    List<string> operatorsList = new();
    var opers = "+*|";
    var opsCount = parts.Count - 2;
    var pow = Math.Pow(opers.Length, opsCount);
    for (int x = 0; x < pow; x++)
    {
        var ops = ConvertBase(x, opers.Length, opers).PadLeft(opsCount, opers[0]);
        operatorsList.Add(ops);
    }

    foreach (var ops in operatorsList)
    {
        long result = parts[1];
        for (int i = 2; i < parts.Count; i++)
        {
            var opsIx = i - 2;
            result = ops[opsIx] switch
            {
                '+' => result + parts[i],
                '*' => result * parts[i],
                '|' => (result.ToString() + parts[i]).ToLong() ?? 0,
                _ => result
            };
        }

        if (result == parts[0])
            return result;
    }

    return 0;
}
static string ConvertBase(long value, int radix, string digits = "0123456789abcdefghijklmnopqrstuvxyz")
{
    if ((radix > digits.Length) || (radix < 2))
        throw new ArgumentOutOfRangeException(nameof(radix), radix, $"Radix has to be within range [2, {digits.Length}];");

    StringBuilder sb = new StringBuilder();
    do
    {
        long remainder = value % radix;
        value /= radix;

        sb.Insert(0, digits[(int)remainder]);
    } while (value > 0);
    return sb.ToString();
}
