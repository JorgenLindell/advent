﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using common;


namespace _9
{

    internal class Program
    {

        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        static readonly string xinput = @"
7-F7-
.FJ|7
SJLL7
|F--J
LJ.LJ
";

        public static Dictionary<char, Pipe> Pipes { get; } = (new List<Pipe>()
        {
            new('-', '═', "LR"),
            new('|', '║', "UD"),
            new('F', '╔', "DR"),
            new('7', '╗', "DL"),
            new('L', '╚', "UR"),
            new('J', '╝', "UL"),
            new('.', '.', ""),
            new('S', 'S', "RDLU"),
            new('s', 's', ""),
            new('E', 'E', "RDLU"),
            new('O', 'O', "RDLU"),
        }).ToDictionary(x => x.Key, x => x);

        internal class Pipe
        {
            public char Key { get; }
            public char Symb { get; }
            public string Exits { get; }

            public Pipe(char key, char symb, string exits)
            {
                Key = key;
                Symb = symb;
                Exits = exits;
            }
        }

        internal class Cell
        {
            public (int r, int c) Coord { get; }

            public Pipe Pipe
            {
                get => _pipe;
                set
                {
                    _pipe = value;
                    ResetMoves();
                }
            }

            private List<(int r, int c)>? _moves;
            private List<(int r, int c)>? _validMoves;
            private Pipe _pipe;

            public Cell((int r, int c) coord, Pipe pipe)
            {
                Coord = coord;
                Pipe = pipe;
            }

            public void ResetMoves()
            {
                _moves = null;
                _validMoves = null;
            }
            public List<(int r, int c)> Moves()
            {
                if (_moves != null) return _moves;
                var list = new List<(int r, int c)>();
                if (Pipe.Exits.Contains('R')) list.Add((Coord.r, Coord.c + 1));
                if (Pipe.Exits.Contains('D')) list.Add((Coord.r + 1, Coord.c));
                if (Pipe.Exits.Contains('L')) list.Add((Coord.r, Coord.c - 1));
                if (Pipe.Exits.Contains('U')) list.Add((Coord.r - 1, Coord.c));
                _moves = list;
                return list;
            }
            public List<(int r, int c)> ValidMoves()
            {
                if (_validMoves != null) return _validMoves;
                var list = Moves();
                var list2 = new List<(int r, int c)>();
                foreach (var other in list)
                {
                    if (!Cells.ContainsKey(other)) continue;
                    var otherMoves = Cells[other].Moves();
                    if (otherMoves.Contains(Coord))
                        list2.Add(other);

                }

                _validMoves = list2;
                return list2;
            }
        }

        public static Dictionary<(int r, int c), Cell>? Cells { get; set; }

        static void Main(string[] args)
        {

            List<string> matrix = input.Split("\r\n", Tidy).ToList();

            Cells = matrix
                .SelectMany((x, line) => x.ToCharArray()
                    .Select((y, col) => new Cell((line, col), Pipes[y])))
                .ToDictionary(x => x.Coord, x => x);
            var start = Cells.Values.First(x => x.Pipe.Key == 'S');

            int currentIndex;
            var toVisit = Visit(start, out var visited);

            var last = toVisit.Last();
            Debug.WriteLine("Last cell visited:" + last.coord + " at dist " + last.dist);
            foreach (var x in Cells)
            {
                if (!visited.ContainsKey(x.Key))
                {
                    x.Value.Pipe = Pipes['.'];
                    x.Value.ResetMoves();
                }
            }

            Cells = ExpandCells(Cells);

            foreach (var x in Cells)
            {
                if (x.Value.Pipe.Key == '.')
                {
                    x.Value.Pipe = Pipes['E'];
                }
                x.Value.ResetMoves();
            }

            start = Cells[(0, 0)];
            start.Pipe = Pipes['O'];
            var toVisitOutside = Visit(start, out var visitedOutside,
                (Cell c) =>
                {
                    if (c.Pipe.Key == 'E')
                        c.Pipe = Pipes['O'];
                });

            PrintMaze(Cells);
            Cells = ShrinkCells(Cells);
            PrintMaze(Cells);

            var cnt = Cells.Count(x => x.Value.Pipe.Key == 'E');
            Debug.WriteLine("Enclosed count:" + cnt);

        }

        private static Dictionary<(int r, int c), Cell> ShrinkCells(Dictionary<(int r, int c), Cell> cells)
        {
            var newCells = new Dictionary<(int r, int c), Cell>();
            var lines = cells.Keys.OrderBy(x => x.r).ThenBy(x => x.c).GroupBy(x => x.r).ToList();
            foreach (var line in lines)
            {
                foreach (var oldCoord in line)
                {
                    if (oldCoord.r % 2 == 0 && oldCoord.c % 2 == 0)
                    {

                        var newCellCoord = (r: oldCoord.r / 2, c: oldCoord.c / 2);
                        var oldCell = cells[oldCoord];
                        newCells[newCellCoord] = new Cell(newCellCoord, Pipes[oldCell.Pipe.Key]);
                    }

                }

            }

            return newCells;

        }

        private static Dictionary<(int r, int c), Cell> ExpandCells(Dictionary<(int r, int c), Cell> cells)
        {
            var newCells = new Dictionary<(int r, int c), Cell>();
            var lines = cells.Keys.OrderBy(x => x.r).ThenBy(x => x.c).GroupBy(x => x.r).ToList();
            foreach (var line in lines)
            {
                foreach (var oldCoord in line)
                {
                    var newCellCoord = (r: oldCoord.r * 2, c: oldCoord.c * 2);
                    var oldCell = cells[oldCoord];
                    newCells[newCellCoord] = new Cell(newCellCoord, Pipes[oldCell.Pipe.Key]);

                    var newCoord = (r: newCellCoord.r, c: newCellCoord.c + 1);
                    var pipe = Pipes['.'];
                    if (oldCell.Pipe.Exits.Contains('R'))
                        pipe = Pipes['-'];
                    newCells[newCoord] = new Cell(newCoord, pipe);

                    newCoord = (r: newCellCoord.r + 1, c: newCellCoord.c);
                    pipe = Pipes['.'];
                    if (oldCell.Pipe.Exits.Contains('D'))
                        pipe = Pipes['|'];
                    newCells[newCoord] = new Cell(newCoord, pipe);

                    newCoord = (r: newCellCoord.r + 1, c: newCellCoord.c + 1);
                    pipe = Pipes['.'];
                    newCells[newCoord] = new Cell(newCoord, pipe);
                }

            }

            return newCells;

        }

        private static List<((int r, int c) coord, int dist)> Visit(Cell start,
            out Dictionary<(int r, int c), int> visited, Action<Cell> func = null)
        {
            Debug.WriteLine(start.Coord);
            var toVisit = new List<((int r, int c) coord, int dist)>();
            visited = new();

            visited[start.Coord] = 0;
            toVisit.AddRange(start.ValidMoves().Select(x => (x, 1)));
            HashSet<(int r, int c)> inQueue = toVisit.Select(x => x.coord).ToHashSet();
            inQueue.Add(start.Coord);
            var currentIndex = 0;
            while (currentIndex < toVisit.Count)
            {
                var current = toVisit[currentIndex++];
                var currentCell = Cells[current.coord];
                visited[current.coord] = current.dist;
                var moves = currentCell.ValidMoves().Where(x => !inQueue.Contains(x)).ToList();
                if (func != null)
                    func(currentCell);
                if (moves.Any())
                {
                    var valueTuples = moves.Select(x => (x, current.dist + 1)).ToList();
                    valueTuples.ForEach(x => inQueue.Add(x.x));
                    toVisit.AddRange(valueTuples);
                }
            }

            return toVisit;
        }

        private static void PrintMaze(Dictionary<(int r, int c), Cell> cells)
        {
            var lines = Cells.Keys.OrderBy(x => x.r).ThenBy(x => x.c).GroupBy(x => x.r).ToList();
            var str = new StringBuilder();
            foreach (var line in lines)
            {
                foreach (var coord in line)
                {
                    str.Append(Cells[coord].Pipe.Symb);
                }

                str.Append("\r\n");
            }
            Debug.WriteLine(str);
        }


        private static readonly string input =
            @"
.7F77F-J-F.LF-|-LL.J-|-F.7-|77L-L.FLFFJ7-7777-|7-F7.LFF--7F.LJ7-J7F-7-FL-7J77|-L-7-F-F--.J7..7FFLJ7..|7.F|--7FL|-|7F--J---JF-LL-L7-|.|-LJ7.F
FL7J-7FLLL..|||.LL|J.7.L||-F-JJ|-J7|LJ-7.L7L--7-7.|7F-JFFJJ7-|LLL77J|FJ|L7-|-JJL-JF|||-J.||F---77L777L7.FJJLF-7F7FLJ..LF7-L7JL|FL|-77.F-|.F|
7-7FF.||..FJ-|JJ7|JF||-LL|-J7JJF7.|7|7LJ77L7J|F-7JJL-J7-|JLF-F-7.L|JLLJ|-L7JJJFJ|F7J-F7J-FFJJ|FL7L77-LJ.|7JF-JJLF.L|.-7JL77|-7LL.7JF77-.L-F.
7JLJJ|JFLFJ.L|LLF--J||7FFFF-F.|L|7J|J--|LJ.|-|.FJ.-|L-JL.-JLFF.77-7.L|-J7FF7F7F7FJ|7.LL77FJ|J.-LJ7L|-JL--7.JF|.FJ-.L-JJ.L-F-7..|LJ-JF|7-7.L-
LJF|.|L---77-J7---7.LF7F7||.F--F.F-7..FJ7.J7-|F7FJLJ-|L|-|F7.LF-7-L|LFJ7L-|LJLJ|L7L7J.L77L-FFJ.L-J-FFJ.|FL-J-J.|JLFF.7-7LL|-LLF7|-LFJLF7LFL7
L-LL-JFL|.|-7L7LFF-FJ|FFL|-FL77L-J||-F7-F7FL-L-J|J-LF-F|-7|FLJL7|F7|.F-7|FL7F-7L7|FJ.J|.J7J|LF77-J7.|7FJ7F|..FLF7.|-FJF-7F--FJ||F.F77-||-7FJ
L7-L.|7F|FL--F.F-JJ|77|LJJ|JFJJ7||F--J|77FF7FL|7.L..F7.|.F7LL77|LJ|-FJFJ7-FJ|JL7||L-7LJ-L7F|F|.|F7LLL77|LF7.F-JJLJ.|L7LF-7J.L.|7L-|LLF7JFJ|J
|.|F7.L-J7--FJFFJ-LJ||L7-JFF7FLJJ-L-7FJJFF7--F7F7-L7.F.L-|L-7FF|F-JFJFJ|.|L-JJF|||F-J7|JFF7LF7FLL|.|FF-F-LJ-7J|L||LF-J.|L7FJFJJ|7|||L-J-77..
F-FJ.L|J.LJ.J|7.|J.FLJ|JF7LJ|--|.LF-J|JF-|||||L7L7.|7|--7L7FJF7|L7FJFJF7FF-7LF-J||L-7F7FLJLJ|L7F-F7FFJ|.LLJF|FF-L7LJ7LF--L7L||L7JF--7LLJLL|7
|F-LJJ|FJL-FJFFF-7F-L--7LL--LJ-LFJL-7L777||F7|FJF7-7FL.LFFJL7|LJFJ|FJJ||FJFJFJF7||F-J||F|7.FL7|7J|F7JL7--L7-F---LJ|-7.LJ.L--|J7LLLJF7-.|.|.7
F7.FL7LJ|7||F|L77LJJ||FJF|7.||F|.F-7|FJF-JLJLJL7||7.7FFF7L7FJL-7|FJ|F7||L7|-L7||LJ|LFJL7.-F7J||7F7|L7.LFFF7.L7F|J.77|F|FF-|.|.77L|LL7.FJF7--
-7-|JJ.L7--F-7.L-J..F--J|L7FFF-7.L7||L7L----7F-J|L77LJ||L-J|F7FJ|L7||LJ|FJ||.LJL-7L7L-7L7J||FJL-J||FJ77F7||77F77F-|77-J-|.7-J-|-.FJF7---JLL|
F|F|.|.|L|7|FL||LJF7LJ..L7LFF7|F-7|||FJF---7|L7L|FJJFFFL--7||||FJFJ||F-J|FJF7-LF-JFJF7L7L7||L-7F-J||FF-7FJ|F7||F7.LJJ|F7-J.L|-|L7|F-J.JL|-7L
F|-F-7-|--JL|FLL|-FL-F77L77-F-7L7||||L7|F-7||FJFJL7JF7F7F7|||||L7L7LJL7L|L7|L7FJF7L-J|FJFJ|L7-|L7FJL7|FJL7LJLJ|||F77.F|.L|F.-7|--7--77.L|7LJ
||JLLJ.F77JJLFJJF-FJLF7F--7|L7L-J||||FJLJFJLJ|FJF7L7|LJ||LJLJLJFJFJF--JFJFJL7|L-JL-7FJL7L-JFJFJFJ|F-J||J-|F--7LJLJL77|L-F7FL-7|7JLJ.|7|.L-LJ
7-7J..-L-7...L-.|-|FFJ|L-7|F7L--7LJLJ|F7-L--7LJFJL-JL-7|L-7F-7FJ|L7|7F-JFJF-JL-7JF7|L-7L--7|FJFJJ||F7|L7FJL-7|F-7F-J7..LLLJJJLL7.FF|JL.FF||J
|L|JL7...J.-F7L777F7L7|F7||||F-7|F7F-J|L--7FJF-JF7F7F7||F7LJFJL7F7|L7|F-J-L--7FJFJ||F-JLF7||L7L--J|||L7||F--JLJFLJ7.|-7-JL-JJ|-LJ-FJ7-7FF7L7
LF-7F7-F7-F.F|7||F||FJ||||LJ|L7|LJ|L77L-7FJL7|F-J|||||||||F7L7FJ|||FJ||JF7-F-JL7L7|||F7FJ|||FJF---J||FJLJL------7F7-|FJL|.J.7-7J7||LJ-|L-J-J
.L---J7.LFFFF7F7-FJ|L7||LJF-JFJL-7L7L7F7|L7FJLJF7||||LJ|||||FJ|FJ||L-JL-JL7L-7FJFJ|||||L7LJLJFJJF7F||L--7F------J|L-7-7|F7J-|--JFLJ|LF7F|--J
7-7J.LF-FFF-JLJ|7L7L-J|L-7|F7L7F-J.L7||LJFJL7F-JLJ|LJF7|||||L7||FJL7F-----J-FJL7L7|||||FJF---JF7|L-J|F7FJL-7F7F77|F-J.F-77JL7J7-7..|-|J|J-7J
L7----J7|-L---7|F7L-7FJF7|||L7|L--7FJ|L7FJF-JL7F-7L--J||||||FJ||L7FJ|F7F77F7L-7L7|||||||FJLF7||||F-7||LJF--J||||FJ|JF-L7|.F-J.7-L7FJ.|7L7-|7
L.LJ7|-7|L|77.|||L7FJL7|LJLJFJ|F--JL7L-J|7|F7FJ|FJF7F7|LJLJ|L7||FJ|FJ|LJL7|L-7L7||||||||L7FJL7||||L||L-7L--7|LJLJFJF|-L||F7.L7L---7F-L7.LFF|
J-|F|7.LF7F7F-JLJFJL-7|L-7F7L-JL7F-7L--7L7LJ||FJL7||||L---7L-JLJL7||JL--7|L7FJFJLJ||LJ||FJ|F7LJLJL7LJF7|F--J|F---JF-77L||-LJFL.F7L|L--F7.FJJ
.FLFF--7||||L---7L7F7||F7LJL--7FJ|FJFF7|FJF-J||F-J||||F7F7L-----7||L-77FJ|FJL7|F-7|L-7||L7LJ|F---7L7FJLJL-7FJ|F7F7|FJF7||||F7.F|7-|JJ|||-77.
FF7LL-7LJLJL---7L7|||||||F-7F-JL-JL7FJLJL7L-7||L-7|LJ||LJL7F---7|LJF-JFJFJL7FJLJFJL7-|||FJF7||F7|L-J|F----JL7|||||||L|||L--7F77.F-L--FJ|.LJ|
7|L-|JL--7F-7F7L-JLJLJ|||L7|L--7F--JL--7FJF7|||F-JL-7|L7F-JL--7||F-JF7|FJF-JL7F7L-7|FJLJL-JLJLJL7F7-|L7F7FF7|LJ||LJ|FJLJF--J||F7.-J-L7|F77L7
F7J|FF---JL7||L------7|||||L7F7|L-77F7FJL7|||||L7LF-JL-J|F7F--J|||F7|||L-JF7FJ|L7FJ|L--7F-7F---7||L7|FJ|L7|LJF7||F-J|F--JF-7|||L7JL-7L-JLJ7|
FJ-|L|F7F-7|LJ7F-----JLJL7L7||LJF7|FJLJF7LJLJ|L7L7L---7FJ||L--7||||||LJF7FJLJ|L7|L7L-77||J|L7F7LJL7LJL7L7||F-JLJ||F7||.F7L7||LJFJ7.LJ.L|||FJ
L|L|.||LJ-LJ-LFL7F-7F7F-7L7||L7FJLJL-7FJL-7F-JFJFJF7F-JL7|L7F7|LJLJ||F-JLJF7LF-J|LL7FJFJL7L-J|L7LFJF--JFJLJL7F--J||LJL7|L7|LJF-J77|-L77L77JJ
L77.FLJ.|FLLF-7FJ|LLJLJ-L7||||||F7FF7LJF--JL7FJFJJ|||F7FJL7|||L7F-7||L--7FJL7L-7|F-JL7|F7L7FFJFJFJFJLF7L-7F-J|F--J|F--JL7||F-JLF7F7|.J-7|L--
LL-F-|J7LLJLL7|L-JF------JLJL-J||L7|L-7L--7FJL7|F-J|||LJF-J||||LJ7|||F-7|L-7|F7||L-7FJLJL7L7L7|L|FJF-J|F7|L-7||F7F|L7F--J||L---JLJL777|L|7-J
7||LLJ-|F|-|L|L7F7L-------7F7F7LJFJL-7|F7FJL7FJ|L-7||L-7L-7|||F7|FJLJL7LJ7FJ||LJ|F7|L7.F7L7L-JL7|L7L-7||LJF-J|LJ|FJFJL7F7||F----7F7|7-7L|-F7
|L|-7|LL--77LL7LJL--------J|LJ|F7L7F7||||L7FJL7|F-J||F-JF-JLJLJ|FJF---J|F7L7|L-7LJ||FJFJL7|F---J|FJF7||L-7L--JF-J|FJF7LJ|LJL-7JJLJLJ.L|-LJL7
LJ.||J-J-JLJ|FL-----------7L7FJ|L7|||||||FJ|F7|LJF-J|L-7L----7FJL7L7F--7||FJ|F7|F7||L7L-7LJ|F7F7|L-J|||F7|F---JLFJL-JL7FJF---J-F77FF7-|J|.-J
JJ.7LJL||..FFF-----------7|FJ|FJFJ|||||||L7|||L7FJF7|F-JF7|F7||L||FJL7FJ|||FJ||LJ|||FJF7|F-J||||L7F-J||||||F---7L7F---J|FJF--7-|L7FJ|77JFLJ7
.F.|-7F-J-L7|L7F-7F7F---7|LJFJ|.L7LJLJ|||FJ|||FJL7|||L-7||FJ||L7FJL7FJL7|||L7|L7FJ||L7||||F7||||FJ|F-J||LJLJF7FJFJL-7F7|L-JF7L7|FJ|FJ7L-7J|L
L77F-J|L|.FF7||L7LJLJF--JL7JL-JF7L---7|||L7LJ||JFJ||L-7||||FJ|F|L7FJL-7||LJ7||F||7LJFJ|||||||||LJFJL7FJ|F---JLJF|F--J|||F--JL7LJ|FJL--77L-7J
L77L-.J.F-FJ|-L-JF--7L---7L-7F-JL----JLJ|FJF-JL7L7||F7|||||L-JFJFJ|F-7||L--7||FJL7F-JFJ||LJ|||L-7L7FJ|FJL--7F7F7|L--7|LJL---7|F7LJF-7FJJJ|LJ
FFJ||FJF--L7|F7L|L-7|F7F7L7FJL-----7F7F7LJFJF7FJFJ|LJLJ||||F--JFJFJ|FJ||F7FJ|||F7||F7L7|L-7||L7FJ|||FJ|F7F-J|LJLJF--J|F7F---JLJ|F7L7|L7--F77
LJ.FJJ-FJ-||LJ|FF-7|||LJL-JL7F----7LJLJL7JL7|||7L7L-7F-J|LJL7F7|JL7|L7|||LJLLJ||||||L7|L7FJ||FJL7FJ|L-J|LJF7L7F--JF7-|||L-----7||L-JL-J7JL-L
|J7L|L|LJ|FL-7L7L7||LJF----7|L--7FJF-7F7L-7|||L-7|F7||F7L7F-J||L-7||J|||L7F---J|LJ||FJL7LJ-||L7FJL-JFF-JF-JL-J|F7FJL-J|L----7FJ|L-7J.L-F7L|J
L.F7-7F7.F-F7L7L-JLJF7|F---JL-7FJL-JFJ||F7|LJ|F-J||||||L7||F-J|F-J||FJ|L-JL7F7FJF-J|L--JF--JL7||F---7L--JF---7LJ||F---J-F7F7LJ.L--JJ-|||L.JJ
|--JFLJ|-|.||LL----7|||L--7F-7|L---7L-JLJLJF7|L-7LJ||||FJ|||F-JL-7LJL7L-7|FJ|||FJF7L--7-L---7|LJL--7L7FF7L--7|F7LJL-----JLJL---7LF-7JFLFJ|J.
LFJ7-|.LF--J|F7F---J|LJF--J|FJL7F--JF------J|L-7L-7|||||FJ|||F7F7L7|FJF7L7L7||||FJL7F-JF-7.FJL-7F7|L7L-JL---J|||F-7F7F7F-7F7F-7|FJFJ77.JF|FL
L|L7LF|FL7F7LJLJF7F-JF-JF--JL-7|L---JF7F-7F7L7-|F7|LJ||||FJ|||LJL7L7|FJL7L7||||||-FJL7L|FJFJF-7||L--JF---7F7.||LJ.||LJ|L7LJLJ7LJL7|JLJ7.-.J|
.|7..-J-LLJL---7|||F-JF7L7F---JL--7F-JLJFLJ|FJFJ||L-7||||L7|LJFF-JFJ|L7JL7|||LJ||FJF7L-J|.L7L7||L--7FJJF7||L7|L--7|L-7L7L7F7F7F7FJL-7LF-J-L7
--L-FJJ.-7F--77LJLJL--JL-JL------7|L7F7F--7|L7L7|L7FJLJ||FJ|F7FJF7L7|FJF-J|LJF-J||FJ|F-7L77L-JLJ7F-J|F-JLJ|FJL---J|F-JFL7LJLJ|||L7F-J.7-|F|L
|J.LLJFF|-L-7L-----7LF7F7F-------JL-J|||F-JL7|FJL7|L--7||L-J|LJFJL7|LJFJF7L-7L-7|||FJL7L7L--7JF-7L-7|L----JL----7-LJF--7|F7F-J|L-J|7|.LJL-|7
7J.7|F--LF7FJF--7F7L-J|||L-7F--7F--7J|LJ|FF7LJL--J|F--JLJF--JF7L-7LJ-FJFJL--JF-J||||F7L7|F-7L7L7|F7LJF7F7F7F-7F7L-7||F-J|||L--JF-7L7.J.|..LL
.-7|-F7JFJLJFJF-J|L7F7LJL-7LJF7LJF7L-JF7L-J|F-7F-7LJF---7L7F7||F7L7F7L7L---7L|F-JLJLJ|FJ|L7|FJFJLJL--JLJLJLJFJ||F-JFJL-7LJL-7F7|J|FJJ..7J7J|
-7.J.LLJL7F7|FJF7||LJL---7L--J|F-J|F7FJL---JL7||FJF7L7F-JFJ|||LJL7LJ|FJF7F-JFJ|F-----JL7|FJ|L7L7F7F7F--7F--7L-JLJF7|F--JF--7|||L7||J.FFJJ-F-
FJ7FJLJ-LLJ||L-JLJF7F---7L---7|L-7LJLJJF7F7F7|LJL7||FJL-7L7||L7LFJF7|L7||L-7L7|L7F7F7F-JLJLL7L7LJLJLJF7LJF-JF7F7FJLJL7F7|F-J|||FJLJ.F-77L77J
L-F7JL..LLLLJ|F7F-J|L--7|F7F7||F7L-----JLJLJLJF-7LJLJF7FJFJ|L-JFJFJ|L7||L7FJFJ|FJ|LJ|L-----7L7|F-----JL--JFFJLJLJF---J|||L-7LJLJ-J.F7-L7.||.
|7||.|FFJ.FF-7||L-7|F7FJLJLJLJLJ|F7F7F7F7F7F--JJL7F7FJ||FL-JF--JFJFJFJ||FJL7|FJL-JF-JF7F7F7L7|||F---------7L7F--7L----JLJF7L--7F7-F-77FJJJL7
.FLLJ-7.77LL7LJL--JLJ|L--------7LJLJLJLJLJ|L-7F-7|||||LJF--7|F-7|FJFJFJ||F7|||F7F7L7FJ||LJL7|||LJF--------JFJL7-|F7F-7F-7||F-7LJL-7JJF77|JFF
.L|.LJ|-FL-FL7F7F-7F7L---------JF-----7F-7L7FJ|FJLJ|L---JF-J||FJ||FJJ|FJ||||LJ||||FJL7|L--7||||F7L---------JF7L7|||L7||FJ||L7|F---J-L|.L|.FJ
F7|||.--|JFFJ||||-LJL-7F-7F-7F7FJF7F--J|FJLLJFJL7F7|F7F-7|F7LJ|FJ|L-7||JLJ||F-JLJLJF7||F-7|||LJ|L7F-------7FJL-JLJL-J||L7|L-J|L---7.7J-JL---
LL-F-7|7|-L.FLJLJ|F7.FJL7LJLLJLJFJLJF7J||F---JF7LJ|||LJFJLJL-7||.|F-JLJJLFJ|L7F-7F7||||L7||LJF7L7|L------7|L-----7F7FJ|FJL7F7L---7|7JJF77JF|
|||J|7-F-FJ-JLF7F-JL-JF-JF--7F7JL---JL-JLJF-7FJ|F7||L-7L7F-7FJ||FJ|.|J.77L-JF||FJ|||LJL7|LJF-JL-JL7.F77F7||F-7F-7LJ||FJ|F7LJL---7||J|FF|7.7|
FLJ7JJLL|JL-LFJ|L--7F7|F7L-7LJL-----------J|LJL|||||F7L7||FJL7LJL7L-7LF77-|JFJ|L7|||F7FJL7.L-----7|FJL7|LJLJFJL7|F-JLJ||||F7F---JLJJL-LJ7-LL
JLLF-|7LF7..|L7L---J|LJ|L7FL7F7F7F--------7F--7||LJLJL7|||L7FJJ|L|F7|.FJ|||.L-JL||LJ|||F7|F--7F7FJLJF7LJF7F7L7FJ|L----7|||||L---7JLJ7F|FL7FJ
LJ-|FL---7.FF7L-----J-FJFJF7LJ||||F-7F--7FJ|F-J|L----7||LJFJ|J|LJ||LJFJ-F7--7F--JL7FJLJ|LJL-7|||L--7|L-7|LJL7LJ|L-7F--JLJ|||F-7FJLLF7-|-LL-J
LJF-JJ--||F-|L---7JF-7L7L-JL--J|LJL7LJF7LJFJL-7L-7F7FJ|L7.L-JL|JLLJLLLJ|..|L-L-7F-JL---JF---JLJL-7LLJF7LJF7FJF7F77LJF-7F7LJ|L7LJF7L|J||--JJ7
FFL7J..-F-|7L---7L7L7L7L---7F7FJ-F7L--JL7FJF-7L-7LJLJJL-J7JFJ-|F|JL|7LFFFL7.|FFLJF7F--7FL--7F-7F7L---JL--JLJFJLJ|F7JL7||L-7L7L--JL-77-F.|-77
F----J.LL7LF7F-7L7|FJFJLF7LLJLJF7||F---7LJFJLL--JF7F7F7.|L77L||L77.-|--J|JL-7--LFJLJF7L7LF7||FJ|L----7F7F--7|F-7LJL--JLJF7|FJF-7F-7|--L-77L|
|.|.||--L7FJ|L7|J||L7L7FJL7-F--JLJLJF7|L7FJJF----JLJLJL7|JL-.-7-L|77|.|.L-7-|-J.L--7|L7L7||LJL-JF----J|LJF-J||LL--7F--7FJLJL7L7LJ-LJJ7.|LFJ|
7-7.F7..FFL7|FJL-JL7|FJL-7L-JF7F----JL-7|L--JF-7F-7F---J7-FJ7.F-F7-JJ.F|FL|-|FF7J|FJ|JL7||L-7JF7L---7FJF-JF7|L-7F7LJ.|LJ7.F7|FJ7-LL|.LF---.|
77|F-JF-7F-J|L--7F7LJL---JF--JLJF----7FJ|F--7L7|L7LJF-7F7JLJF-|F|J.L77L|-J.|L-JJ-LL7|F-JLJF-JFJL7-F7LJLL--J||F-J||7F7F7F7-||LJ.F77.|-LL..LF7
LJJ-7FL7|L-7L7F7LJL-7F-7F7|F---7L---7LJFLJF7L-JL-JF-JFJ||JFL|.LLJ.L7LF7777FLFJ||JL-LJL-7F7L--JF7L-JL--7F---J|L--J|FJLJLJL7|L7F-JL--7JL|F7..|
L-||F--JL-7L7|||FF-7LJLLJLJL--7L----J|F--7|L--7FF7L7FJFJL-77|-7LF7.|FJ-J7FL.|.|77|F-7F7LJL---7|L-----7|L----JF7F-J|F-7F7FJ|FJL7F---JJ.L|-|7|
LFFFL7F7F7L-JLJL7L7L-7F-7F7F7FJF7F----JF-J|F--JFJ|FJL-JF--J.|-7..F77J|.L-7LJJF7LLFL7LJL------J|F-----J|F7F---J|L--JL7LJLJ.|L-7||F---7-||L|--
F|7JLLJLJL-7F7F7L-JF7LJFJ||||L-J||LF--7L-7|L7F7L7LJF7F7L--7-JFJ77|JL7J7F|77L-||FF77|F------7F-JL-----7LJLJ7F7FL----7L7F7F7|F-J|LJF--JF7--L-J
7|F-F|-LF7FJ|LJL7F7||F7L-JLJL7F-J|FJF7L--J|FJ||FJF-JLJL-7FJ-F-.|7LJJ.FJ7LFJJL|L-JL7LJJF---7||F---7F-7L7F--7|L77F7F7L7LJ||||L-7|F7L7JL-JJJ.||
.F|7|.LFJLJFJF7FJ|LJLJL--7F-7LJF7|L-JL----JL7||L7L-----7|L7F-F-LJ||FF.FF-JFF7L---7||F7L--7LJ|L--7|L7|FJ|F-J|FJFJLJL7L--J|||F-J||L-JFLL7LF-|-
FF7J7.-L7F7|FJLJFJF------J|FL7FJLJ.F7F------J||FJF7F7F-JL-J7-|.|-JF-7.FFJF7L7-F--JL7|L-7FJF-JF--JL-JLJFJL--JL-JF7F7L---7|||L--JL77L|7JLFJ7|.
-||-7-LFJ|||L7F7L7L-------JF7LJF---JLJFF7F7LFJLJFJ|||L-7.F---7F|JLF7|.L|J|J7.FL---7|L7FJL-JF7L--7F-7F-JFF--7F--JLJ|F7F7LJLJF7F--JF-7-77LJ-J.
JJ|.L7FL7|LJ7LJL-JF--------JL-7L-------JLJL-JF7FJFJ|L--JFJF--JF7-7.FL7-L-L.LFFF--7|L-JL----JL---J|FJ|F-7L-7|L-7F77LJ|||F---J|L---JFJJL|-L|--
J|L7|L-L|L7F7F-7F7L7F-7F7F-7F-JF----7F----7F7|LJ|L7L-7LFJFJ-|LLLJ.F.||.-|J-|F-JF7LJF7F7F7F-----7FJL7|L7L--JL-7LJL--7LJLJF--7|F-7F-J||F-J-7-|
J7JLFJJLL-J||L7||L7LJLLJLJFJ|F7L---7|L--7LLJ|L--7FJF7L-JFJ77F7|J7.-F-77FJ7F7L--JL7FJLJLJLJF---7LJF-J|.L-----7L7F7F7L----JF-J|L7LJJJ|J-|7FJFJ
L--L||7|JF-JL-J|L7|-F7LF77L7|||F---JL--7L--7L7F-JL7||F7FJF7FJL-7-||L7L77-FJ|LFF7.||F--7F--JF7FJF7L--JF------JLLJLJL----7FJF7L-JJJ|.LLJL-JF|J
F--FF-F-7L----7L-JL-JL-JL--JLJ||F----7|L---JF|L--7LJ||LJF|||F--J7FF7|FJ77L7L7FJL7||L-7|L---J|L-J|LF7|L--------7F-----7JLJFJL-777|-JF|L-J|L|7
|.L|JLL7||F7F-JF-----------7F7|LJF7F7L-----7FJF-7L-7||F--J||L-7-FFJ||L7LF7|FJL7FJLJF7||F7.F7L--7L-J|F7F---7F--JL7F---JF7.|F-7L7F-JF|7L|L|JL|
LFF-J7FJL-JLJF7|F------7F7FLJ||LFJLJL--7F-7|L7L7L-7|LJL7F-J|F-J77L7||FJFJLJL7L|L-7FJLJ||L7|L---JF-7|||L--7LJF7F7||F--7||FJL7L-JJJ7-FL7-7L|-|
L-|J.LL--7F-7|||L-7F7F7LJL---JL7|F----7|L7LJ7L7L7JLJF--J|F7|L--7F-JLJ|FJF---JFJF-JL7F7LJFJ|F---7|FJLJL---JF7|LJLJ||F-J|LJF-JJL|L|JF|7J|L-F-J
L|FJ7|7F-JL7LJLJF7||LJL-7F7F--7|LJ7F7FJL-JF--7L-JF-7L--7LJ||F7FJL---7|L7L7F7JL7L--7LJL-7L7LJF7JLJL-7F7F7F-JLJF--7LJL7FJF7L-7.7|F7-|J7.L7J|.7
FFF-FF7|F--JF---JLJ|F--7LJLJF-JL7F-JLJJF7F|F-JF7F|FJ.F7|F-J||LJLF-7FJL7L7LJL7FJF--JF7F7|FJ|FJL-7F--J|LJLJF7F7L7|L--7LJFJL7FJ.L7LJJ|.-..L---L
FL|FF|LJ|F--JF-7F-7|L-7|FF7|L--7|L----7|L7||F7||FJL7FJ||L-7|L-7FL7|L-7|7L-7FJ|FJ-F7|LJ|||F7L7F-JL--7|F---JLJL7L---7L7FJF7LJF7|F7F77JLF-||LJ|
-.-F-JF7|L---JF||JLJ|FJL-JL----JL-----J|FJ|LJLJLJF-JL7||F-J|F-J-FJ|F-J|F7FJL-JL-7||L-7||LJL-JL7F7F7LJL------7L7F--JFLJL|L--JL7|L-7JL-F7|7J.F
|L-|F7||L7JF---JL7-F-JF7F7F7F--7F--7F--JL7|F7F-7FJF-7|LJ|F7||F7FJFJL7FJ||L7F7F-7||L-7||L7F7F-7LJLJL7F7F7F-7FJFJL-------JF-7F7||F-J|77L|.L.7.
7J.||LJL7|-L7F-7FJFJF7|||LJ||F-JL-7LJF7F7LJ|LJFLJ7L7|L7FJ|||||||FJF-JL7|L7LJ|L7LJL-7LJL7||||FJF-7F7LJ|||L7|L7L-7F-7F----JL|||LJL7-FJ7FJ-|LJ.
LJ.LJJLL|||L||FJ|JL-JLJ||F7LJL7F--JF-JLJL7FJFF7LF7FJL-JL7|||||||L7L--7||FJF-JFJ|F77|F7FJ||LJL7|-|||F7LJL-JL-JF7||FJL---7F7|||F--J7L|F77FFJF7
|L7-|.|FLJ7LLJ|FJF--7F-JLJ|F7FJ|F--JF7F-7|L--JL7||L7F--7LJ|||||L7L--7|||L7L7FJF7|L7LJ|L7LJF--J|FJ|||L-7F-----J||||F7F--J||LJLJF7LF7FJ|JJLFL-
FFL7|-F|LL-.||LJFJF7LJF--7||LJFJL---JLJFJ|F7F-7LJ|FLJ.FJF-J||||LL7F-J||L7|FJ|L||L7|F7|FJF7L7F7|L-J||F-JL7F----JLJLJ|L---JL----JL7|||FJ|7F-LJ
FFJ-J-L-7F|FJF.|L-JL-7|F-J|L--JF7FF-7F7|FJ|LJJ|F-JF77FJFJF7|LJ|F-JL-7|L7||L7L-JL7||||||FJL7LJLJF7.LJL---JL-7LF--7F7L-----7F7F7F-J||||F7-F7L-
F-7-JJ.|L-|.FF7F-----J|L--J-F7FJL7L7LJLJL7L--7LJF7||FJFJF|||F-JL7F--JL7||L7L7F--J|LJLJLJF-JF7F7|L---------7L-JF-J||F7F7F7LJLJLJFFJLJLJL7J7-L
|FL7|F--J.F-7||L------JJF---JLJF7L-JF7F-7L---JF-JLJLJFJF7|||L7F-JL7LF7||L7L-J|FF7|F7F-7FJF7|LJ||F-7F-7F--7|F-7L7FJ||LJLJL---77F7L7F--7FJ|JJ.
F7LF7J-||FL7LJ|F7F-7F--7|F-----JL---JLJFJF7F7FL-7F--7L7|||||FJL7F7L7|LJL7L7F7L-J|||||FJL7||L7FJLJFJ|FJL7FJLJ|L-J|FJ|F-------JFJL-JL7-||FF7.F
LJJL7-F7-L7L-7|||L7|L-7|LJF7F----------JFJ||L-7LLJF7L7LJLJLJ|F7LJL7||F--JFLJL7F-JLJLJL7FJ|L7|L--7|FJL7FJL---7F77|L-JL-7F----7|F----J-|L7L7F-
|.F-JF|-.LLF-JLJL7|L--JL7FJ|L-----------JFJ|F-JF-7||-|F----7|||F7L||||7F7F7F-J|F7F7|F7|L7L7||F--JLJF7LJF7F--J|L-JF7F7FJ|F---J|L7F-7F7L-J.--J
LLJL-|J7|J|L----7|L----7|L7L---7|F------7L7||F7L7LJ|FJL7-F7LJ|||L-JLJL7||||L-7||||L7|LJFJFJLJL77F7FJ|F-JLJLF7L7F-JLJLJJ||F7F7|FJ|FJ||L|J-|L|
F|.FL77F-F-F7F--JL-7F--JL-JF--7L7L-7F--7|FJ|||L7L-7|L7FJFJ|F7||L---7F-J|LJ|F-JLJ|L7|L-7|FJF---JFJ||FJL-----JL-JL-------JLJLJLJL7||FJL--77-77
JJ-LJ|-F7F7||L----7|L------JF7L7L-7LJF-JLJFJLJFJLFJL-JL-JFJ|||L-7F-J|F-JF-JL--7FJFJ|LFJ||FJF--7L7|||F-----------7F-7F7F7F-7F7F7LJLJF-7FJ7.|J
|J..-J|JFJLJL-----JL--------JL-JF7L-7L-7F7L-7FJF7L--7F---JFJ|L-7|L-7|L7FJLF7F7||LL7|FJFJ|L7|F-JFJ|||L--7F------7||LLJ||LJF|||||F7F-JLLJJF-LF
|F7FJ.LFL---7F7F7F-7F7F7F-7F-7F-JL-7L--J||F7||FJL7F7|L7F-7L7L-7|L7FJ|FJ|F7|LJ|||F7||L7L7|FJ||F7L7LJL7F7LJF-7F--JLJF-7|L--7LJ||LJLJF-7J|FJJ.|
JFJ777L|.F-7LJLJLJFJ|||LJFJ|FLJF7.FJF7F7||||||L7FJ|||FJ|FJLL-7||FJL7||FJ||L-7|||||||LL7|||-||||J|F--J||F7L7|L-----JFJL7F7L-7|L----JFJ--J-||J
.|7|F7F--JFJJF---7L7|||F7L-JF7FJL-JFJLJ||LJLJL7||FJ|||FJ|F7F7|||L7FJ|||FJL7FJ||LJ|||F7||||FJ|||FJL7F7|LJ|FJL-7F7F--JF7LJ|F-JL7F7F-7L7J|---7.
7.LL|LJF-7|F7L--7|7LJLJ|L7F7|LJF---J-F-JL-7JF7||||-||||FJ||||||L7||.||LJF-J|LLJF-J|||||||||L||||F-J|LJF-JL--7|||L---JL-7|L--7|||L7|FJ777LLF-
L-7.L7FJFJLJL7F7||F--7FJFJ|LJF7|F----JF7F7L7||LJ|L7|||||FJ||||L7|||FJL-7|F7L--7L-7||||||||L7||||L-7L-7L-7F7FJLJL7F7F7F-JL---JLJL-JLJJ.|7||J7
L-LF-LJJL--7FJ||||L7FJL7L7L--J|||F---7||||FJ|L--JFJLJLJ|L7|||L7||||L7F7|||L7F7L7FJ|LJ||LJ|FJ||LJF-JF-J.FJ||L7F7.||LJ|L---7LF----77-|F7|LFL.|
F--JFLF--7FJ|FJ||L7|L-7|FJF---J||L-7FJ|LJ||F|F7F7L--7-FJFJ||L7||||L7|||||L7LJ|FJL7L7FJL7FJ|FJL-7L-7L7-FJFJL7LJL7|L7LL-7F7L-JF7F-J7J.F-|JFFJ7
7L7-J.L-7LJFJL7LJFJL-7||L-JF7F-J|F7|L7L-7LJFJ||||F--JFJFJFJ|FJ|LJ|FJLJ|||FJF-J|F-JFJ|F7|L7|L7F-JF-JFJFJFJF-JF--JL7L7F7LJ|F7FJ|L--7J.7L77F-JF
F-|-|7L|L-7|F7S7FJ7|FJLJF-7|||F-J||L7L-7L7FJFJ|||L-7FJFJFJFJ|FJF-J|F7FJ||L7L7FJ|F7|7LJ|L7LJ||L7FJF7L7|FJF|F7L---7L7LJL-7LJLJL|F--JFFL-LFJJ.|
|F-.---F--JLJL-J|F7FJF-7L7||||L7FJL7|F-JFJL-JFLJL7FJ|FJ7|FJFJL7|F7LJ|L7||FJ-||FJ|||F7.|FJF--JFJL7||FJ|L-7LJL-7F-JFJF7F7L---77LJ|F|J|F.FJ.L7J
.FJFLJ||F7F7F7F7LJLJFJL|FJLJ||FJ|F7LJ|F7L-----7JFJ|FJ|F-J|7L7FJLJ|F7L7|||L-7LJ|FJ|LJL7LJLL--7L-7|||L7|F7L---7|L-7L-J||L7F--JF7-F7.|F.-JF..L7
F7.F-7-LJLJ||||L7F-7L7FJL--7LJL7LJL-7LJL7F-7F7|FJFJ|FJ|F7|F-JL-7FJ||FJ||L7FJF7|L7L7F7L--7F--JF-J|||FJ||L7F-7||F-JF-7LJFJL---JL-JL-7LF-FJ7F-7
L|7L-7J|.F-J|LJ7|L7L7|L7F-7|F--JF7F7|F--J||||||L7L7|L7||LJL7F-7|L-J||7||FJ|-|LJFJFJ|L7F7|L7F7||FJ|LJ-||FJ||LJ||F7|FJF7L7F7F--7F7F-J-J.L|F--.
L.||LJ7|7|F7L7F7|FJFJL7|L7LJL--7|LJ||L7F7L7|||L7L7||FJ||F-7|||LJF--J|FJ|L7|FJF7|-L7L7||||7|||L7L7L--7||L7|F--JLJLJL-J|FLJLJJL||LJ7|JJ|7.7J|7
|.F7777|FJ|L7||LJL7L7FJ|FJF-7F7|L7FJ|7LJ|FJLJ|FJFJ||L7||L7LJL--7L--7|L-JJLJL7||L7FJFJ||LJFJ|L7L7L7F7|LJJ||L7F7F-7F7F7L-----7JLJLL-|---F-F.J-
F7|JF7-FJFJJ||L7F-JFJL7|L7L7LJLJFJL7|F--JL--7LJFJFJL7|||FJF-7F7L77FJL-7F----J|L7|L7|FJL-7L7|FJFJFJ|LJJF-J|FJ||L7||LJ|F7F---JJLJ-JLF-7-JFLJ7J
|L--|L-JFJJFLJ-LJF-JF-J|FJJL7F7FJF7|||F-7F--JF-JFJJJLJLJL-JFJ||FJFJF7FJL---7FJ||L7LJL7F-JFJ|L7L7L7|F--JF7|L7|L7||L-7|||L---77J7.J.FJ|---FLL7
LJLLL-7FJJFLF7F-7|F7|F-J|F--J|||-||||||FJL--7|F7|F7F-------JFJLJ-L7||L--7F-JL7FJFJF--JL-7|FJFJFJFJ||F-7||L7|L7|||F7||||F7F7L7F|7.-L-.-7-7.LJ
J7|J.L||JLL|||L7LJ|||L--JL7F7||L-J|||||L7F7FJLJ|LJ|L-7F7F7F7|7F---J|L7F-JL-7FJ|FJ.L--7F-JLJ-L7L7L7|LJFLJ|FJ|FJ||LJ||||||LJL-JJ.L7|FL--JFJ--J
LL7.L-LJLF7FJL-JF-J||F-7F-J||||F7FJ||||FJ||L-77|F7L-7LJLJ|||L7L7F-7L7||-F7F|L7|L7F---JL-----7L7L7|L--7.FJL7||FJL7.||LJ|L----7J|.F-J7|FJ|JJLJ
-|J-7||LFF-L7F7FJF-JLJFJL-7|LJ||LJFJ|LJL7||F7L7LJ|F-JF---J||FJFJL7L-J|L-JL7L7||FJL---7F7F7F7L7L7|L7F-JFJF7|LJL-7|F||F-JF7F-7L7-.-7L|-J-L-7.|
.LLF-FJ.LJ.FLJLJJL7F-7L7JLLJ-FJ|F-JFJF--J|||L7L-7|L-7L---7||L7L7FJF--JF-7FJ.|||L7F7F7||LJLJL-J.LJ.|L7FJFJ|L-7F-JL7LJL7FJLJJ|FJJFL--L-JFL7L.7
|.FJLL-7-J-F|J.FF-JL7|FJ.LJL-L7||F7L7|F7FJLJLL7FJ|F7|F---J||FJ.||7L7F7|J|L7FJ||FJ|LJLJL-----7F----JFJL7|FJF-J|F7FJ|F|LJLJJ7LJ.|FJ|7LF-7.FLF|
F7|.F|7LL..F-.--L-7FJLJ|.F-7.|LJ|||FJ||||J7JF-JL7||LJL7F7FJ|L-7||F-J||L7|FJ|FJ||FL--7F7F-7F-J|F---7|F7LJL7L-7|||L7F777L7.-FJ-----L-J|LF-7.FJ
FJLL-JL.|.7JJ7.J.FJ|FJF-7|.J-7|.LJ||JLJ|L77FL-7FJ|L7J|LJ|L7|F-JLJ|F7|L7||L7||FJL--7FJ|||FJL7-LJF--JLJL7LFJF-JLJL7LJ|7-777J.LFJ7F|7L-F.LLJ7.|
|7..L7L-F7JLF|7-LL7|FLJJ|-F7FL7FJLLJJ.FL-J7FF-JL7L-JLF--JFJ|L---7LJ||FJ||FJ|||F7F7|L-J|||F7L--7|F-7F-7L7L7L-7J|JL-7L7J|.77FF7.77-F7LFF.7J--L
F-77L7FFJJF-FJ|7-|LJ7|.LFJ|F-JJ-7.|J7.FLJ7F-L7F7||-|-L--7|FJF-7FJ|FJ|L7|||FJ||||||L--7||LJ|F7FJLJJ|L7|FJFJF7|.7F|L|FJ.|.-JFFLFJ|7|J-L|-L7..|
7-77.|F|.F|JL-J|FF|F|FL7L-J||.L|FF|.|--JL|J|-LJLJ-.|-JJFLJ|FJLLJJ-L7|FJ||||FJLJ|||F7FJ||.FJ|LJ.F--JFJLJJL7|LJ-|-J-LJLF7F|.FJ.J-J7|F--|LL--|7
JJ|7F--777.L||F7F||L-7.JJFLL7.7F--.LLJL7-J.F7JJ7||-L--FJ7FJL7J7..LLLJL7|LJ|L--7LJLJ|L7LJ-L7L77LL--7|JF7F-JL---7.L|.L-||FLJ|FJF7FF-FJ-L7LL--F
LFF|J|L|FJ7.J-7F7-J-L|F|JFFL|FFJJ-JJF-7J.L..L.LL-|LJJ.|JF|F7|F7-J--J|L||.FJF7FJJ|J||FJJ|.LL7|7FLJJ|L-J||F7F--7L7JL-|J||-JJ-FLL.F.L|L-JF-FJ.L
7JL|J77FJ-JJLLLJLLJ|.LF|-FF.|-FJ|.LFJ7JF7|.-|-F.|FJ.LFJFFLJLJJJ.L|.LL7LJ-L7|||JF|FLLJF|-.LLLJ-|LFF|F-7|LJ|L7JL-J7-F-J||J7FFF-LJ.LF|-J.|FJ-L7
|.F.FL7LLF|-FFF|7|-|FFLJFJJ.L---J|.LJL-|-LLJ.||FFJ|..FL7|.|JJ-L7.LF-LFJJJLLJ||F-JJL|.JLJ7J||F-LF-FJL7LJ7.|FJ.LL.|LJL-.JL-JJ|F7-J|LL7-|.FJJJ|
J7--77|-FL|7||LLFF-F7JLL7J-L-FJ-7.J|FL7|J-JF|-F-J-J-|JJL|-LJ|7.J7.7J||7L.F|FLJJ.|JF77|.L|-LL7LLFJL-7|.LL-LJJ-7--F.|..F--JL-F---7LL-J.LL7.F-|
|JLFJJ|FLJJ|-J-7J|LJJ7|L|.7|F|.F7.L--7F-J.LF|...|.|F7-7-||--L--JFFL.FJ7-7|LLJJ7L|FL-J77|L7LL|.JJ7F-JL77|..||JF7L--77.LFJ.7-|FLL77|.L-7.LL|LJ
|7-|7.L|.FFL-F.L7F.JF-7.LFJ-LJJ-7.F7LL|LF7J|LFLJL-F-J.L7L-F7JJ.F7J|FFJ|F|7.|F--JL-J7JL77.7JLL7JL-L---JJ7.F7|7||77LJ-.|..FJF-JJLFJJ-FFJFFLL..
-|F--L.L|-7LFLJLLFJ7L-F7-J.7||JJ|F.F7||F-J.F7-L--LJJ-L7L7|L7-7F-|.L7|LJ-77F|7J.FFJL|7-7-LJ7-F||7-|.F|JL-7|LJ.FL-7-||.|--L-F7.|7LJ..-FJ-|LL-7
LJJLF-|-L.J.LJL-.7-7JLLL-LLLJLF-|J-|.|-L---JJ-L-|LJLLJ7.--77-J-JJ.L7-FJLL.FL-7-L--FJJ.|J|-L-7JFF---|J-7JL-JLLJJLL-F-FJJ-L-JJJ--7L-.|.L.F-|JJ";
    }

}

