using System.Diagnostics;
using System.Text;
using common;


//https://adventofcode.com/2022/day/10
internal class Program
{
    private static string _testData =
        @"addx 15
addx -11
addx 6
addx -3
addx 5
addx -1
addx -8
addx 13
addx 4
noop
addx -1
addx 5
addx -1
addx 5
addx -1
addx 5
addx -1
addx 5
addx -1
addx -35
addx 1
addx 24
addx -19
addx 1
addx 16
addx -11
noop
noop
addx 21
addx -15
noop
noop
addx -3
addx 9
addx 1
addx -3
addx 8
addx 1
addx 5
noop
noop
noop
noop
noop
addx -36
noop
addx 1
addx 7
noop
noop
noop
addx 2
addx 6
noop
noop
noop
noop
noop
addx 1
noop
noop
addx 7
addx 1
noop
addx -13
addx 13
addx 7
noop
addx 1
addx -33
noop
noop
noop
addx 2
noop
noop
noop
addx 8
noop
addx -1
addx 2
addx 1
noop
addx 17
addx -9
addx 1
addx 1
addx -3
addx 11
noop
noop
addx 1
noop
addx 1
noop
noop
addx -13
addx -19
addx 1
addx 3
addx 26
addx -30
addx 12
addx -1
addx 3
addx 1
noop
noop
noop
addx -9
addx 18
addx 1
addx 2
noop
noop
addx 9
noop
noop
noop
addx -1
addx 2
addx -37
addx 1
addx 3
noop
addx 15
addx -21
addx 22
addx -6
addx 1
noop
addx 2
addx 1
noop
addx -10
noop
noop
addx 20
addx 1
addx 2
addx 2
addx -6
addx -11
noop
noop
noop"
            .Replace("\r\n", "\n");

    //     private static string _testData =
    //       @"noop
    // addx 3
    // addx -5"
    //       .Replace("\r\n", "\n");
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
        var lines = new List<string>();
        while (stream.ReadLine() is { } inpLine) lines.Add(inpLine);

        var logging = new List<(int cycle, long reg)>();

        var datamaskin = new AocProcessor();

        datamaskin.Register["X"] = 1;

        datamaskin.InsertInspection(20 + 0 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });
        datamaskin.InsertInspection(20 + 1 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });
        datamaskin.InsertInspection(20 + 2 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });
        datamaskin.InsertInspection(20 + 3 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });
        datamaskin.InsertInspection(20 + 4 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });
        datamaskin.InsertInspection(20 + 5 * 40, x => { logging.Add((x.Clock, x.Register["X"])); });

        var crt = new Crt();
        datamaskin.ClockConsumer = crt;

        datamaskin.ExecuteProgram(lines);
        var sum = logging.Sum(x => x.cycle * x.reg);
        Debug.WriteLine("result1:" + sum);

        crt.rows.ForEach(x=>
            Debug.WriteLine(x.ToString()));
    }
}

public interface IClockConsumer
{
    void Tick(int clock, AocProcessor processor);
}

public class Crt : IClockConsumer
{
    public List<StringBuilder> rows = new List<StringBuilder>();

    public void Tick(int clock, AocProcessor processor)
    {
        clock--;
        var row = clock / 40;
        var col = clock - (row * 40);
        while (row >= rows.Count)
            rows.Add(new StringBuilder("".PadLeft(40, '.')));
        var reg = processor.Register["X"];
        if (reg - 1 <= col && reg + 1 >= col)
            rows[row][col] = '#';
    }
}

public class AocProcessor
{
    private readonly Dictionary<int, Action<AocProcessor>> _inspections = new();

    internal InstructionState CurrentInstrState = null!;

    public List<Instruction> Instructions = new()
    {
        new Instruction("noop", 1, 0, (instr, proc, args) =>
        {
            if (instr.CyclesToGo == 0)
            {
                proc.Pc++;
            }

        }),
        new Instruction("addx", 2, 1, (instr, proc, args) =>
        {
            if (instr.CyclesToGo == 0)
            {
                proc.Register["X"] += args[0].ToLong()!.Value;
                proc.Pc++;
            }
        })
    };

    public IClockConsumer? ClockConsumer { get; set; } = null;
    public int Clock { get; set; }

    public int Pc { get; set; }
    public Dictionary<string, long> Register { get; } = new();

    public void ExecuteProgram(List<string> programList)
    {
        Pc = 0;
        Clock = 1;
        CurrentInstrState = Decode(programList[Pc]);
        do //Pc is incremented by each instruction at its end.
        {
            if (_inspections.ContainsKey(Clock))
                _inspections[Clock](this);
            ClockConsumer?.Tick(Clock, this);
            var getNext = CurrentInstrState.Tick(this);
            Clock++;
            if (getNext && Pc < programList.Count)
                CurrentInstrState = Decode(programList[Pc]);
        } while (Pc < programList.Count);
    }

    private InstructionState Decode(string statement)
    {
        var split = statement.Split(' ');
        var instr = Instructions.First(x => x.Name == split[0]);
        var args = split.Skip(1).ToList();
        return new InstructionState(Clock, instr, args);
    }

    public void InsertInspection(int i, Action<AocProcessor> action)
    {
        _inspections[i] = action;
    }

    public class InstructionState
    {
        private readonly List<string> _arguments;
        private readonly int _completesAtCycle;
        private readonly Instruction _instruction;
        private readonly int _startedAtCycle;
        internal int CyclesToGo;

        public InstructionState(int startedAtCycle, Instruction instruction, List<string> arguments)
        {
            _startedAtCycle = startedAtCycle;
            _instruction = instruction;
            _arguments = arguments;
            _completesAtCycle = startedAtCycle + instruction.Cycles + startedAtCycle;
            CyclesToGo = instruction.Cycles;
        }

        public bool Tick(AocProcessor processor)
        {
            CyclesToGo--;
            _instruction.ExecuteTick(this, processor, _arguments);
            return CyclesToGo <= 0;
        }
    }

    public class Instruction
    {

        public Instruction(string name, int cycles, int argcount,
            Action<InstructionState, AocProcessor, List<string>> action)
        {
            Cycles = cycles;
            Name = name;
            ArgCount = argcount;
            ExecuteTick = action;
        }


        public string Name { get; }
        public int Cycles { get; }
        public int ArgCount { get; }
        public Action<InstructionState, AocProcessor, List<string>> ExecuteTick { get; }
    }
}