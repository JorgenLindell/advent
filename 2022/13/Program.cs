using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using common;


//https://adventofcode.com/2022/day/13
internal class Program
{
    private static string _testData =
        @"[1,1,3,1,1]
[1,1,5,1,1]

[[1],[2,3,4]]
[[1],4]

[9]
[[8,7,6]]

[[4,4],4,4]
[[4,4],4,4,4]

[7,7,7,7]
[7,7,7]

[]
[3]

[[[]]]
[[]]

[1,[2,[3,[4,[5,6,7]]]],8,9]
[1,[2,[3,[4,[5,6,0]]]],8,9]"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }

    private static void SecondPart(TextReader stream)
    {
        var pair = 1;
        var sumOfGood = 0;
        var fullList = new List<ListValue>();

        while (stream.ReadLine() is { } inpLine)
        {
            if (inpLine.Length > 1)
            {
                var list1 = ListValue.Parse(inpLine);
                fullList.Add(list1);
            }
        }

        var entry2 = ListValue.Parse("[[2]]");
        fullList.Add(entry2);
        var entry6 = ListValue.Parse("[[6]]");
        fullList.Add(entry6);

        var ordered = fullList.OrderBy(x=>x,new ListValueComparer()).ToList();
        foreach (var listValue in ordered)
        {
            Debug.WriteLine(listValue);
        }

        var index2 = ordered.IndexOf(entry2);
        var index6 = ordered.IndexOf(entry6);

        Debug.WriteLine(index2 + 1);
        Debug.WriteLine(index6 + 1);
        Debug.WriteLine((index2 + 1)*(index6 + 1));

    }


    private static void FirstPart(TextReader stream)
    {
        var pair = 1;
        var sumOfGood = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            var part1 = inpLine;
            var part2 = stream.ReadLine();
            stream.ReadLine(); //divider
            int consumed = 0;
            var str = $"{part1},{part2}";
            var list = ListValue.Parse(str, out consumed);
            Debug.WriteLine(list.Values[0]);
            Debug.WriteLine(list.Values[1]);
            var checkResult = list.Values[0].CheckOrder(list.Values[1]);
            Debug.WriteLine(checkResult);
            if (checkResult == ListValue.CheckResult.Good)
            {
                sumOfGood += pair;
            }

            pair++;
        }
        Debug.WriteLine("Sum of good pairs=" + sumOfGood);
    }

}

public class ListValue : IComparable<ListValue>
{
    public bool IsList { get; set; } = true;
    public int? SingleValue { get; set; } = null;
    public List<ListValue> Values { get; set; } = new();
    private void AddNumber(int number)
    {
        Values.Add(new ListValue(number));
    }
    private void AddList(ListValue list)
    {
        Values.Add(list);
    }

    public ListValue()
    {

    }
    public ListValue(int value)
    {
        IsList = false;
        SingleValue = value;
    }

    public int CompareTo(ListValue? other)
    {
        var res = CheckOrder(other);
        return res switch
        {
            CheckResult.Good => -1,
            CheckResult.Bad => 1,
            _ => 0
        };
    }

    public override string ToString()
    {
        if (IsList)
        {
            return "[" + Values.Select(x => x.ToString()).StringJoin(",") + "]";
        }
        else
        {
            return "" + SingleValue;
        }
    }

    public static ListValue Parse(string input, out int consumed)
    {
        var result = new ListValue();
        var numberStart = -1;
        for (consumed = 0; consumed < input.Length; consumed++)
        {
            if (input[consumed] == '[')
            {
                numberStart = -1;
                var newList = Parse(input.Substring(consumed + 1), out int innerConsumed);
                consumed += innerConsumed;
                consumed++;
                result.AddList(newList);
            }
            else if (input[consumed].In("0123456789") && numberStart < 0)
            {
                numberStart = consumed;
            }
            else if (input[consumed].In("],"))
            {
                if (numberStart >= 0)
                {
                    var number = input.Substring(numberStart, consumed - numberStart).ToInt()!.Value;
                    result.AddNumber(number);
                }
                numberStart = -1;
                if (input[consumed] == ']')
                {
                    return result;
                }
            }
        }

        return result;
    }

    public enum CheckResult
    {
        Bad,
        Undecided,
        Good
    };
    public CheckResult CheckOrder(ListValue b)
    {
        ListValue a = this;
        if (a.IsList && b.IsList)
        {
            var i = 0;
            CheckResult r = CheckResult.Undecided;
            while (i < a.Values.Count && i < b.Values.Count)
            {
                r = a.Values[i].CheckOrder(b.Values[i]);
                if (r != CheckResult.Undecided)
                    return r;
                i++;
            }

            if (a.Values.Count < b.Values.Count)
            {
                return CheckResult.Good;

            }
            if (a.Values.Count > b.Values.Count)
            {
                return CheckResult.Bad;
            }

            return CheckResult.Undecided;

        }
        if (!a.IsList && !b.IsList)
        {
            return a.SingleValue == b.SingleValue ? CheckResult.Undecided : a.SingleValue < b.SingleValue ? CheckResult.Good : CheckResult.Bad;
        }
        if (a.IsList)
        {
            return a.CheckOrder(b.AsList());
        }
        if (b.IsList)
        {
            return a.AsList().CheckOrder(b);
        }

        return CheckResult.Undecided;
    }

    private ListValue AsList()
    {
        var res = new ListValue();
        res.Values.Add(new ListValue(SingleValue.Value));
        return res;
    }

    public static ListValue Parse(string part1)
    {
        return Parse(part1, out int consumed);
    }

  
}

public class ListValueComparer : IComparer<ListValue>
{
  public int Compare(ListValue? x, ListValue? y)
    {
        return x.CompareTo(y);
    }
}
