using System;
using System.Collections.Generic;
using System.Linq;

namespace common;
public class IntCodeMachine2019
{
    internal class Instruction
    {
        public long OpCode { get; }
        public int Length { get; }
        public Func<Memory, int, int> Operation { get; }

        public Instruction(
            long opCode,
            int length,
            Func<Memory, int, Instruction,  int> operation
        )
        {
            OpCode = opCode;
            Length = length;
            Operation =(m,i)=> operation(m,i,this);
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
        public void SetRef(int a, long content) => _cells[_cells[a]] = content;

    }
    private readonly long[] _inputCells;
    private int Ip { get; set; }

    private Dictionary<long, Instruction> Instructions { get; } =
        new()
        {
            /*add*/
            [1] = new Instruction(1, 4, (m, ip,instr) =>
            {
                    m.SetRef(ip + 3, m.Ref(ip + 1) + m.Ref(ip + 2));
                    return ip + instr.Length;
            }),
            /*mul*/
            [2] = new Instruction(2, 4, (m, ip,instr) =>
            {
                    m.SetRef(ip + 3, m.Ref(ip + 1) * m.Ref(ip + 2));
                    return ip + instr.Length;
            })
        };



    public IntCodeMachine2019(IEnumerable<long> inputCells)
    {
        this._inputCells = inputCells.ToArray();
    }
    public long[] Evaluate(int noun, int verb)
    {
        var memory = new Memory(_inputCells);
        memory.Set(1, noun);
        memory.Set(2, verb);

        Ip = 0;
        while (memory.At(Ip) != 99 )
        {
            var instr = Instructions[memory.At(Ip)];
            Ip = instr.Operation(memory, Ip);
        }
        return memory.Cells;
    }
}