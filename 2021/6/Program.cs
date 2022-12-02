using System;
using System.Collections.Generic;
using System.Linq;

namespace _6
{
    class Program
    {
        private static readonly List<int> List = new List<int>()
    {
      //     3, 4, 3, 1, 2
      1, 1, 5, 2, 1, 1, 5, 5, 3, 1, 1, 1, 1, 1, 1, 3, 4, 5, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 5, 4, 5, 1, 5, 3,
      1, 3, 2, 1, 1, 1, 1, 2, 4, 1, 5, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 3, 4, 5, 1, 1, 2, 1, 1, 5, 1, 1, 4, 1, 4, 4, 2,
      4, 4, 2, 2, 1, 2, 3, 1, 1, 2, 5, 3, 1, 1, 1, 4, 1, 2, 2, 1, 4, 1, 1, 2, 5, 1, 3, 2, 5, 2, 5, 1, 1, 1, 5, 3, 1, 3,
      1, 5, 3, 3, 4, 1, 1, 4, 4, 1, 3, 3, 2, 5, 5, 1, 1, 1, 1, 3, 1, 5, 2, 1, 3, 5, 1, 4, 3, 1, 3, 1, 1, 3, 1, 1, 1, 1,
      1, 1, 5, 1, 1, 5, 5, 2, 1, 5, 1, 4, 1, 1, 5, 1, 1, 1, 5, 5, 5, 1, 4, 5, 1, 3, 1, 2, 5, 1, 1, 1, 5, 1, 1, 4, 1, 1,
      2, 3, 1, 3, 4, 1, 2, 1, 4, 3, 1, 2, 4, 1, 5, 1, 1, 1, 1, 1, 3, 4, 1, 1, 5, 1, 1, 3, 1, 1, 2, 1, 3, 1, 2, 1, 1, 3,
      3, 4, 5, 3, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 4, 1, 5, 1, 3, 1, 1, 2, 5, 1, 1, 4, 1, 1, 4, 4, 3, 1, 2, 1, 2, 4, 4,
      4, 1, 2, 1, 3, 2, 4, 4, 1, 1, 1, 1, 4, 1, 1, 1, 1, 1, 4, 1, 5, 4, 1, 5, 4, 1, 1, 2, 5, 5, 1, 1, 1, 5
    };
        private static readonly Dictionary<int, ulong> Dict = new Dictionary<int, ulong>()
        {
            [0] = 0,
            [1] = 0,
            [2] = 0,
            [3] = 0,
            [4] = 0,
            [5] = 0,
            [6] = 0,
            [7] = 0,
            [8] = 0,
        };
        static void Main(string[] args)
        {

            foreach (var i in List)
            {
                Dict[i]++;
            }

            Report($"Initial");


            int days = 256;
            for (int day = 0; day < days; day++)
            {
                var startOfDay = Dict.ToDictionary(x => x.Key, x => x.Value);
                var spawn = startOfDay[0];
                for (int i = 1; i < 9; i++)
                {
                    Dict[i - 1] = startOfDay[i];
                }
                Dict[8] = spawn;
                Dict[6] += spawn;
                Report($"After day:{day + 1}");
            }
        }

        private static void Report(string afterDay)
        {
            Console.Write($"{afterDay,18}");
            ulong sum = 0;
            foreach (var val in Dict.Values)
            {
                sum += val;
            }
            Console.Write($"{sum,20:D} : ");
            foreach (var d in Dict)
            {
                Console.Write($"[{d.Key}]={d.Value}, ");
            }

            Console.WriteLine();

        }
    }
}
