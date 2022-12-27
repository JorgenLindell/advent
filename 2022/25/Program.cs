using System;
using System.Diagnostics;
using System.Numerics;
using System.Transactions;
using common;


//https://adventofcode.com/2022/day/25
internal class Program
{
    private static readonly string _testData =
        @"1=-0-2
 12111
  2=0=
    21
  2=01
   111
 20012
   112
 1=-1=
  1-12
    12
    1=
   122"
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
        var numchars = "012=-";
        var lines = Load(getStream());
        BigInteger sum = new BigInteger();
        lines.ForEach(s =>
        {
            Console.Write($"{s,9} ");
            var n = SnafuToBig(s);
            Console.Write($"{n} {((long) n).LongToBase(5)} ");
            var s2 = BigToSnafu(n);
            Console.WriteLine($"{s2} ");
            sum += n;
        });
        Debug.WriteLine("Sum lines=" + sum +" "+BigToSnafu(sum));
    }



    private static BigInteger SnafuToBig(string s)
    {
        var figures = s.Trim().ToCharArray();
        BigInteger n = 0L;
        BigInteger exp = 1;
        int bas = 5;
        for (int i = 0; i < figures.Length; i++)
        {
            int indexFromEnd = figures.Length - i - 1;
            n += figures[indexFromEnd] switch
            {
                '0' => 0,
                '1' => 1L * exp,
                '2' => 2L * exp,
                '-' => -1L * exp,
                '=' => -2L * exp,
                _ => throw new InvalidDataException("Unknown char")
            };
            exp *= bas;
        }

        return n;
    }

    private static string BigToSnafu(BigInteger n)
    {
        var figures = "";
        BigInteger bas = 5;

        var chars = new Dictionary<long, (long value, char c)>()
        {
            {0,(0L , '0') },
            {1,(0L , '1') },
            {2,(0L , '2') },
            {3,(-2L, '=')  },
            {4,(-1L, '-')  },
        };

        BigInteger current = n;
        while (current > 0)
        {
            int remainder = (int)(current % bas);
            figures = $"{chars[remainder].c}{figures}";
            current -= chars[remainder].value;
            current /=bas;
        }

        return figures;
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

