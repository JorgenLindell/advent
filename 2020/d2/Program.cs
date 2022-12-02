
string[] lines = System.IO.File.ReadAllLines(@"./input1.txt");
var validC = 0;
validC = T2(lines, validC);

Console.WriteLine("valid " + validC);


int T1(string[] strings, int i)
{
    foreach (var line in strings)
    {
        var splitLine = line.Split('-', ' ', ':');
        if (splitLine.Length == 0)
            break;
        var f = int.Parse(splitLine[0]);
        var t = int.Parse(splitLine[1]);
        var c = splitLine[2][0];
        var str = splitLine[4];
        var countOfC = str.Count(x => x == c);
        if (countOfC >= f && countOfC <= t)
        {
            i++;
        }
    }

    return i;
}
int T2(string[] strings, int i)
{
    foreach (var line in strings)
    {
        var splitLine = line.Split('-', ' ', ':');
        if (splitLine.Length == 0)
            break;
        var p1 = int.Parse(splitLine[0]) - 1;
        var p2 = int.Parse(splitLine[1]) - 1;
        var c = splitLine[2][0];
        var str = splitLine[4];
        var countOfC = str[p1] == c ? 1 : 0;
        countOfC += str[p2] == c ? 1 : 0;
        if (countOfC == 1)
        {
            i++;
        }
    }

    return i;
}
