using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using common;


//https://adventofcode.com/2022/day/20
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
        Debug.WriteLine("============================= Part 2 ======================");
        var encryptionKey = 811589153L;
        var numberOfMix = 10;
        var lines = Load(getStream());

        var list = new DoubleLinkedList();
        foreach (var line in lines)
        {
            list.Add(line.ToLong()!.Value * encryptionKey);
        }

        var verify = list.AsEnumerable().ToList();
        var equal = (verify.Count == lines.Count && verify.SequenceEqual(lines.Select(line => line.ToLong()!.Value)));
        Debug.WriteLine($"List valid: {equal} Min value={verify.Min()} Max value={verify.Max()}");
        if (list.Count < 50) Debug.WriteLine(list);
        for (int i = 0; i < numberOfMix; i++)
        {
            Debug.WriteLine("===== Mix " + (i + 1));
            list.MixOnce();
        }


        var node0 = list.NodeByValue(0);
        var nodeAt1000 = node0.Go(1000);
        var nodeAt2000 = node0.Go(2000);
        var nodeAt3000 = node0.Go(3000);
        Debug.WriteLine($"{nameof(nodeAt1000)} {nodeAt1000.Value}");
        Debug.WriteLine($"{nameof(nodeAt2000)} {nodeAt2000.Value}");
        Debug.WriteLine($"{nameof(nodeAt3000)} {nodeAt3000.Value}");

        Debug.WriteLine("Sum lines= " + (nodeAt1000.Value + nodeAt2000.Value + nodeAt3000.Value));

    }


    private static void FirstPart(Func<TextReader> getStream)
    {
        var lines = Load(getStream());

        var list = new DoubleLinkedList();
        foreach (var line in lines)
        {
            list.Add(line.ToLong()!.Value);
        }

        var verify = list.AsEnumerable().ToList();
        var equal = (verify.Count == lines.Count && verify.SequenceEqual(lines.Select(line => line.ToLong()!.Value)));
        Debug.WriteLine($"List valid: {equal} Min value={verify.Min()} Max value={verify.Max()}");
        if (list.Count < 50) Debug.WriteLine(list);

        list.MixOnce();


        var node0 = list.NodeByValue(0);
        var nodeAt1000 = node0.Go(1000);
        var nodeAt2000 = node0.Go(2000);
        var nodeAt3000 = node0.Go(3000);
        Debug.WriteLine($"{nameof(nodeAt1000)} {nodeAt1000.Value}");
        Debug.WriteLine($"{nameof(nodeAt2000)} {nodeAt2000.Value}");
        Debug.WriteLine($"{nameof(nodeAt3000)} {nodeAt3000.Value}");

        Debug.WriteLine("Sum lines= " + (nodeAt1000.Value + nodeAt2000.Value + nodeAt3000.Value));
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
    private readonly List<DoubleLinkedNode> _orgList = new();
    public int Count => _orgList.Count;
    public DoubleLinkedList()
    {
        DoubleLinkedNode.List = this; // assuming only one instance...
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
            InsertAfter(_root!.Prev, node);
        }
        _orgList.Add(node);
        return node;
    }

    public DoubleLinkedNode NodeAt(long index) => _root?.Go(index)
                                                  ?? throw new InvalidDataException("List is empty");
    public void MixOnce()
    {
        foreach (var node in _orgList)
        {
            var before = this.ToString();
            node.MoveSteps(node.Value);
            if (Count < 50)
            {
                Debug.WriteLine($"\n{node.Value}");
                Debug.WriteLine(before);
                Debug.WriteLine(this);
            }
        }
    }

    public IEnumerable<long> AsEnumerable()
    {
        if (!(_root is { })) yield break;

        var node = _root;
        do
        {
            var nodeValue = node.Value;
            node = node.Next;
            yield return nodeValue;
        } while (node != _root);
    }
    private void InsertAfter(DoubleLinkedNode oldNode, DoubleLinkedNode newNode)
    {
        if (newNode == oldNode.Prev)
            return;

        if (newNode.Prev != newNode)
        {
            newNode.Extract();
        }

        newNode.Next = oldNode.Next;
        oldNode.Next.Prev = newNode;
        oldNode.Next = newNode;
        newNode.Prev = oldNode;
    }

    public DoubleLinkedNode NodeByValue(long value)
    {
        return _dict[value];
    }
    public override string ToString()
    {
        var list1 = AsEnumerable().ToList().Select(x => "" + x).StringJoin(", ") ?? "";
        return list1 + " | " + list1 + " | " + list1;
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
            if (List!._root == this)
                List._root = this.Next; // To keep printout stable
            this.Prev.Next = this.Next;
            this.Next.Prev = this.Prev;
            this.Next = this;
            this.Prev = this;
        }


        public void MoveSteps(long steps)
        {
            if (steps != 0)
            {
                var starting = Next;
                if (steps < 0)
                {
                    starting = Prev;
                }
                else
                {
                    steps -= 1;
                }

                this.Extract();
                var count = List!.Count - 1;
                DoubleLinkedNode current = starting;
                steps %= count;

                if (steps < 0)
                    for (long i = 0; i < -steps; i++)
                        current = current.Prev;
                else if (steps > 0)
                    for (long i = 0; i < steps; i++)
                        current = current.Next;

                List!.InsertAfter(current, this);
            }
        }

        public DoubleLinkedNode Go(long steps)
        {
            steps %= List!.Count;
            var current = this;
            if (steps < 0)
                for (long i = 0; i < -steps; i++)
                    current = current.Prev;
            else if (steps > 0)
                for (long i = 0; i < steps; i++)
                    current = current.Next;

            return current;
        }

        public override string ToString()
        {
            return $"{Value} next:{Next.Value} prev:{Prev.Value}";
        }
    }

}

