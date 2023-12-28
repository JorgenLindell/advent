using System.Diagnostics;
using common;


namespace _6
{
    internal class Program
    {
        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        static readonly string xinput = @"
Time:      7  15   30
Distance:  9  40  200
";

        static void Main(string[] args)
        {
            List<string> matrix = input.Split("\r\n", Tidy).ToList();
            //var times = matrix[0].Split(' ', Tidy).Skip(1).Select(x => x.ToLong() ?? 0).ToList();
            //var distances = matrix[1].Split(' ', Tidy).Skip(1).Select(x => x.ToLong() ?? 0).ToList();
            var times = matrix[0].Replace(" ", "").Split(':', Tidy).Skip(1).Select(x => x.ToLong() ?? 0).ToList();
            var distances = matrix[1].Replace(" ", "").Split(':', Tidy).Skip(1).Select(x => x.ToLong() ?? 0).ToList();

            var totProd = 1L;
            times.ForEach((tTot, ix) =>
            {
                //var results = new List<(long, long)>();
                //for (int load = 0; load <= tTot; ++load)
                //{
                //    var dist = CalcDist(tTot, load);
                //    results.Add((load, dist));
                //}

                var better = Better(times[ix], distances[ix]);
                Debug.Write($"{times[ix]} {distances[ix]} cnt:{better}: ");
                // results.ForEach(x => Debug.Write($"{x.Item1}({x.Item2})  "));
                Debug.WriteLine("");
                totProd *= better;
            });

            Debug.WriteLine($" sum:{totProd}");
        }

        private static long Better(long time, long distance)
        {
            var start = Task.Run(() =>
            {
                var start = time / 2;
                while (CalcDist(time, start) > distance) start -= 1;
                return start;
            });
            var end = Task.Run(() =>
            {
                var end = time / 2;
                while (CalcDist(time, end) > distance) end += 1;
                return end;
            });
            Task.WhenAll(start, end);
            return end.Result - start.Result - 1;
        }

        private static long CalcDist(long tTot, long load)
        {
            var timeLeft = tTot - load;
            var dist1 = timeLeft * load;
            return dist1;
        }

        private static readonly string input =
            @"
Time:        61     67     75     71
Distance:   430   1036   1307   1150
"; 
    }

}

