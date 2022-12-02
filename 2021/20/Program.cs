using System.Diagnostics;
using common;

namespace _20
{
    class Program
    {

        static void Main(string[] args)
        {
            var data = LoadData("input.txt");
            //var data = LoadData();
            var (enhancer, image) = data;
            image.PrependRows(3, 0);
            image.PrependColumns(3, 0);
            var imageRows = image.Rows + 3;
            var imageColumns = image.Columns + 3;
            for (int i = 0; i < imageRows; i++)
            {
                for (int c = image.Columns; c < imageColumns; c++)
                {
                    image.Set((i, c), 0);
                }
            }
            Console.WriteLine($"Size at start: {image.Rows}  {image.Columns}");
            Console.WriteLine(image.ToString((c, v) => v == 0 ? "." : "#"));
            var outsideValueOdd = enhancer[511];
            var outsideValueEven = enhancer[0];

            for (int iterations = 1; iterations < 51; iterations++)
            {
                image.OutsideFunc = c => ((iterations & 1) == 1 ? outsideValueOdd : outsideValueEven);
                var imageOutsidevalue = image.OutsideFunc((-1, -1));
                image.PrependRows(1, imageOutsidevalue);
                image.PrependColumns(1, imageOutsidevalue);
                imageColumns = image.Columns;
                for (int r = 0; r < image.Rows; r++) image.Set((r, imageColumns), imageOutsidevalue);
                imageRows = image.Rows;
                for (int c = 0; c < image.Columns; c++) image.Set((imageRows, c), imageOutsidevalue);

                Console.WriteLine($"Input for:{iterations}\n{image.ToString((c, v) => v == 0 ? "." : "#", 3)}");


                var updates = new List<((int r, int c), int changeTo)>();
                for (int r = 0; r < image.Rows; r++)
                {
                    for (int c = 0; c < image.Columns; c++)
                    {
                        var bstr = data.ExtractPoints(r, c, 3);
                        var index = Convert.ToUInt64(bstr, 2);
                        var changeTo = enhancer[index];
                        var value = image.Value((r, c));
                        {
                            Debug.WriteLine($"{bstr} {index} {(r, c)} {value} => {changeTo}");
                            updates.Add(((r, c), changeTo));
                        }
                    }
                }


                updates.ForEach(
                    u =>
                    {
                        var (cell, changeTo) = u;
                        image.Set(cell, changeTo);
                    });
                Console.WriteLine("\n\n------------------------------ Iteration: " + iterations);
                Console.WriteLine(image.ToString((c, v) => v == 0 ? "." : "#", 0));
                var ones = image.Cells.Count(cell => cell.Value == 1);
                Console.WriteLine("Number of ones: " + ones);
                Console.WriteLine($"Size: {image.Rows}  {image.Columns}");
            }
            image.OutsideFunc = c => (outsideValueOdd);

            Console.WriteLine(image.ToString((c, v) => v == 0 ? "." : "#", 3));
            var ett = image.AllCells.Count(cell => cell.Value == 1);
            Console.WriteLine("Number of ones: " + ett);
        }

        static ImageData LoadData(string file = "")
        {
            var data = new ImageData();
            TextReader reader;
            if (file == "")
            {
                reader = new StringReader(testData);
            }
            else
            {
                Console.WriteLine("Loading from file " + file);
                reader = File.OpenText(file);
            }

            var enhancer = "";
            var line = "";
            while (reader.Peek() != -1 & (line = reader.ReadLine())?.Length > 2)
            {
                enhancer += line.ReplaceLineEndings("");
            }
            data.Enhancer = enhancer.Select(c => (c == '#' ? 1 : 0)).ToArray();

            var r = 0;
            while ((line = reader.ReadLine())?.Length > 2)
            {
                var chars = line.ReplaceLineEndings("")
                    .ToList();
                chars
                    .ForEach((b, c) =>
                    {
                        var value = (b == '#' ? 1 : 0);
                        data.Image.Set((r, c), value);
                    });
                r++;
                if (reader.Peek() == -1) break;
            }

            return data;
        }


        private static string testData =
@"..#.#..#####.#.#.#.###.##.....###.##.#..###.####..#####..#....#..#..##..##
#..######.###...####..#..#####..##..#.#####...##.#.#..#.##..#.#......#.###
.######.###.####...#.##.##..#..#..#####.....#.#....###..#.##......#.....#.
.#..#..##..#...##.######.####.####.#.#...#.......#..#.#.#...####.##.#.....
.#..#...##.#.##..#...##.#.##..###.#......#.#.......#.#.#.####.###.##...#..
...####.#..#..#.##.#....##..#.####....##...##..#...#......#.#.......#.....
..##..####..#...#.#.#...##..#.#..###..#####........#..####......#..#

#..#.
#....
##..#
..#..
..###
";
    }
}
