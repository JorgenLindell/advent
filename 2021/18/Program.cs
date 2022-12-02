using common;

namespace _18;

internal class Program
{


    private static void Main()
    {
        var stream = StreamUtils.GetInputStream(file: "input.txt");
        TestSimpleReduce();
        TestSimpleAdd();
        TestComplexAdd();
        TestMagnitude();
        TestHomeWork();
        TestHomeWorkSum();
        SolveA(stream);
        SolveB(stream);
    }

    private static void SolveA(TextReader stream)
    {
        var sum = SnafuNumber.Parse(stream.ReadLine());
        while (stream.Peek() != -1)
        {
            sum += SnafuNumber.Parse(stream.ReadLine());
        }

        Console.WriteLine(sum.ToString());
        Console.WriteLine("OK " + sum.Magnitude);
    }
    private static string homeworkexample = @"[[[0,[5,8]],[[1,7],[9,6]]],[[4,[1,2]],[[1,4],2]]]
[[[5,[2,8]],4],[5,[[9,9],0]]]
[6,[[[6,2],[5,6]],[[7,6],[4,7]]]]
[[[6,[0,7]],[0,9]],[4,[9,[9,0]]]]
[[[7,[6,4]],[3,[1,3]]],[[[5,5],1],9]]
[[6,[[7,3],[3,2]]],[[[3,8],[5,7]],4]]
[[[[5,4],[7,7]],8],[[8,3],8]]
[[9,3],[[9,9],[6,[4,9]]]]
[[2,[[7,7],7]],[[5,8],[[9,3],[0,2]]]]
[[[[5,2],5],[8,[3,7]]],[[5,[7,5]],[4,4]]]";
    private static void TestHomeWorkSum()
    {
        var stream = StreamUtils.GetInputStream(testData: homeworkexample);
        SolveB(stream);
    }

    private static void SolveB(TextReader stream)
    {
        var list = new List<SnafuNumber>();
        while (stream.Peek() != -1)
        {
            var number = SnafuNumber.Parse(stream.ReadLine());
            list.Add(number);
            Console.WriteLine($"{number}   {number.Magnitude}");
        }

        var results = list.SelectMany(
            l1 => list.Select(
                l2 =>
                {
                    var sum = l1.Clone().Add(l2.Clone());
                    return new { n1 = l1, n2 = l2, s = sum, m = sum.Magnitude };
                })).OrderByDescending(x => x.m).ToList();
        results.Take(20).ToList().ForEach(
            x => Console.WriteLine($"{x.m} {x.n1} + {x.n2} = {x.s} "));
    }

    private static void TestHomeWork()
    {

        var stream = StreamUtils.GetInputStream(testData: homeworkexample);
        var sum = SnafuNumber.Parse(stream.ReadLine());
        while (stream.Peek() != -1)
        {
            sum += SnafuNumber.Parse(stream.ReadLine());
        }
        CompareEqual(sum, "[[[[6,6],[7,6]],[[7,7],[7,0]]],[[[7,7],[7,7]],[[7,8],[9,9]]]]", "Wrong sum", "Test sum Succeded");
        if (sum.Magnitude == 4140)
        {
            Console.WriteLine("OK " + 4140);
        }
        else
        {
            Console.WriteLine("fail " + sum.Magnitude + " " + 4140);
        }

        var sum2 = SnafuNumber.Parse("[[2,[[7,7],7]],[[5,8],[[9,3],[0,2]]]]") +
                   SnafuNumber.Parse("[[[0,[5,8]],[[1,7],[9,6]]],[[4,[1,2]],[[1,4],2]]]");

        CompareEqual(sum2, "[[[[7,8],[6,6]],[[6,0],[7,7]]],[[[7,8],[8,8]],[[7,9],[0,6]]]]", "Wrong sum", "Test sum Succeded");
        Console.WriteLine("Magnitude=" + sum2.Magnitude);

    }

    private static void TestMagnitude()
    {
        ValueTuple<string, int>[] _testData =
        {
            ("[[1,2],[[3,4],5]]                                    ", 143),
            ("[[[[0,7],4],[[7,8],[6,0]]],[8,1]]                    ", 1384),
            ("[[[[1,1],[2,2]],[3,3]],[4,4]]                        ", 445),
            ("[[[[3,0],[5,3]],[4,4]],[5,5]]                        ", 791),
            ("[[[[5,0],[7,4]],[5,5]],[6,6]]                        ", 1137),
            ("[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]", 3488),
        };

        foreach (var valueTuple in _testData)
        {
            var snafu = SnafuNumber.Parse(valueTuple.Item1);
            if (snafu.Magnitude == valueTuple.Item2)
            {
                Console.WriteLine("OK " + valueTuple.Item2);
            }
            else
            {
                Console.WriteLine("fail " + snafu.Magnitude + " " + valueTuple.Item2);

            }
        }
    }

    private static void TestComplexAdd()
    {
        SnafuNumber testNumber;

        //Bigger example
        string example = @"[[[0,[4,5]],[0,0]],[[[4,5],[2,6]],[9,5]]]
[7,[[[3,7],[4,3]],[[6,3],[8,8]]]]
[[2,[[0,8],[3,4]]],[[[6,7],1],[7,[1,6]]]]
[[[[2,4],7],[6,[0,5]]],[[[6,8],[2,8]],[[2,1],[4,5]]]]
[7,[5,[[3,8],[1,4]]]]
[[2,[2,2]],[8,[8,1]]]
[2,9]
[1,[[[9,3],9],[[9,0],[0,7]]]]
[[[5,[7,4]],7],1]
[[[[4,2],2],6],[8,7]]";

        var facit = @"
[[[[4,0],[5,4]],[[7,7],[6,0]]],[[8,[7,7]],[[7,9],[5,0]]]]
[[[[6,7],[6,7]],[[7,7],[0,7]]],[[[8,7],[7,7]],[[8,8],[8,0]]]]
[[[[7,0],[7,7]],[[7,7],[7,8]]],[[[7,7],[8,8]],[[7,7],[8,7]]]]
[[[[7,7],[7,8]],[[9,5],[8,7]]],[[[6,8],[0,8]],[[9,9],[9,0]]]]
[[[[6,6],[6,6]],[[6,0],[6,7]]],[[[7,7],[8,9]],[8,[8,1]]]]
[[[[6,6],[7,7]],[[0,7],[7,7]]],[[[5,5],[5,6]],9]]
[[[[7,8],[6,7]],[[6,8],[0,8]]],[[[7,7],[5,0]],[[5,5],[5,6]]]]
[[[[7,7],[7,7]],[[8,7],[8,7]]],[[[7,0],[7,7]],9]]
[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]
".ReplaceLineEndings().Split('\n');
        var j = 1;
        var stream = StreamUtils.GetInputStream(testData: example);
        var readLine = stream.ReadLine()!;
        testNumber = SnafuNumber.Parse(readLine);
        testNumber.ReduceAll();
        while (stream.Peek() != -1)
        {
            var line = stream.ReadLine()!;
            var term = SnafuNumber.Parse(line);
            var before = testNumber.ToString();
            var termbefore = term.ToString();
            var t2 = testNumber + term;
            var shouldBe = SnafuNumber.Parse(facit[j]);
            Console.WriteLine($"{before} + {termbefore} =");
            CompareEqual(shouldBe, t2.ToString(), "Example failed", "Example Succeded");
            testNumber = t2;
            ++j;
        }
    }

    private static void TestSimpleAdd()
    {
        var s = "[[[[4,3],4],4],[7,[[8,4],9]]]";
        var testNumber = SnafuNumber.Parse(s);
        CompareEqual(testNumber, s, "Parse!=ToString");
        var added = testNumber.Add((1, 1));
        CompareEqual(added, "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]", "Something failed in Add", "Add successful");
        Console.WriteLine("\nAddingTests");
        //adding
        testNumber = new SnafuNumber(1, 1);
        testNumber += (2, 2);
        testNumber += (3, 3);
        testNumber += (4, 4);
        CompareEqual(testNumber, "[[[[1,1],[2,2]],[3,3]],[4,4]]", "Something failed in Add", "Add successful");
        testNumber += (5, 5);
        CompareEqual(testNumber, "[[[[3,0],[5,3]],[4,4]],[5,5]]", "Something failed in Add", "Add successful");
        testNumber += (6, 6);
        CompareEqual(testNumber, "[[[[5,0],[7,4]],[5,5]],[6,6]]", "Something failed in Add", "Add successful");


    }

    private static void TestSimpleReduce()
    {
        ValueTuple<string, string>[] _testData =
        {
            ("[[[[[9,8],1],2],3],4]", "[[[[0,9],2],3],4]"),
            ("[7,[6,[5,[4,[3,2]]]]]", "[7,[6,[5,[7,0]]]]"),
            ("[[6,[5,[4,[3,2]]]],1]", "[[6,[5,[7,0]]],3]"),
            ("[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[7,0]]]]"),
            ("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[7,0]]]]")
        };
        for (var i = 0; i < 4; i++)
        {
            var input = _testData[i];
            var snafu = SnafuNumber.Parse(input.Item1);
            CompareEqual(snafu, input.Item1, "Parse!=ToString");
            snafu.ToTree();
            snafu.Root.ReduceAll();
            if (snafu.ToString() == input.Item2)
            {
                Console.WriteLine();
                Console.WriteLine($"Yay! {input.Item2} " + i);
                Console.WriteLine($"Yay! {snafu} " + i);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"Was       {input.Item1} " + i);
                Console.WriteLine($"Should be {input.Item2} " + i);
                Console.WriteLine($"Fail      {snafu} " + i);
            }
        }
    }

    private static void CompareEqual(SnafuNumber snafu, string input, string message, string success = "")
    {
        if (snafu.ToString() != input)
        {
            Console.WriteLine($"{input}");
            Console.WriteLine($"{snafu} {message}");
        }
        else if (success != "")
        {
            Console.WriteLine($"{input}");
            Console.WriteLine($"{snafu} {success}");

        }
    }


    private static IEnumerable<string> LoadStream(TextReader stream)
    {
        var inputLine = stream.ReadLine();
        while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
        {
            if (inputLine is { } && inputLine != "") yield return inputLine;
            inputLine = stream.ReadLine();
        }
    }
}