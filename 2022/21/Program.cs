using System.Diagnostics;
using System.Reflection;
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
        var result = root.Evaluate(); // try to evaluate to save call order
        Debug.WriteLine("root says:" + result);


        // part 2
        root.Operation = "=";
        var bottom = monkeys["humn"];

        // get call order upwards that was registered during calculation
        var stack = new Stack<string>();
        while (bottom != null && bottom.Name != "root")
        {
            stack.Push(bottom.Name);
            bottom = bottom.CalledBy; // was noted during first evaluation
        }
        // NeedToBe calculates "backwards" what the operand needs to be on each level to get the expected result
        var target = root.NeedToBe(expectedResult: 1, stack);

        Debug.WriteLine("target=" + target);

        // redo to test
        stream = GetDataStream(debug);
        monkeys = LoadMonkeys(stream);
        root = monkeys["root"];
        root.Operation = "=";
        monkeys["humn"].SetConstant(target);
        result = root.Evaluate();
        Debug.WriteLine($"root says:{result}  meaning the solution {(result == 1 ? "succeeded" : "failed")}");

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
    public static Dictionary<string, Monkey> Monkeys = null!;
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
        AddOperand(parts[1]);
        if (parts.Length < 3)
        {
            //value only given
            _result = _operands[0].Value;
        }
        else
        {
            // formula given
            Operation = parts[2];
            AddOperand(parts[3]);
        }

        // local
        void AddOperand(string part)
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

    public long? Evaluate(Monkey? calledBy = null, int level = 0)
    { 
        //ask monkey to calculate its value
        CalledBy = calledBy; //who is asking, remember this to be able to track human value dependents later for part 2
        Level = level;
        if (Level > Monkeys.Count)
        {
            throw new InvalidOperationException($"Recurse level > {Monkeys.Count}, there can't be that many monkeys; loop in definitions")
        }
        if (_result != null)
            return _result; // already have a value, given or calculated

        if (_operands.Count == 2)
        {
            var a = OperandValue(0); // ask for operand 0
            var b = OperandValue(1); // ask for operand 1
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


    public long NeedToBe(long expectedResult, Stack<string> callOrder)
    {
        // if this monkey should return the expected result, what should the next last monkey in the call chain say?
        if (_operands.Count == 1 || callOrder.Count == 0)
            return expectedResult;

        var nextMonkey = callOrder.Pop();
        var operIx = _operands.First().MonkeyName == nextMonkey ? 0 : 1;
        var operOther = operIx == 1 ? 0 : 1;
        long? nextNeeded = 0;
        // order of operands matters in - and / so they need to be handled depending on operand order
 
        var b = OperandValue(operOther);
        nextNeeded = Operation switch
        {
            "=" => b,

            "+" => expectedResult - b,

            "*" => expectedResult / b,

            "-" => operIx == 0
                ? (expectedResult + b) // _result = a - b;)
                : (b - expectedResult), // _result = b - a),

            "/" => operIx == 0
                ? expectedResult * b // _result = a / b;
                : b / expectedResult, // _result = b / a
            _ => throw new InvalidDataException("Unknown operation "+Operation)
        };

        // tell next monkey if it should return nextNeeded, what should the bottom money (=human) say?
        return Monkeys[_operands[operIx].MonkeyName!].NeedToBe(nextNeeded!.Value, callOrder);
    }


    private long? OperandValue(int index)
    {
        // operands[] is either (long? Value, string? MonkeyName) i.e. a set value or a monkey name.
        if (index < _operands.Count)
            return _operands[index].Value != null
                ? _operands[index].Value    // has already a value set from start
                : Monkeys[_operands[index].MonkeyName!].Evaluate(this, Level + 1); // ask monkey to calculate value
        return null; // this is a failure
    }

    public override string ToString()
    {
        return $"{Formula}  {OperandValue(0)} {Operation} {OperandValue(1)}  {Level} {CalledBy?.Name}";
    }
}