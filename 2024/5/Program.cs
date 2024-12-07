
using common;
#pragma warning disable S4158

var data = StreamUtils.GetLines();

var rules = new HashSet<(string, string)>();
var updates = new List<List<string>>();

data.ForEach((line, _) =>
{
    if (line.Contains('|'))
    {
        var splt = line.Split('|');
        rules.Add((splt[0], splt[1]));
    }
    if (line.Contains(','))
    {
        updates.Add(line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList());
    }
});


var totSumMid = 0;
var badlyOrdered = new List<List<string>>();

updates.ForEach((update, _) => {
    var allOk = true;
    for (int i = 0; i < update.Count - 1; i++)
    {
        if (HasRule(update[i + 1], update[i]))
        {
            //both present, but wrong order (there is a rule against)
            allOk = false;
            //do the first swap already
            (update[i], update[i + 1]) = (update[i + 1], update[i]);
            badlyOrdered.Add(update);
            break;
        }
    }

    if (allOk)
    {
        int mid = (update.Count / 2);
        totSumMid += update[mid].ToInt() ?? 0;
    }
});
Console.WriteLine(totSumMid);
totSumMid = 0;

badlyOrdered.ForEach((update, _) =>
{
    //bubble sort values
    var allOk = false;
    while (!allOk)
    {
        allOk = true;

        for (int i = 0; i < update.Count - 1; i++)
        {
            if (HasRule(update[i + 1], update[i]))
            {
                //there is a rule against this, swap them
                allOk = false;
                (update[i], update[i + 1]) = (update[i + 1], update[i]);
            }
        }
    }
    int mid = (update.Count / 2);
    totSumMid += update[mid].ToInt() ?? 0;
});
Console.WriteLine(totSumMid);
return;

bool HasRule(string s1, string s2) => rules.Contains((s1, s2));

