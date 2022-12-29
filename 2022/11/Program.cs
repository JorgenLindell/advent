using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using common;


//https://adventofcode.com/2022/day/11
internal class Program
{
    private static string _testData =
        @"Monkey 0:
  Starting items: 79, 98
  Operation: new = old * 19
  Test: divisible by 23
    If true: throw to monkey 2
    If false: throw to monkey 3

Monkey 1:
  Starting items: 54, 65, 75, 74
  Operation: new = old + 6
  Test: divisible by 19
    If true: throw to monkey 2
    If false: throw to monkey 0

Monkey 2:
  Starting items: 79, 60, 97
  Operation: new = old * old
  Test: divisible by 13
    If true: throw to monkey 1
    If false: throw to monkey 3

Monkey 3:
  Starting items: 74
  Operation: new = old + 3
  Test: divisible by 17
    If true: throw to monkey 0
    If false: throw to monkey 1"
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
    }


    private static void FirstPart(TextReader stream)
    {
        var monkeyList = ParseMonkeys(stream);
        var monkeyDict = monkeyList.ToDictionary(x => x.Name, x => x);

        void TransferFunction(string targetName, long item) => monkeyDict[targetName].Recieve(item);

        for (int roundIndex = 0; roundIndex < 10000; roundIndex++)
        {
            foreach (var monkey in monkeyList)
            {
                monkey.DoTurn(TransferFunction);
            }

            if((roundIndex+1).In(1,20,1000,2000,10000))
            {
                Console.WriteLine($"After Round {roundIndex + 1}");
                foreach (var monkey in monkeyList)
                {
                    Console.WriteLine($"Monkey {monkey.Name}: {monkey.InspectionCount}");
                }
                Console.WriteLine($"");
            }

        }

        var ordered = monkeyList.OrderByDescending(x => x.InspectionCount).ToList();
        Debug.WriteLine($"Most active {ordered[0].InspectionCount} * {ordered[1].InspectionCount} = {(ulong)(ordered[0].InspectionCount) *(ulong)( ordered[1].InspectionCount)}");
    }

    private static List<Monkey> ParseMonkeys(TextReader stream)
    {
        Monkey currentMonkey = null!;
        List<Monkey> monkeyList = new List<Monkey>();
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Split(": ,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                if (parts[0] == "Monkey")
                {
                    if (currentMonkey != null!)
                        monkeyList.Add(currentMonkey);
                    currentMonkey = new Monkey()
                    {
                        Name = parts[1]
                    };
                }
                else if (parts[0] == "Starting")
                {
                    currentMonkey.Items = new Queue<long>();
                    for (int pa = 2; pa < parts.Length; pa++)
                    {
                        currentMonkey.Items.Enqueue(parts[pa].ToLong()!.Value);
                    }
                }
                else if (parts[0] == "Operation")
                {
                    var oper = inpLine.Split('=', StringSplitOptions.TrimEntries)[1];
                    currentMonkey.ParseOperation(oper);
                }
                else if (parts[0] == "Test")
                {
                    currentMonkey.DivBy = parts[3].ToLong()!.Value;
                }
                else if (parts[0] == "If")
                {
                    if (parts[1] == "true")
                    {
                        currentMonkey.TrueMonkey = parts[5];
                    }
                    else
                    {
                        currentMonkey.FalseMonkey = parts[5];
                    }
                }
            }
        }
        if (currentMonkey != null && monkeyList.Last() != currentMonkey)
            monkeyList.Add(currentMonkey);

        return monkeyList;
    }
}

internal class Monkey
{
    private static long _bigDivBy = 1;
    private long _divBy;
    public string Name { get; set; } = "";
    public long Value { get; set; } = 0;
    public Queue<long> Items { get; set; }
    public Func<long, long> Operation { get; private set; } = x => x;

    public long DivBy
    {
        get => _divBy;
        set
        {
            _divBy = value;
            _bigDivBy *= value;
        }
    }

    public string TrueMonkey { get; set; } = "";
    public string FalseMonkey { get; set; } = "";
    public int InspectionCount { get; private set; } = 0;

    public void ParseOperation(string oper)
    {
        var parts = oper.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        long Func(long old)
        {
            var arg1 = old;
            var arg2 = old;
            if (parts[0] != "old") arg1 = parts[0].ToLong()!.Value;
            if (parts[2] != "old") arg2 = parts[2].ToLong()!.Value;
            switch (parts[1])
            {
                case "+":
                    return arg1 + arg2;
                case "-":
                    return arg1 - arg2;
                case "*":
                    return arg1 * arg2;
                case "/":
                    return arg1 / arg2;
            }

            return old;
        }
        Operation = Func;
    }

    public void DoTurn(Action<string, long> transferFunction)
    {
        while (Items.Count > 0)
        {
            this.InspectionCount++;
            var worry = Items.Dequeue();
            worry = Operation(worry) % _bigDivBy;
            transferFunction(worry % DivBy == 0
                ? TrueMonkey
                : FalseMonkey,
                worry);
        }
    }


    public void Recieve(long item)
    {
        Items.Enqueue(item );
    }
}
