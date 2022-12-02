using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _10
{
    internal class Program
    {
        public enum ResultStatus
        {
            OK,
            Broken,
            Incomplete
        }


        private static readonly string testData =
          @"[({(<(())[]>[[{[]{<()<>>
[(()[<>])]({[<{<<[]>>(
{([(<{}[<>[]}>{[]{[(<()>
(((({<>}<{<{<>}{[]{[]{}
[[<[([]))<([[{}[[()]]]
[{[{({}]{}}([{[{{{}}([]
{<[[]]>}<{[{[{[]{()[[[]
[<(<(<(<{}))><([]([]()
<{([([[(<>()){}]>(<<{{
<{([{{}}[<[[[<>{}]]]>[]]
";

        private static readonly List<string> inputLines = new List<string>();

        private static void Main(string[] args)
        {
            var stream = StreamUtils.GetInputStream(file: "input.txt");
            //  var stream = StreamUtils.GetInputStream(testData: testData);
            LoadStream(stream);
            var credits = 0;
            var complScores = new List<ulong>();

            var CheckedLines = new List<string>();
            for (var r = 0; r < inputLines.Count; r++)
            {
                var inputLine = inputLines[r];
                var res = Analyse(inputLine);
                if (res.Status == ResultStatus.OK)
                {
                    CheckedLines.Add(inputLine);
                }
                else if (res.Status == ResultStatus.Incomplete)
                {
                    ulong complScore = 0;
                    var completionString = "";
                    while (res.Completion.Count > 0 && res.Completion.Peek().End > 0)
                    {
                        var par = res.Completion.Pop();
                        completionString += par.End;
                        complScore *= 5;
                        complScore += (ulong)par.CompletionScore;
                    }
                    complScores.Add(complScore);
                    Console.WriteLine($"Line{r,4:##0}: {res.Message} Completion= {completionString} {complScore}");
                    CheckedLines.Add(inputLine + completionString);
                }
                else if (res.Status == ResultStatus.Broken)
                {
                    Console.WriteLine($"Line{r,4:##0}: {res.Message} ");
                    credits += res.BadParens.Score;
                }


            }
            complScores.Sort();
            var mid = (complScores.Count / 2);
            Console.WriteLine($"Total credits: {credits} ");
            Console.WriteLine($"Middle Score: {complScores[mid]} ");
        }

        private static LineStatus Analyse(string inputLine)
        {
            var stack = new Stack<Parens>();
            var expected = Parens.None;
            var pos = 0;

            // Console.WriteLine($"Analysing:{inputLine}");
            var tokenList = inputLine.ToList();
            foreach (var token in tokenList)
            {
                // Console.WriteLine("".PadLeft(stack.Count)+token);
                ++pos;
                if (token.In(Parens.Starters))
                {
                    stack.Push(expected);

                    var par = Parens.All.First(x => x.Start == token);
                    expected = par;
                }
                else if (token.In(Parens.Stoppers))
                {
                    var expectedEnd = expected.End;
                    if (token != expectedEnd)
                    {
                        //broken
                        return new LineStatus
                        {
                            Status = ResultStatus.Broken,
                            Message = $"Broken line, expected {expected.End}, found {token} in pos {pos}",
                            Expected = expected,
                            BadParens = Parens.All.First(x => x.End == token)
                        };
                    }
                    expected = stack.Pop();
                }
            }

            if (stack.Count > 0)
            {
                stack.Push(expected);
                //incomplete
                return new LineStatus
                {
                    Status = ResultStatus.Incomplete,
                    Message = "Incomplete line, ended prematurely. ",
                    Expected = expected,
                    BadParens = Parens.None,
                    Completion = stack
                };
            }

            //complete
            return new LineStatus
            {
                Status = ResultStatus.OK,
                Message = "Line OK",
                Expected = Parens.None,
                BadParens = Parens.None
            };
        }

        private static void LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            var r = 0;
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                Console.WriteLine(inputLine);
                inputLines.Add(inputLine);

                inputLine = stream.ReadLine();
                ++r;
            }
        }

        public class LineStatus
        {
            public Parens BadParens;
            public string Message;
            public int Pos;
            public ResultStatus Status;
            public Parens Expected { get; set; }
            public Stack<Parens> Completion { get; set; }
        }
    }
}