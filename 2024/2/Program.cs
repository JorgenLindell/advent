using common;

var lines = StreamUtils.GetLines();
/*  @"7 6 4 2 1
1 2 7 8 9
9 7 6 2 1
1 3 2 4 5
8 6 4 4 1
1 3 6 7 9".Split("\r\n"); 
*/
int ok1 = 0;
int ok2 = 0;
foreach (var line in lines)
{
    List<int> splt = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
        .Select(x => x.ToInt() ?? 0)
        .ToList();
    if (IsValid(splt))
    {
        ok1++;
    }
    else if (TryExcluded(splt))
    {
        ok2++;
    }
}


Console.WriteLine(ok1);
Console.WriteLine(ok2);
Console.WriteLine(ok1 + ok2);

bool TryExcluded(List<int> list)
{
    for (int i = 0; i < list.Count; i++)
    {
        var values = list.ExcludeIndex(i).ToList();
        if (IsValid(values))
        {
            var dir = values.Skip(1).Select((x, ix) => Sign(x, values[ix])).GroupBy(x=>x);
            var max = values.Skip(1).Select((x, ix) => int.Abs(x - values[ix])).Max();

            Console.WriteLine(ok2 +": "+dir.Count()+" "+max+"    |" +string.Join(",", list) + " =>(" + list[i] +") " + string.Join(",", values));
            return true;
        }
    }

    return false;
}
bool IsValid(List<int> list)
{
    var dir = Sign(list[1] , list[0]);
    if (dir == 0) return false;
    for (int i = 1; i < list.Count; i++)
    {
        if (int.Abs(list[i] - list[i - 1]) > 3 ||
            Sign(list[i], list[i - 1]) != dir)
        {
            return false;
        }
    }

    return true;
}

int Sign(int int1, int int2)
{
    if (int1 > int2) return 1;
    if (int1 < int2) return -1;
    return 0;
}