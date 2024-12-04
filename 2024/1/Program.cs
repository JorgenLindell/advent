using common;

var lines = StreamUtils.GetLines();
var list1 = new List<int>();
var list2 = new List<int>();
foreach (var line in lines)
{
    var splt = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    list1.Add(splt[0].ToInt() ?? 0);
    list2.Add(splt[1].ToInt() ?? 0);
}
list1.Sort();
list2.Sort();
var totdiff = 0;
for (int i = 0; i < list1.Count; i++)
{
    totdiff += int.Abs(list1[i] - list2[i]);
}
Console.WriteLine(totdiff);
var tot = list1.Select(x => list2.Count(y => y == x) * x).Sum();
Console.WriteLine(tot);
