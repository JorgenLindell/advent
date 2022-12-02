using common;

namespace _18;

public class SnafuNumber : BTreeNode<int, SnafuNumber>
{
    public SnafuNumber? SnafuLeft
    {
        get => NodeLeft;
        set => NodeLeft = value;
    }
    public SnafuNumber? SnafuRight
    {
        get => NodeRight;
        set => NodeRight = value;
    }

    public SnafuNumber? SnafuParent
    {
        get => Parent;
        set => Parent = value;
    }

    public SnafuNumber(){}
    public SnafuNumber((int, int) p)
    : base(p)
    {
    }

    public SnafuNumber(int i1, int i2)
    : base(i1, i2)
    {
    }

    public SnafuNumber(int i)
    : base(i)
    {
    }

    public SnafuNumber(SnafuNumber n1, SnafuNumber n2)
        : base(n1, n2)
    {
    }

    public static SnafuNumber Parse(string input)
    {
        var stack = new Stack<object>();
        foreach (var c in input.ToCharArray())
            if (c == '[')
            {
                stack.Push(c);
            }
            else if (c.In("0123456789"))
            {
                var snafuNumber = new SnafuNumber(int.Parse("" + c));
                stack.Push(snafuNumber);
            }
            else if (c == ']')
            {
                var n2 = (SnafuNumber)stack.Pop();
                var n1 = (SnafuNumber)stack.Pop();
                var x = stack.Pop();
                var n = new SnafuNumber(n1, n2);
                stack.Push(n);
            }

        if (stack.Count != 1)
            throw new InvalidDataException("Input string was not balanced");
        return (SnafuNumber)stack.Pop();
    }

    public bool IsRegular => IsValueNode;
    public static SnafuNumber operator +(SnafuNumber a, SnafuNumber b) => a.Add(b); // new object
    public static implicit operator SnafuNumber((int, int) p) => new SnafuNumber(p);
    public static implicit operator SnafuNumber(int p) => new SnafuNumber(p);

    public int Magnitude
    {
        get
        {
            if (IsRegular)
                return Value;

            return 3 * SnafuLeft!.Magnitude + 2 * SnafuRight!.Magnitude;
        }
    }
    public bool NeedExplode => NumberOfParents > 3 && (SnafuLeft?.IsRegular ?? false) && (SnafuRight?.IsRegular ?? false);
    public bool NeedSplit => IsRegular && Value > 9;

    public void ReduceAll()
    {
        //  - If any pair is nested inside four pairs, the leftmost such pair explodes.
        //  - If any regular number is 10 or greater, the leftmost such regular number splits.
        IEnumerable<SnafuNumber> foundE;
        IEnumerable<SnafuNumber> foundS = (new SnafuNumber[] { });
        do
        {
            foundE = Root.Where(s => s.NeedExplode).ToList();
            foreach (var e in foundE.Take(1))
                e.Explode();

            if (!foundE.Any())
            {
                foundS = Root.Where(s => s.NeedSplit).ToList();
                foreach (var s in foundS.Take(1))
                    s.Split();
            }

        } while (foundE.Any() || foundS.Any());
    }



    public SnafuNumber? Find(int i1, int i2)
    {
        if ((SnafuLeft?.IsRegular ?? false) && (SnafuRight?.IsRegular ?? false))
        {
            if (SnafuLeft?.Value == i1 && SnafuRight?.Value == i2)
                return this;
        }
        else
        {
            var found = SnafuLeft?.Find(i1, i2);
            found ??= SnafuRight?.Find(i1, i2);
            return found;
        }

        return null;
    }

    public SnafuNumber Add(SnafuNumber term)
    {
        var newSnafu = new SnafuNumber(Clone(), term.Clone());
        newSnafu.Root.ReduceAll();
        return newSnafu;
    }


    public void Explode()
    {
        /*
* To explode a pair, the pair's left value is added to the first regular number
* to the left of the exploding pair (if any),
* and the pair's right value is added  to the first regular number to the right
* of the exploding pair (if any).
*
* Exploding pairs will always consist of two regular numbers.
* Then, the entire exploding pair is replaced with the regular number 0.
*
*/
        if (NeedExplode)
        {
            var report = new List<SnafuNumber>();
            Report("before explode");
            var regularLeft = GetValueNodeToTheLeft(this);
            var regularRight = GetValueNodeToTheRight(this);
            if (regularLeft != null)
            {
                regularLeft.Value += SnafuLeft?.Value ?? 0;
                report.Add(regularLeft);
            }

            if (regularRight != null)
            {
                regularRight.Value += SnafuRight?.Value ?? 0;
                report.Add(regularRight);
            }

            var replaced = SnafuParent!.ReplaceMe(this, 0);
            report.Insert(0,replaced);
            Report("after explode " + report.Skip(1).Select(s => "" + s.Value).StringJoin() + "\n", report);

        }
    }

    private void Report(string message, IEnumerable<SnafuNumber> report = null!)
    {
        if (!ReportFlag) return;
        string[] substrings;
        if (report != null!)
        {
            // have been replaced in explode
            var snafuNumber = Root.ToString();//updates positions

            var reportList = report.Skip(1).OrderBy(x => x.PosInLine).ToList();

            substrings = new string[reportList.Count() * 2 + 1];
            var v = 0;
            var used = 0;
            for (int i = 0; i < reportList.Count(); i++)
            {
                var prefix = snafuNumber.Substring(used, reportList[i].PosInLine - used);
                substrings[v++] = prefix;
                var value = reportList[i].ToString();
                substrings[v++] = value;
                used += (prefix.Length + value.Length);
            }
            substrings[v] = snafuNumber.Substring(used);
        }
        else
        {
            var fragment = ToString();
            var snafuNumber = Root.ToString();//updates positions
            var posInLine = PosInLine;
            substrings = new[]
            {
                snafuNumber.Substring(0, posInLine),
                snafuNumber.Substring(posInLine, fragment.Length),
                snafuNumber.Substring(posInLine + fragment.Length),
            };

        }
        Console.Write($"{this,7}:");
        Console.Write($"{substrings[0]}");
        var written = substrings[0].Length;
        for (int i = 1; i < substrings.Length; i++)
        {
            if ((i & 1) == 1)
                Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write($"{substrings[i]}");
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.WriteLine($"  {message}");
    }

    public void Split()
    {
        /*
         * To split a regular number, replace it with a pair;
         * the left element of the pair should be the regular number divided by two and rounded down,
         * while the right element of the pair should be the regular number divided by two and rounded up.
         *
         * For example, 10 becomes [5,5], 11 becomes [5,6], 12 becomes [6,6], and so on.
         */
        if (IsRegular && Value > 9)
        {
            Report("before Split");
            int v1 = (int)Math.Floor(Value / 2d);
            int v2 = (int)Math.Ceiling(Value / 2d);
            SnafuLeft = new SnafuNumber(v1) { SnafuParent = this };
            SnafuRight = new SnafuNumber(v2) { SnafuParent = this };
            Report("after Split\n");
        }
    }
}