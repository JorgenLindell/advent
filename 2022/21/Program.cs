using System.Diagnostics;
using common;


//https://adventofcode.com/2022/day/15
internal class Program
{
    private static readonly string _testData =
        @"root: pppw + sjmn
dbpl: 5
cczh: sllz + lgvd
zczc: 2
ptdq: humn - dvpt
dvpt: 3
lfqf: 4
humn: 5
ljgn: 2
sjmn: drzm * dbpl
sllz: 4
pppw: cczh / lfqf
lgvd: ljgn * ptdq
drzm: hmdt - zczc
hmdt: 32"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        var debug = false;
        FirstPart(GetDataStream(debug), debug);
        SecondPart(GetDataStream(debug), debug);
    }

    private static TextReader GetDataStream(bool debug)
    {
        return debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");
    }

    private static void SecondPart(TextReader stream, bool debug)
    {
    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        var monkeys = LoadMonkeys(stream);
        var root = monkeys["root"];
        var result = root.Result(null, 0);
        Debug.WriteLine("root says:" + result);

        //monkeys.Values.OrderBy(x => x.Name).ForEach((m, i) => Debug.WriteLine(m));

        // part 2
        root.Operation = "=";
        var bottom = monkeys["humn"];

        // get call order upwards
        var list = new List<string>();
        while (bottom.Name != "root")
        {
            list.Add(bottom.Name);
            bottom = bottom.CalledBy;
        }

        var stack = new Stack<string>(list);
        var target = root.NeedToBe(1, stack);

        Debug.WriteLine("target=" + target);

        // redo to test
        stream = GetDataStream(debug);
        monkeys = LoadMonkeys(stream);
        root = monkeys["root"];
        root.Operation = "=";
        monkeys["humn"].SetConstant(target);
        result = root.Result(null, 0);
        Debug.WriteLine($"root says:{result}  meaning the solution {(result == 1 ? "succeeded":"failed")}");
        
    }

    private static Dictionary<string, Monkey> LoadMonkeys(TextReader stream)
    {
        var monkeys = new Dictionary<string, Monkey>();
        while (stream.ReadLine() is { } inpLine)
        {
            var monkey = new Monkey(inpLine, monkeys);
            monkeys[monkey.Name] = monkey;
        }

        return monkeys;
    }
}

internal class Monkey
{
    private static Dictionary<string, Monkey> _monkeys=null!;
    private readonly string _formula;
    private readonly List<(long? value, string? monkey)> _operands = new();
    private long? _result;
    public string Name { get; }
    public string Operation { get; set; } = "";
    public int Level { get; set; }
    public Monkey CalledBy { get; set; } = null!;
    public Monkey(string formula, Dictionary<string, Monkey> dictionary)
    {
        _monkeys = dictionary;
        _formula = formula;
        var parts = formula.Split(": ".ToCharArray(),
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Name = parts[0];
        ParseValue(1);
        if (parts.Length < 3)
        {
            _result = _operands[0].value;
        }
        else
        {
            Operation = parts[2];
            ParseValue(3);
        }

        void ParseValue(int i)
        {
            if (parts[i].ToLong().HasValue)
                _operands.Add((value: parts[i].ToLong(), monkey: null));
            else
                _operands.Add((value: null, monkey: parts[i]));
        }
    }


    public void SetConstant(long target)
    {
        _result = target;
    }

    public long? Result(Monkey calledBy, int level)
    {
        CalledBy = calledBy;
        Level = level;
        if (_result != null)
            return _result;

        if (_operands.Count == 2)
        {
            var a = OperandValue(0);
            var b = OperandValue(1);
            _result = Operation switch
            {
                "=" => a == b ? 1 : 0,
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => _result
            };
        }

        return _result;
    }

    public long NeedToBe(long targetValue, Stack<string> callOrder)
    {
        if (_operands.Count == 1 || callOrder.Count == 0)
            return targetValue;

        var nextMonkey = callOrder.Pop();
        var operIx = _operands.First().monkey == nextMonkey ? 0 : 1;
        long? nextNeeded = 0;
        if (operIx == 0)
        {
            var b = OperandValue(1);
            nextNeeded = Operation switch
            {
                "=" => b,
                "+" => targetValue - b,
                "-" =>
                    // _result = a - b;
                    targetValue + b,
                "*" => targetValue / b,
                "/" =>
                    // _result = a / b;
                    targetValue * b,
                _ => nextNeeded
            };

            return _monkeys[_operands[operIx].monkey!].NeedToBe(nextNeeded!.Value, callOrder);
        }

        var a = OperandValue(0);
        nextNeeded = Operation switch
        {
            "=" => a,
            "+" => targetValue - a,
            "-" =>
                // _result = a - b;
                a - targetValue,
            "*" => targetValue / a,
            "/" =>
                // _result = a / b;
                a / targetValue,
            _ => nextNeeded
        };

        return _monkeys[_operands[operIx].monkey!].NeedToBe(nextNeeded!.Value, callOrder);
    }


    private long? OperandValue(int index)
    {
        if (index < _operands.Count)
            return _operands[index].value != null
                ? _operands[index].value
                : _monkeys[_operands[index].monkey!].Result(this, Level + 1);
        return null;
    }

    public override string ToString()
    {
        return $"{_formula}  {OperandValue(0)} {Operation} {OperandValue(1)}  {Level} {CalledBy?.Name}";
    }
}