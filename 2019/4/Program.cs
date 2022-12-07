using System.Diagnostics;
using common;

//https://adventofcode.com/2019/day/3

internal class Program
{
    private static string _testData =
        @""
.Replace("\r\n", "\n");



    private static void Main(string[] args)
    {
        FirstPart();
    }

    internal class Tracker
    {
        public char Key;
        public int Count;

        public Tracker(char key, int count)
        {
            Key = key;
            Count = count;
        }
    }
    private static void FirstPart()
    {
        var low = 128392;
        var hi = 643281;
        var matches1 = 0;
        var matches2 = 0;
        for (int i = low; i <= hi; i++)
        {
            if (Test(i, x => x > 1)) matches1++;
            if (Test(i, x => x == 2)) matches2++;
        }
        Debug.WriteLine($"Result 1: {matches1}");
        Debug.WriteLine($"Result 2: {matches2}");


        bool Test(int i, Func<int, bool> predicate)
        {
            var chars = ("" + i).ToArray();
            var list = new List<Tracker>();
            var current = new Tracker( chars[0],  1);
            list.Add(current);
            for (int j = 1; j < chars.Length; j++)
            {
                if (j > 0 && chars[j] < chars[j - 1])
                    return false;
                if (chars[j] == current.Key)
                {
                    current.Count++;
                }
                else
                {
                    current = new Tracker( chars[j],  1);
                    list.Add(current);
                }
            }

            return list.Any(x => predicate(x.Count));
        }
    }
}