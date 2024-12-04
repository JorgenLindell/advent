using System.Diagnostics;
using System.Text.RegularExpressions;
using common;

var data = StreamUtils.GetInputStream("../../../input.txt").ReadToEnd();
var re = new Regex(@"(do\(\))|(don't\(\))|(mul\((\d{1,3}),(\d{1,3})\))");
var matches = re.Matches(data);

var tot = 0;
var isDoing = true; //initially ON
foreach (Match match in matches)
{
    if (match.ToString() == "do()")
        isDoing = true;
    else if (match.ToString() == "don't()")
        isDoing = false;
    else if (isDoing)
    {
        var x = match.Groups[4].Value.ToInt() ?? 0;
        var y = match.Groups[5].Value.ToInt() ?? 0;
        tot += x * y;
    }
}


Console.WriteLine(tot);

