using System.Collections.Generic;

namespace _10
{
    public class Parens
    {
        public static Parens[] All =
        {
      new Parens('(', ')', 3,1),
      new Parens('[', ']', 57,2),
      new Parens('{', '}', 1197,3),
      new Parens('<', '>', 25137,4)
    };


        public static Parens None = new Parens((char)0, (char)0, 0, 0);
        public static List<char> Starters = new List<char>();
        public static List<char> Stoppers = new List<char>();

        static Parens()
        {

            foreach (var parens in All)
            {
                Starters.Add(parens.Start);
                Stoppers.Add(parens.End);
            }
        }
        public Parens(char start, char end, int score, int completionScore)
        {
            Start = start;
            End = end;
            Score = score;
            CompletionScore = completionScore;
        }

        public char Start { get; }
        public char End { get; }
        public int Score { get; }
        public int CompletionScore { get; }
    }
}