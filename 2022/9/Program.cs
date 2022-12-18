using System.Diagnostics;
using common;


//https://adventofcode.com/2022/day/9
internal class Program
{
    private static string _testData =
        @"R 4
U 4
L 3
D 1
R 4
D 1
L 5
R 2"
.Replace("\r\n", "\n");



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
        Knot head = new Knot(1, 1);
        List<Knot> tail = new();
        Enumerable.Range(1, 9).ForEach((x, _) => tail.Add(new Knot(1, 1)));

        while (stream.ReadLine() is { } inpLine)
        {
            head.Move(inpLine, tail);
        }

        var cellcount = tail.Last().history.GroupBy(x => x).Count();

        Debug.WriteLine($"result1:{cellcount} ");

    }


    private static void FirstPart(TextReader stream)
    {
        Knot head = new Knot(1, 1);
        List<Knot> tail = new();
        Enumerable.Range(1, 1).ForEach((x,_) => tail.Add(new Knot(1, 1)));

        while (stream.ReadLine() is { } inpLine)
        {
            head.Move(inpLine, tail);
        }

        var cellcount = tail.Last().history.GroupBy(x => x).Count();

        Debug.WriteLine($"result1:{cellcount} ");
    }

}

internal class Knot
{
    private int r = 0;
    private int c = 0;
    public List<(int r, int c)> history = new();

    public Knot(int c, int r)
    {
        this.c = c;
        this.r = r;
        history.Add((r, c));
    }

    public bool AdjecentTo(Knot other)
    {
        var cdist = Math.Abs(other.c - c);
        var rdist = Math.Abs(other.r - r);
        if (cdist <= 1 && rdist <= 1) return true;
        return false;
    }

    public void Move(string move, List<Knot> otherKnots)
    {
        char dir = move[0];
        int length = move.Substring(1).Trim().ToInt()!.Value;
        for (int i = 0; i < length; ++i)
        {

            if (dir == 'R') c++;
            else if (dir == 'L') c--;
            else if (dir == 'U') r++;
            else if (dir == 'D') r--;

            history.Add((r, c));
            Knot last = this;
            foreach (var nextKnot in otherKnots)
            {
                nextKnot.Follow(last);
                last=nextKnot;
            }
        }
    }
    public void Follow(Knot other)
    {
        if (AdjecentTo(other)) return;

        if (other.r != r)
            r += other.r < r ? -1 : 1;
        if (other.c != c)
            c += other.c < c ? -1 : 1;


        history.Add((r, c));
    }
}
