using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic;

namespace common;
public class IntCodeMachine2019
{
    internal class Instruction
    {
        public long OpCode { get; }
        public int Length { get; }
        public Func<Memory, int, char[], int> Operation { get; }

        public Instruction(
            long opCode,
            int length,
            Func<Memory, int, Instruction, char[], int> operation
        )
        {
            OpCode = opCode;
            Length = length;
            Operation = (m, i, mode) => operation(m, i, this, mode);
        }
    }

    internal class Memory
    {
        private readonly long[] _cells;

        public Memory(long[] cells)
        {
            this._cells = cells.ToArray();
        }

        public long[] Cells => _cells.ToArray();

        public long At(int a) => _cells[a];
        public long Set(int a, long content) => _cells[a] = content;
        public long Ref(int a) => _cells[_cells[a]];
        public long Get(int a, char mode) => mode == '0' ? Ref(a) : At(a);
        public void SetRef(int a, long content) => _cells[_cells[a]] = content;

    }
    private readonly long[] _inputCells;
    private int Ip { get; set; }

    private Dictionary<long, Instruction> Instructions { get; } =
        new()
        {
            /*add*/
            [1] = new Instruction(1, 4, (m, ip, instr, mode) =>
            {
                m.SetRef(ip + 3, m.Get(ip + 1, mode[0]) + m.Get(ip + 2, mode[1]));
                return ip + instr.Length;
            }),
            /*mul*/
            [2] = new Instruction(2, 4, (m, ip, instr, mode) =>
            {
                m.SetRef(ip + 3, m.Get(ip + 1, mode[0]) * m.Get(ip + 2, mode[1]));
                return ip + instr.Length;
            }),
            /*inp*/
            [3] = new Instruction(3, 2, (m, ip, instr, mode) =>
            {

                m.SetRef(ip + 1, InputBox());
                return ip + instr.Length;
            }),
            /*inp*/
            [4] = new Instruction(4, 2, (m, ip, instr, mode) =>
        {

            var l = m.Get(ip + 1, mode[0]);
            Debug.WriteLine($"out: {l}");
            return ip + instr.Length;
        })
        };

    private static long InputBox()
    {
        long? res = null;
        string? s = "";
        do
        {
            Console.Write("Give input: ");
            s = Console.ReadLine();
        } while (!s.ToLong().HasValue);
        var l = s.ToLong()!.Value;
        Debug.WriteLine($"Input: {l}");
        return l;
    }


    public IntCodeMachine2019(IEnumerable<long> inputCells)
    {
        this._inputCells = inputCells.ToArray();
    }
    public long[] Evaluate(int noun, int verb)
    {
        var memory = new Memory(_inputCells);

        memory.Set(1, noun);
        memory.Set(2, verb);

        Run(memory);

        return memory.Cells;
    }
    public long[] Evaluate()
    {
        var memory = new Memory(_inputCells);

        Run(memory);
        return memory.Cells;
    }

    private void Run(Memory memory)
    {
        Ip = 0;
        while (memory.At(Ip) != 99)
        {
            var formatted = memory.At(Ip).ToString("00000");
            var opCode = formatted.Substring(3, 2).ToLong()!.Value;
            var mode = formatted.Take(3).Reverse().ToArray();
            var instr = Instructions[opCode];
            Ip = instr.Operation(memory, Ip, mode);
        }
    }
}