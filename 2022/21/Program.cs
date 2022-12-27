using System.Diagnostics;
using common;


//https://adventofcode.com/2022/day/21
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
        var result = root.Result(); // try to evaluate to save call order
        Debug.WriteLine("root says:" + result);


        // part 2
        root.Operation = "=";
        var bottom = monkeys["humn"];

        // get call order upwards that was registered during calculation
        var stack = new Stack<string>();
        while (bottom!=null && bottom.Name != "root")
        {
            stack.Push(bottom.Name);
            bottom = bottom.CalledBy; // was noted during first evaluation
        }

        var target = root.NeedToBe(1, stack);

        Debug.WriteLine("target=" + target);

        // redo to test
        stream = GetDataStream(debug);
        monkeys = LoadMonkeys(stream);
        root = monkeys["root"];
        root.Operation = "=";
        monkeys["humn"].SetConstant(target);
        result = root.Result();
        Debug.WriteLine($"root says:{result}  meaning the solution {(result == 1 ? "succeeded":"failed")}");
        
    }

    private static Dictionary<string, Monkey> LoadMonkeys(TextReader stream)
    {
        Monkey.Monkeys = new Dictionary<string, Monkey>();
        while (stream.ReadLine() is { } inpLine)
        {
            var monkey = new Monkey(inpLine);
            Monkey.Monkeys[monkey.Name] = monkey;
        }

        return Monkey.Monkeys;
    }
}

internal class Monkey
{
    public static Dictionary<string, Monkey> Monkeys=null!;
    private readonly List<(long? Value, string? MonkeyName)> _operands = new();
    private long? _result;
    public string Name { get; }
    public string Operation { get; set; } = "";
    public int Level { get; set; } // not essential, just for debug
    public Monkey? CalledBy { get; set; } //save who was asking for value. Assuming only one is asking.

    public Monkey(string formula)
    {
    //root: pppw + sjmn
    //dbpl: 5

        var parts = formula.Split(": ".ToCharArray(),
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Name = parts[0];
        ParseValue(parts[1]);
        if (parts.Length < 3)
        { 
            //value only given
            _result = _operands[0].Value;
        }
        else
        {
            // formula given
            Operation = parts[2];
            ParseValue(parts[3]);
        }

        // local
        void ParseValue( string part)
        {
            //Add value or monkey name to operands
            if (part.ToLong().HasValue)
                _operands.Add((Value: part.ToLong(), MonkeyName: null));
            else
                _operands.Add((Value: null, MonkeyName: part));
        }
    }

    public string Formula =>
        $"{_operands[0].MonkeyName}{(_operands.Count > 1 ? Operation + _operands[1].MonkeyName : _result)}";


    public void SetConstant(long value)
    {
        _result = value;
    }

    public long? Result(Monkey? calledBy=null, int level=0)
    {
        CalledBy = calledBy; //who is asking
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
        else
        {
            throw new InvalidDataException($"This monkey formula should have 2 operands:{Formula}");
        }

        return _result;
    }


    public long NeedToBe(long targetValue, Stack<string> callOrder)
    {
        if (_operands.Count == 1 || callOrder.Count == 0)
            return targetValue;

        var nextMonkey = callOrder.Pop();
        var operIx = _operands.First().MonkeyName == nextMonkey ? 0 : 1;
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

            return Monkeys[_operands[operIx].MonkeyName!].NeedToBe(nextNeeded!.Value, callOrder);
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

        return Monkeys[_operands[operIx].MonkeyName!].NeedToBe(nextNeeded!.Value, callOrder);
    }


    private long? OperandValue(int index)
    {
        if (index < _operands.Count)
            return _operands[index].Value != null
                ? _operands[index].Value // has already a value
                : Monkeys[_operands[index].MonkeyName!].Result(this, Level + 1); // calculate value
        return null;
    }

    public override string ToString()
    {
        return $"{Formula}  {OperandValue(0)} {Operation} {OperandValue(1)}  {Level} {CalledBy?.Name}";
    }
}