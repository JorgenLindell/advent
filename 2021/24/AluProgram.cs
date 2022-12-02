using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using common;

namespace _22
{
    public class AluProgram
    {
        public class Statement
        {
            public int Row { get; }
            private readonly AluProgram _prog;
            public OpCode Op;
            public string P1;
            public long P1N
            {
                get => Variables[P1];
                set => Variables[P1] = value;
            }

            public string P2;
            private readonly Action<AluProgram, Statement> _action;
            public long P2N
            {
                get
                {
                    if (!Variables.ContainsKey(P2) && long.TryParse(P2, out var parsed))
                    {
                        Variables[P2] = parsed;
                    }
                    return Variables[P2];
                }
            }


            public void Action()
            {
                string History(string par)
                {
                    if (_prog.Tracking[par] != null!)
                        return _prog.Tracking[par];
                    if ("" + Variables[par] == par)
                    {
                        return par;
                    }

                    return par + ":(" + Variables[par] + ")";
                }
                if (_prog.Trace)
                {
                    var s = $"[{Row}]{$"{Op} {P1} {P2} =".PadLeft(15)} ({P1N})";
                    _action(_prog, this);
                    s = $"{s} ({P2N}) => {P1N}".PadRight(36);
                    if (_prog.Track)
                    {
                        if (Op == OpCode.Inp)
                        {
                            _prog.Tracking[P1] = $"{P2}";
                        }
                        else if (Op == OpCode.Mul && P2 == "0")
                        {
                            _prog.Tracking[P1] = $"{P1}{_prog.OpSymbol[Op]}{P2}";
                        }
                        else
                        {
                            _prog.Tracking[P1] = $"({History(P1)}) {_prog.OpSymbol[Op]} ({History(P2)})";
                        }
                        s += "\n" +
                             " " + _prog.Tracking[P1];
                    }

                    Console.WriteLine(s);
                }
                else
                {
                    _action(_prog, this);
                }
            }
            public void Translate()
            {
                string History(string p)
                {
                    var history = "(" + _prog.Tracking[p] + ")";
                    history = history == "()" ? p : history;
                    history = history == "(0)" ? "0" : history;
                    return history;
                }

                var s = $"[{Row}]{$"{Op} {P1} {P2} =".PadLeft(15)}";
                if (Op == OpCode.Inp)
                {
                    _prog.Tracking[P1] = $"{P2 + Row}";
                }
                else if (Op == OpCode.Mul && P2 == "0")
                {
                    _prog.Tracking[P1] = $"0";
                }
                else if (Op == OpCode.Mul && P2 == "1")
                {
                }
                else if (Op == OpCode.Div && P2 == "1")
                {
                }
                else if (Op == OpCode.Eql)
                {
                    _prog.Tracking[P1] = $"({History(P1)}=={History(P2)}):1:0";
                }
                else
                {
                    _prog.Tracking[P1] = $"{History(P1)} {_prog.OpSymbol[Op]} {History(P2)}";
                }

                s += "\t//\t" + _prog.Tracking[P1];

                Console.WriteLine(s);

            }

            public override string ToString()
            {
                return $"{Op} {P1} {P2} ";

            }

            public Statement(int row, AluProgram prog, OpCode opcode, string p1, string p2,
                Action<AluProgram, Statement> action)
            {
                Row = row;
                _prog = prog;
                Op = opcode;
                P1 = p1;
                P2 = p2;
                if (long.TryParse(p2, out var parseResult))
                    Variables[p2] = parseResult;
                _action = action;
            }

            public DictionaryWithDefault<string, long> Variables => _prog.Variables;
        }


        public enum OpCode { Nop, Inp, Add, Mul, Div, Mod, Eql }
        public Dictionary<OpCode, string> OpSymbol = new()
        {

            [OpCode.Nop] = "",
            [OpCode.Inp] = "<-",
            [OpCode.Add] = "+",
            [OpCode.Mul] = "*",
            [OpCode.Div] = "/",
            [OpCode.Mod] = "%",
            [OpCode.Eql] = "=="
        };

        public DictionaryWithDefault<string, long> Variables = new DictionaryWithDefault<string, long>(x => 0);
        public DictionaryWithDefault<string, string?> Tracking = new DictionaryWithDefault<string, string?>(x => null);
        public class Datasource
        {
            private string _data;
            public int Pos;

            public Datasource(string data)
            {
                _data = data;
                Pos = 0;
            }

            public long Next()
            {
                var substring = _data.Substring(Pos++, 1);
                long.TryParse(substring, out var parseResult);

                return parseResult;
            }
            public void SetData(string data)
            {
                _data = data;
                Pos = 0;
            }

            public void Reset()
            {
                Pos = 0;
            }
        }

        public Datasource Data;
        public List<Statement> Statements = new List<Statement>();
        public bool Trace;
        public bool Track;

        public AluProgram(TextReader stream, string data)
        {
            Data = new Datasource(data);
            var row = 0;
            while (stream.Peek() != -1)
            {

                var opcode = stream.ReadWord();
                var p1 = stream.ReadWord();
                /*
                     inp a - Read an input value and write it to variable a.
                     add a b - Add the value of a to the value of b, then store the result in variable a.
                     mul a b - Multiply the value of a by the value of b, then store the result in variable a.
                     div a b - Divide the value of a by the value of b, truncate the result to an integer, then store the result in variable a. (Here, "truncate" means to round the value toward zero.)
                     mod a b - Divide the value of a by the value of b, then store the remainder in variable a. (This is also called the modulo operation.)
                     eql a b - If the value of a and b are equal, then store the value 1 in variable a.Otherwise, store the value 0 in variable a.

                  */
                switch (opcode.ToLower())
                {
                    case "inp":
                        Statements.Add(new Statement(++row, this, OpCode.Inp, p1, "Inp",
                            (alu, statement) =>
                            {
                                statement.P2 = "Inp" + Data.Pos;
                                var inp = Data.Next();
                                Variables[statement.P2] = inp;
                                statement.P1N = inp;
                            }));
                        break;
                    case "add":
                        Statements.Add(new Statement(++row, this, OpCode.Add, p1, stream.ReadWord(),
                            (alu, statement) => statement.P1N += statement.P2N));
                        break;
                    case "mul":
                        Statements.Add(new Statement(++row, this, OpCode.Mul, p1, stream.ReadWord(),
                            (alu, statement) => statement.P1N *= statement.P2N));
                        break;
                    case "div":
                        Statements.Add(new Statement(++row, this, OpCode.Div, p1, stream.ReadWord(),
                            (alu, statement) => statement.P1N =(long) Math.Truncate((statement.P1N /(double) statement.P2N)+0.0000000001)));
                        break;
                    case "mod":
                        Statements.Add(new Statement(++row, this, OpCode.Mod, p1, stream.ReadWord(),
                            (alu, statement) => statement.P1N %= statement.P2N));
                        break;
                    case "eql":
                        Statements.Add(new Statement(++row, this, OpCode.Eql, p1, stream.ReadWord(),
                            (alu, statement) => statement.P1N = statement.P1N == statement.P2N ? 1 : 0));
                        break;
                    default:
                        break;
                }

                var end = stream.ReadLine();
            }
        }

        public void Execute(string input)
        {
            Data.SetData(input);
            Variables
                .Where(kvp => "" + kvp.Value != kvp.Key)
                .ToList()
                .ForEach(kvp => Variables.Remove(kvp.Key));
            Statements.ForEach(s => s.Action());
        }

        public void Execute(int start, int length, string input)
        {
            var statements = Statements.GetRange(start, length);
            Data.SetData(input);
            statements.ForEach(s => s.Action());
        }
        public void Translate(int start, int length)
        {
            var statements = Statements.GetRange(start, length);
            statements.ForEach(s => s.Translate());
        }

    }


}