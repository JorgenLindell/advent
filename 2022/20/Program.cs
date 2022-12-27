using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Transactions;
using common;


//https://adventofcode.com/2022/day/25
internal class Program
{
    private static readonly string _testData =
        @"1
2
-3
3
-2
0
4"
            //@""
            .Replace("\r\n", "\n");

    private static bool _debug = false;

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream);
        SecondPart(GetDataStream);
    }

    private static TextReader GetDataStream()
    {
        return _debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");
    }

    private static void SecondPart(Func<TextReader> getStream)
    {
    }


    private static void FirstPart(Func<TextReader> getStream)
    {
        var lines = Load(getStream());

        var list = new DoubleLinkedList();
        foreach (var line in lines)
        {
            list.Add(line.ToLong()!.Value);
        }
        list.MixOnce();
        var nodeAt1000 = list.NodeAt(1000);
        var nodeAt2000 = nodeAt1000.Go(1000);
        var nodeAt3000 = nodeAt2000.Go(1000);


        Debug.WriteLine("Sum lines="+(nodeAt1000.Value+ nodeAt2000.Value+ nodeAt3000.Value));
    }


    private static List<string> Load(TextReader stream)
    {
        var lines = new List<string>();
        while (stream.ReadLine() is { } inpLine)
            lines.Add(inpLine);
        Debug.WriteLine("Read lines=" + lines.Count);

        return lines;
    }
}

internal class DoubleLinkedList
{
    private DoubleLinkedNode? _root;
    private readonly Dictionary<long, DoubleLinkedNode> _dict = new();
    private readonly List<DoubleLinkedNode> _list = new();

    public DoubleLinkedList()
    {
        DoubleLinkedNode.List = this;
        _root = null!;
    }
    public DoubleLinkedNode Add(long key)
    {
        var node = new DoubleLinkedNode(key);

        if (_dict.Count == 0)
        {
            _dict[key] = node;
            _root = node;
        }
        else
        {
            _dict[key] = node;
            _root!.Prev.InsertAfter(node);
        }
        _list.Add(node);
        return node;
    }

    public DoubleLinkedNode NodeAt(long index) => _root?.Go(index) 
                                                  ?? throw new InvalidDataException("List is empty");
    public void MixOnce()
    {
        foreach (var node in _list)
        {
            node.MoveSteps(node.Value);
        }
    }

    public IEnumerable<long> ToList()
    {
        if (! (_root is{})) yield break;

        var node = _root;
        do
        {
            yield return node.Value;
            node = node.Next;
        } while (node != _root);

    }
    internal class DoubleLinkedNode
    {
        public long Value { get; }
        internal DoubleLinkedNode Prev;
        internal DoubleLinkedNode Next;
        internal static DoubleLinkedList? List;
        public DoubleLinkedNode()
        {
            Prev = this;
            Next = this;
        }

        public DoubleLinkedNode(long value)
            : this()
        {
            Value = value;
        }

        public void Extract()
        {
            this.Prev.Next = this.Next;
            this.Next.Prev = this.Prev;
            this.Next = this;
            this.Prev = this;
        }
        public void InsertAfter(DoubleLinkedNode node)
        {
            if (node == this.Prev)
                return;

            if (this.Prev != this)
            {
                Extract();
            }

            this.Next = node.Next;
            node.Next.Prev = this;
            this.Prev = node;
            node.Next = this;

        }

        public void MoveSteps(long steps)
        {
            var toInsertAfter = Go(steps);
            this.Extract();
            this.InsertAfter(toInsertAfter);
        }

        public DoubleLinkedNode Go(long steps)
        {
            DoubleLinkedNode toInsertAfter=this;
            if (steps < 0)
            {
                toInsertAfter = this.Prev;
                for (long i = 0; i < -steps; i++)
                {
                    toInsertAfter = toInsertAfter.Prev;
                }
            }
            else if (steps > 0)
            {
                toInsertAfter = this;
                for (int i = 0; i < steps; i++)
                {
                    toInsertAfter = toInsertAfter.Next;
                }
            }

            return toInsertAfter;
        }
    }
}

