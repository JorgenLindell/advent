using System;
using System.Collections.Generic;

namespace common
{
    public static class LineExtensions
    {
        public static IEnumerable<(int r, int c)> Cells(this ((int r, int c) from, (int r, int c) to) line)
        {
            if (line.from.r == line.to.r)
            {
                var start = Math.Min(line.from.c, line.to.c);
                var end = Math.Max(line.from.c, line.to.c);
                for (int c = start; c <= end; c++)
                {
                    yield return (line.from.r, c);
                }
                yield break;
            }

            if (line.from.c == line.to.c)
            {
                var start = Math.Min(line.from.r, line.to.r);
                var end = Math.Max(line.from.r, line.to.r);
                for (int r = start; r <= end; r++)
                {
                    yield return (r, line.from.c);
                }
                yield break;
            }

            if (Math.Abs(line.from.c - line.to.c) == Math.Abs(line.from.r - line.to.r))
            {
                var incrR = line.from.r > line.to.r ? -1 : 1;
                var incrC = line.from.c > line.to.c ? -1 : 1;
                int c = line.from.c;
                for (int r = line.from.r; r != line.to.r + incrR; r += incrR)
                {
                    yield return (r, c);
                    c += incrC;
                }
                yield break;
            }
        }
    }
}