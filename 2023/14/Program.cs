using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using common;
using common.SparseMatrix;

namespace _13
{

    internal class Program
    {

        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        static readonly string xinput = @"
O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....";


        static async Task Main(string[] args)
        {
            var strings = input.Trim("\r\n".ToCharArray()).Split("\r\n", StringSplitOptions.TrimEntries);
            var panel = strings.Select((x, i) => x.ToCharArray()).ToArray();
            var stones = panel.Select((row, rix) =>
                    row.Select((cell, cix) => (cell, rix, cix))
                        .Where(x => x.cell == 'O')
                        .Select(x => (x.rix, x.cix)))
                .SelectMany(x => x)
                .OrderBy(x => x.rix).ThenBy(x => x.cix)
                .ToList();
            Dump(panel);
            var sum = 0;

            for (int i = 0; i < 1000; i++)
            {

                sum = TiltPanel(panel, ref stones);
                //   Debug.WriteLine("North :" + sum);
                //   Dump(panel);
                //
                panel = Rotate90Clockwise(panel, out stones);
                sum = TiltPanel(panel, ref stones);
                //   Debug.WriteLine("West :" + sum);
                //   Dump(panel);
                //
                panel = Rotate90Clockwise(panel, out stones);
                sum = TiltPanel(panel, ref stones);
                //   Debug.WriteLine("South :" + sum);
                //   Dump(panel);
                //
                panel = Rotate90Clockwise(panel, out stones);
                sum = TiltPanel(panel, ref stones);
                //   Debug.WriteLine("East :" + sum);
                //
                //   Dump(panel);
                panel = Rotate90Clockwise(panel, out stones); // now north directed but not tilted

                sum = WeighNorth(panel, stones);

                Debug.WriteLine($"{i:D5} {i % 34} after Cycle:{i + 1}  >North :{sum}");
             //   Dump(panel);
             //   Debug.WriteLine("-------------------------");
            }
            Debug.WriteLine($"{(1000000000-1) % 34} after Cycle:{1000000000}  >North :{sum}");

        }

        private static int TiltPanel(char[][] panel, ref List<(int rix, int cix)> stones)
        {
            var sum = 0;
            for (int i = 0; i < panel.Length; i++)
            {
                sum += TiltAndWeighColumn(panel, i, ref stones);
            }

            return sum;
        }

        private static void Dump(char[][] panel)
        {
            var s = new StringBuilder();
            foreach (var t in panel)
            {
                for (int c = 0; c < panel.Length; c++)
                {
                    s.Append(t[c]);
                }
                s.Append("\r\n");
            }
            Debug.WriteLine(s.ToString());
        }
        private static char[][] Rotate90Clockwise(char[][] matrix, out List<(int rix, int cix)> stones)
        {
            int w = matrix.Length;
            var ret = matrix.Select((x, c) =>
                Enumerable.Range(0, w).Select(x => matrix[x][c]).Reverse().ToArray()).ToArray();
            for (var i = 0; i < matrix.Length; i++)
            {
                matrix[i] = ret[i];
            }
            stones = matrix.Select((row, rix) =>
                    row.Select((cell, cix) => (cell, rix, cix))
                        .Where(x => x.cell == 'O')
                        .Select(x => (x.rix, x.cix)))
                .SelectMany(x => x)
                .OrderBy(x => x.rix).ThenBy(x => x.cix)
                .ToList();
            return ret;
        }
        private static int TiltAndWeighColumn(char[][] panel, int column, ref List<(int rix, int cix)> stones)
        {
            var height = panel.Length;
            var totWeight = 0;
            var colStones = stones.Where(x => x.cix == column).ToList();

            foreach (var stone in colStones)
            {
                var weight = height - stone.rix;
                for (int j = stone.rix - 1; j >= 0; j--)
                {
                    if (panel[j][column] == '.')
                    {
                        panel[j][column] = panel[j + 1][column];
                        panel[j + 1][column] = '.';
                        weight = height - j;
                    }
                    else
                    {
                        break;
                    }
                }
                totWeight += weight;
            }

            return totWeight;
        }

        private static int WeighNorth(char[][] panel, List<(int rix, int cix)> stones)
        {
            var height = panel.Length;
            var totWeight = 0;

            foreach (var stone in stones)
            {
                var weight = height - stone.rix;
                totWeight += weight;
            }

            return totWeight;
        }


        private static readonly string input =
            @"
#...O.#O...#...#.OOO..O.###O......O#.O.......O..#.....#..O...O.##...O......O...#.#.O.#.#.#...#......
.#OO..#.#.O.#..O....#O..O##.OO...#..#O...OO....O....#O.......O....##.....#O....#.#..OO#O..O#......OO
O..##..#...O.O..O.#..#.O....O....#...#.......O..#......#O..#.#....O#.O....#..O#.O.#O.O..O....#.....O
O.O.#....#...OO...#..O..O#OO.O...O.........O..##O#..O#...OO.....O....O...O.O..#.O#....O.....O....#..
#.#..O...#.#..##.O.OO.O.......O..#.O...#.O.#O#..OOO....O....#....#O..####.O...O.........O......#...#
.O.O#.OO..##...O.....O..#O.##O.#.OOO.#.#O..#.O.#O.#OOO......O...#.......#...##..#...O....#OO#O.O.#..
..#.#.......O...#.#.O####.........OO.....O.OO.O..#..#..O.O..O....OOO..###.#....O##......OO..........
...O.O.O..#O.....O.O.O.......#.....O##......O.O....O.....#.###...O#...#O..#....#.O..O...#...#.##..O#
..O.O...#OO#..OO..O..OO#..#.O...#...O.#.......O#.##.O#O.OO.......#.#.OO#O.O...#.O.O.....O.O#.O.#O.O.
.....O.##O#...O.O.............OO.......#O.#OO.O....O..O#...O.#.#.#..#.O#O.O.....##O.O...O.......O.#.
...#.#O#....O.O..#....OO.OO...##.#..O.OO.OO..O.OO#.O...O.#.#...OO....O....##...#O....#.OOOO..O..OOO.
....#.O.O#OO....#.#O..#..O..OOO#OOO..#.OO.O..O.O....#..O.##O..#...O...O....#O...OO...........O..O...
.OO...##.O.....O.#....O.....OO..#.......#O##..O##.#O..........OO.OO........O...O..#..#..#O.OO...#O..
..O#O#..#.#...#OO..O...#.O..#..OOOO####....O.O..O........OO..OO...##...#....#..#.OO..#...O....O.O...
##...#O.......O.O##....O.OO#...........O#.....O##....O#.O#O......O#.#......##...OO#....O...O.OO.O#.#
.....O..........O.O..OO......#....O##..#......O.#.#..O.#.#...O.#.O#.O...O#.OO...#.#.O..O#.#.O.#....#
....O.O......#.OO#...........#......#.O.O..O#O#.#.O.##.#.#O.O#OO...O...##......#.#..#..#..O....O#O#.
##.O.OO..#O.#O.....O..O.O..O....O...#.#O.O.#O#.#O##OO..O...O.O......O#O...O#...#.#......##..O..O.#O.
...O...O..OO..#.O..OO.....#..O......O.....#..O....#.O..#..O.O#.#.O......O.OO...O..O..O......O.#...O.
.#..##...OO..#..#...#O..O.....#O#....#.....#.O.....O.....##O...O.#..#OO.......#...#..........##....#
...O........OO.#.O...#O..OO..O#.........##..#O...O....#...O..#..........#...#.....#.O.O......#...OO.
##O......O#.O...O.##..O......OO.O#O.#...O.O#.##.#.#.O.#..#..........O..##.O.........O.O...#....O...O
#.#..O.#.##OO......OO...O.OO...#.O.OO..O.#....O........O..#...O.##...##.#.O..O.O#..OO#...O...#...#O.
.#O..O..#..O..O.#OO#....##....O.##..###.#..O#.#..#........OO#..O.#..#........O....O#.O.O.......#..##
...#.........O#O.#O..#...#......##.O.O...OO#...#....#........##...OO#.......O..OOOO...##...O#..O#O..
..#.OOO##...#.O#..#..#.OOO..#O..#....##.O...OO.#O...O..OOO#O..O.OO..#....#.#.........#...#..#.#.#..O
.O..O.O#...#O.O...O...O....O#.O..#.O#....O.O.O..#.O...O...O.O.O.....O.......O.O##.#...O.O#..........
..#..#.....O##...O.#...O#....#..OO.#.O..O...#O.#.O....#...O....O..##..###....#....OO#..O......O#....
....#O.....O.....#..O.#..OO#.....#O...O.OO##..#.....O..#...O...O..........O....OO.O..O..O..##.......
.###O..O.........O..##.#......O#O......O..#.....##.......O.....#..#.#O#O##..#.#....OO....O#.###.O.#.
OO#....#........O..##O..#.OO.........O...O......OO.O#OO.O#O..O#O..#..##O.#.OO#...O.....O##....O....#
#...........O..#OO..#.##O...O##.O.......O#.O......O###O#..#OO##....O.OO#.......O........#O.....#....
O#O...#.##.O....O....OO....O.O.O#.O#O....O#......OO...O.....#............O.OO#...O...#.O.O.O#....O..
...O.#.O..O..O..O.#O..O#.#..O..O.....#.#.#..O.OO.O#O.O#.............O.#.#.....#OOO.O...O#.#...##.O..
#.....O#.....#...O.#..OOO.#..#OO....O....#.O...........#..OO...##.O..#O...#.##.....O....#..........O
..O..#...O..O#O.O.#.#O#.#..OO..##.#O.#.#..#.........O...#.O.#.##.##.O...#..##...O......O....#..O....
##.......#......O.O.#..#O...#.....#.#.#.O.......#....#O.O##O.......O..#..#.......O.....O...#.#...#..
O#O#..#O.....#..##O#..O.#.....O#...O.#...O#OOO.#O.O.#.#....#.O##OO....#........#.O.O.O.#.#O.......O.
O.O..O#O......OO...O..#OO..#......O.......#.O....#..#.#O###OO.O...#...O.#.O.OO.O.####.#..#.#..O..O.#
........O.#..##.O.###OO#.#O.O##.....O##...O.....#........##.O.##.O........O.#....#.#...OO.O.O...O##O
..#........O.#..OO##O.......#...#.O.O.OO##...#........#..##.#.O.O..O.....O....O.O#O..O..O.O.#.O.....
..O#..OO.O...#.##..#.#..O...O...O#.#O.#...#O...O.#...OO..##.OO.#.O.#.##...O.....#......#..O.O.....O.
..O..O..O..O.O..#...#O#.....O.##O..OO.#..O.OO#.O......O..#....O.O.O..O#.....O.#..O...O#..##.#O.O#.O.
..O#.O.OO##O##..O.##O...#OO#.....O...O.OO....OO###.OO...#O#O..##.O.O...#O.#.OOO.O..#.#O.#.O..#....O.
.....O....#OOO..O#.O#.#.O....O..OOO.............O#...O.....OO.#OOO#....O...O...#...O.##.##O#O.......
#...#.#O.....O#OO#......#.O#O..OO....O.....OO.#......#...#....#O.O#O...OO..#....O.....O#....O..#OO#.
OO.O...O.#.....#O....#.#......#.#.#..#.#.O....O##O.O..#....#O...#.##.#O..#..O...#....#..O.##O.#..O.#
##.#.#...O#O..#....#.##.O.O.O...#.OO..#....OOOO..#...OO.......O....O....OO..OO.....O...#OO...#O.....
...O##...OO..........O..O..O.#...OOO.OOO..O.O.......O.O.O#..O....O#...O.#OO#.#.#.#.....#...#.O..#O.O
#...O..OO........#...#.O......#.OO...O.O.#....O..#.O..#......#...O#...........O..##O..O......O#.#...
O....O#..#O.O.............#....O..O.O.#O.O...O...#....O.#.#...O..#....O.........OO....O.O#OO.....O..
...O..O..O.OO#.#O.....O.O#.O.#..##...#.#O...#.OOOOOO..#..O#.#O#.O.#.O..#.O.O#..##...O.......#O#O..OO
#..#.....O.#O#.O.......#.O#.O.#....#.O.....OO..O.#O#..#O...O.........#O.O#.#......#O....O.O.OO.#O...
.OO#.O.##.O.#.......###...O........#....O.O#.OO#.....#...#...O.##....O.#..O...O...O..OO....O#..O..O.
#O.O............O.O....#OO#..O.......O.....OO.............O#.O#...OO.......#.O.##...#......O.#......
#..O..#...........O..#.O#..##..O#..O..#..#...##.##OO..#O.....O.O.#..O.###.......OO......OOO...OO...O
....#OO.O....#.OO.O..##O..OO.O#.#O..O..O.#.#.#....#O.O##O.......O#......##.O#.#O#.#.O....#..##.#....
...O....#.....OO.O.#..##..O...##.#.....#.O.O...O......O#....O.........O..OOO.......O.#.#..#.....O..#
.OOO...##......O#.#..O......#.#..#O..#O#.....#...O#....O.O.O.O.#O###..#....O.#..O#O..........O......
O.#..O...O.O..#O..#....O#.O....#......O#..O.#....OO....O..O....O....#.#O.O.##O.#..O#....O#...##..OO.
.............O.........OO........O...O.OO.#.OOO...O..O#.......##....#O...OO....#.O.O...OOOO...O.#..#
...O.#O....#.......#.O.O##O............OOO...#..#..##...OO##..#....O...#O##..OOO..O..#..O#......O...
O...O...#..##......#.O.O..#....OOO.O..OO...##...O.O.......#.#.#.....##...O..#O.#...O.........OO....#
O#O#.....O..#..O#.O...###...O.OO.#O.O#.O.#...#........#O.O........O.#..O#........#....#O...O#O#.....
...#.O....#..O....#O.....O...O.#.##O.O#OO#.O.O.##O.......##......O#..#..#.....OO......#O.#O...OO.###
......OO#OO..O#O.....O.#.O#.O.#O....O....#.O..#...#.#....#.OO...O......#..O.#O#.#.....O.O.O.#....O..
#O....O.O....O.O....#....##...#OO.O##...O....##........#.#..O................O#O.O....O..#...##O.#..
###O#....#O#O.O.OO....#.........O#.#.O...O#..#.O.....#....#.O......O..O##.......#......O.....O....O#
......OO.#.#O..O...O........O#.....O...#OO##OOO....O#..O.O..........O....O.....OO.O..OOO.O....O.O#..
O..#..O#O.....OO##..O#...OO.....##O#O.O..O...#OOOOO.OO#.O...O....O#O#.##....#O....O......O....OO.##.
..O.....OO#O...........O...#...O.O#O.......OOO.....#.O....#.#....O..#.#.............O...O.....OOOO.O
...OO..O.....#..#.O.O..#.O..OOOOO....O.#...#..#.OO......O#.#O.##..##............O....O.O.O.....O...#
#.....O....#O..#..O..#..#.OO#.#.O.#...#.....O..#.......OO#O.#......#..O.O...O.#.#...O##O.....#OO#...
.O.#...O.OO...#...........#..##..#...O.#....O.#...#....OO..#.....#...##..#...#OO.......OOOO..O....#.
..#.##..O.O.....O#.......#..#........#.###.O...#.O#O#OO.OO.O#..#.O..#.OO#..#OO....#O..#.........#OO.
..#........O#...O....O..O..##..###O..................O......O.#.O.O.#OOO.#OO....#..O#......OO.OOO...
..#O.#O..O..OO#..#..#.OO.OOO.#.OOOO..O#...O#...O....#O........#............O.O....#...OO.O..#.OO....
.OO#....#.#O.#O#O.#.##......#...#..O.O.##......#.#..#...#............#O....##.OO.O.O........#OO.#.#.
..##.......#.O#...OO......##.OO...O.........#........O...........#O.O##OO...........#.O.O..OO.......
OO##O#.....#.#O..#......#....#OO.#O....###.#.....##.O.O#..O....#O.#.....##.##..O#.O.....#.....#O.#..
##...O#.OO.O..O.O.O#...#.#..#.#.O..#O.....O#.........O#.........O...#...#.O......O..#O..#O..O.#O.O.O
..O......O#...O......OOO..O.....OO.....##.....#O.........O.......#......O#...O.#..O.......#....#....
O..#.O..#.#.......#O.#.##O..O#.O##....#.#.OO..OO....#O..O..O....#...O.#....#.#.O#......O.#......O...
.O..##.#O.O...#O...#.O.O.#...O..OO..#O...#OO......O...O.....OO........#.O...#..O#.O.#.O..O...O#....O
..O....#.##...O##.O#..#.O.O.#..OOOO.....#OO.O....#...#O....O........#..O...#O#..O#.#..O#...OOO#.O...
.O#...O#....#..O...#..O.O.#..#.#........#.OO.O.O.#................##....#.....#...O.#...#.....O.#...
..O.........##.O..O.....OO##..OO#O.#...#O##....#...#.......##.......O#......O#..O#..O.##....O..#....
.O.O..#O......#..#..##....O..O#O..O#...O.##...O..O..#.O.OO....O.##....##.OO..O.........O#.#...#.##..
......O...O.#..OO#......#.....OO.O.##.#...#O.O#....O....#.O............O........#.OO#......OO##..O..
O#O..O...O.O....OO.....##O.....OOO..OO.....#O...OO.#....#.O.......#.#.........#O..#..O##.#.O.#.OO.#.
#O#O..##.##O.....O.O.#..O....#.O#.......O......#O..O.O...O.O..O...#OO.....##..#.....O.#..#O..O.O.#..
.O.##...#....#..#O...##O...##..#.O..O.#..#O...O.O........#.O...O...O#.OO..O.##..O#.O.O#..#OO##OO...O
.##..#..O....OO......#OO..#.....OO.O..#.O..O.O#..#.O#........O....O..O..#....O..#...##.#OOO...#OO#OO
#.#O...#...OOO#.....#.#O.##...##O...#....O......O....#O.#.O......#.....O....O....O.O.#..#O...##O....
......#..OOOO.....O....#.#.##O.#..#..O.#........O....#....#...OO....#..O.......O...OO#....#.#O..#..#
###..O...#...O#.OO.#.#O..O.#.#..##.##OOO.O....#O.#.#....O.#O..O..#O...OO.O.O.O.O#..O......O.#.#..OO.
.#O....###.......O#..#O#O...#....O.OO#..O.#O.O#.O.##..#....#...OO..#.O###..O...........##.O...##.#.O
...OO.#.#..O#.....##OO.......O..##..OO.#..........#.OO..OO..#.#O....O.#..O...O......O....O.O#...O...
..O....#.O#..O..#.....O..O#..#OO......#...OO.O#OO..#O#..O#.......#...O.#.OO.##.O#O....O..#..#O..O.#.
...#O....OOO.#OOO.....O..##..OOOO..O..#..#.#...O..#.O..........#OOO.#.O.....OOO.O..#..O..O.#.....#.#";
    }
}

