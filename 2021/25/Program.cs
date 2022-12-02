using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using common;

namespace _22;

public class Game
{
    public static Game game;

    public Game()
    {
        game = this;
    }
    public abstract class Secu
    {
        public int r, c;
        public int blockCounter = 0;
        public bool Locked { get; private set; }

        protected Secu(int row, int col)
        {
            r = row; c = col;
        }

        public abstract (int r, int c) NextPos();

        public void Copy()
        {
            game.NextBoard[r, c] = this;
        }
        public void Move()
        {
            var next = NextPos();
            if (game.Board[next.r, next.c] == null)
            {
                game.NextBoard[r, c] = null;
                game.NextBoard[next.r, next.c] = this;
                r = next.r;
                c = next.c;
                blockCounter = 0;
                game.NumberOfMoved++;
            }
            else
            {
                game.NextBoard[r, c] = this;
                blockCounter++;
                if (blockCounter >  game.SizeR+game.SizeR)
                    Locked = true;
            }
        }
    }

    public class EastBound : Secu
    {
        public EastBound(int row, int col) : base(row, col)
        {
            game.Eastbound.Add(this);
        }

        public override (int r, int c) NextPos()
        {
            return (r, (c + 1) % game.SizeC);
        }

    }
    public class SouthBound : Secu
    {
        public SouthBound(int row, int col) : base(row, col)
        {
            game.Southbound.Add(this);
        }

        public override (int r, int c) NextPos()
        {
            return ((r + 1) % game.SizeR, c);
        }

    }


    public Secu?[,] Board = new Secu?[0, 0];
    public Secu?[,] NextBoard = new Secu?[0, 0];
    public int SizeC { get; set; }
    public int SizeR { get; set; }
    public List<Secu> Eastbound { get; set; } = new List<Secu>();
    public List<Secu> Southbound { get; set; } = new List<Secu>();
    public int NumberOfMoved { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int r = 0; r < SizeR; r++)
        {
            for (int c = 0; c < SizeC; c++)
            {
                sb.Append(Board[r, c] == null ? "." : Board[r, c] is SouthBound ? "v" : ">");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public int PlayRound()
    {
        NextBoard = new Secu[SizeR, SizeC];
        NumberOfMoved = 0;
        Eastbound.ForEach(s => s.Move());
        Southbound.ForEach(s => s.Copy());
        Board = NextBoard;
    //    Console.WriteLine("Inside:");
    //    Console.WriteLine(this);
        NextBoard = new Secu[SizeR, SizeC];
        Southbound.ForEach(s => s.Move());
        Eastbound.ForEach(s => s.Copy());
        Board = NextBoard;
        return NumberOfMoved;
    }

    public void Play()
    {
        Console.WriteLine("Start:");
        Console.WriteLine(this);
        var rounds = 1;
        while (PlayRound() > 0)
        {
            Console.WriteLine("After:" + rounds);
            //Console.WriteLine(this);
            rounds++;
        }
        Console.WriteLine(rounds);
    }
}
internal class Program
{

    private static void Main()
    {
        var stream = StreamUtils.GetInputStream(testData: Data);
        var sw = new Stopwatch();
        sw.Start();
        var game = new Game();
        var all = stream.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        game.Board = new Game.Secu[all[0].Length, all[0].Length];
        game.SizeC = all[0].Length;
        game.SizeR = all.Length;
        var row = 0;
        foreach (var line in all)
        {
            var col = 0;
            foreach (char c in line)
            {
                game.Board[row, col] = c switch
                {
                    '.' => null,
                    '>' => new Game.EastBound(row, col),
                    'v' => new Game.SouthBound(row, col),
                    _ => null
                };
                col++;
            }

            row++;
        }
        game.Play();
        sw.Stop();
        Console.WriteLine("" + sw.Elapsed);

        return;
    }

    private static string Data0 = @"v...>>.vv>
.vv>>.vv..
>>.>v>...v
>>v>>.>.v.
v>v.vv.v..
>.>>..v...
.vv..>.>v.
v.v..>>v.v
....v..v.>";

    private static string Data = @"....v>>.>>v>....>>v....>..vv.>..v.>.v>v.v..v>vv.>>>...>v..v....vv.v>.>v.>..v...vv>v.>v>>vvv...>>>vvvvv>.>>>vv....v>>>vv.>.>>>..v>>v>>.>>..>
.vv>.>.>....v....>v>>vvv....>>v....vv...>..>..v...>.>.vv.....v..v.>.>..v.>v>>..>v.....>.....>>.v>v.......v...>v.vv.v>........>.....>..>..v.
vvv..>>...v>v.>>.>......v.>>v...vv..>...>v>.....vv..v..........vv>vv..>>v.>v>...>.v>..v.>>v.v......v.v..v>v.v.v.....v>>..>v>vvv......v>...v
..>>>vv...v>.v>.>v>.vvv>..>.>.v>.>.v.vv>v.v.>v>...>vvvv....v.vv>.v..>v>.vv.v..v>.vv>>>..v..v>.>.>>...vvv>v>.>.v>..>>...v....v>.>...vv.v.v.v
>.>..>v.v..>>.v>>..v...v>..>.>.v.>v.>.....>.........>>v.v..>.>>vv.>>...v>v...v.>v..v>.v>.v..v>>.v.>vvv...v.>v>...>v.vvv>v..>>.>v..>...>v>.v
.v.>v...v..>.v.>.>v>..>v>..>>v..v>v.>..>.v.>>>.v.vv...vv.vv...v>.>v>....v>..v.v...>.>v.....vv>.v.>...>>>>>.>>.v.>......v..>..v.>>>>..>v>.vv
...>.v.vv...v...v.v>v>>.v.>>..>.....v.>.v.v...>.v.v.v...>>.>>.v>v...v>v....vv>>vv>...>....>.v.vvvv....v.v>>..v>v>.v>..v.>.v..v>>vv>..>>....
.vv.>>.>vv...vv.>>..v.vv.>v.v.>>>.>.vv>>.>.v>.>.v...v..v..v>v>.v.v>>..>v..>>..v...v>vv..>..>vv.vvvv>v.v>>.v>>>v.....>v.v>.>>.>>v.>>.>..>>v.
.......v..>..v>...vv.>>v>..>..>.>..v>.>>>vvv.v..v.vv>..v>.vv>v.v.>.>vv.vv>.vv.v>.v..>.....v>..v..vv>.>.>...>>v.>>v.>.>v.v>>>>...>.v>v>>v.>>
>...>>>v..>..>>....>.>>>v.....>>v.v...v.....v>.>.>..vv.....>...>v..v.....v.>v>v..v>..v>v.>v.....>..>.>.>>v.>.v..>.v>>vvv...v>>>>>vv.v>..>v>
....>..>.v>v.v.>..v>.v>>..v.v>>vvv>>.>v.>>...v...v>v>>.v>.>.>.....v.v>.v....v>>>..vv..v>>>>>..>.v.v.>v.v..>v>>>...>>v.....>v>>>.v>>.v..>.>v
vvv.......v..>..vv.......v>>v...>.>>..>.v....v.>>...>>...v...>.v.>.v>v.>.vvvv..v.vv...v....v.v>>.>..>...>...v>>>..v.>.....v.>v.>v..>v...>..
......>>>>.>vvv.>.......v>>v>v...vv...vv...v>...>.v.v..vv>.v>>..v.vv>v..v>.v>vvv>vv.>vv>>>...>.>.v...v..>>>...v..vv.>.vvv.>>>vvv.vv.v>v.v>v
>.>v>>>v...vv.v>..vv>...vvv....v..v..vvv>.v.>.vv..>...>>>.>>v>..v>>>.vv>..vvv.vvv.>>v.>.>>v>..vvv>.>>v.....v..>.>..>...v>.v.v.v.>.v>.v>.>.v
>.>v..v.v>vvv.>.vvvv>...>....vvvv..>...v.....v>v.>>..>.>..>v>.>>.v>>.v....>v>..>>v..v>.>>v>..v.>vvv>vv.>...>v.>>..>..>.....>>...v...v.v>.>>
.>.v..>>..v.>>vv..v..>>>>....>>.v>....>.v.....v....>v..>..>>.v.>..v.....v..v>v..vvv>v..>..v.>v.>v>.>vv.v..v....>.vv>.>>.vv>.>vvv>.v.v>..v.>
>>v>>......>.>>>>..>..>......>vv...>>v.>.....>.v.v>v>v..vvv.>>v.v.>v.>..>.>.>>.>v..vv...>v.>..vvv>.>..>v>>...>..v.v..v>..v>.v.vv>v.v>v>vv>>
.....>>v.v..v..v.....v>.v..>>....>...v>v.>...>..v....>...>..>.>..v..>v.......v.vv>.....vvv.>vv..>...v..>..>.>>.>v..>vvv...v..v.vv..>>v.>v>.
>..v>>>....v.v>..>>.>v.>..>..>v.v.>.vv..>.v.v....>..v..>...>.v>>..>>>...v..v>v>.>vv...vv.vv>..>>vv..>>.>v>.v.>vv.>v>..>v>..>.v..v.........v
v.vvv>v.>...vvvv.>>.>...>v>........v...v.v.>>v.vv.>vv>v...vvvv>.v...>.>v...v>.>>.>>.>v>v.......v.vv>v>.v..v>..vv>..v..>>..>v>.v>.>v>.>v>.v>
......v.>v>>v>v..v.v.v..>v.....>>.vvv>>.........>.v>v>>.v>v.v>>..v>>.v.vv.>.>vv>>.>....v..>.v....>.>v.v.v..>>..v....v..>>>vv.v..>.>.>>...v>
vv..v>..>v>>vv.......>>..v....>v...>>vv.>.v>..>v...>v.......vv........>>>vv>>...>v.>v.>>...>>v....v.>>.v.vvv>>v..>.v>>..vv>>>.>..v>...vv.v.
vvvv>..v..>.......>..v.>>v>...v>..>.....v>>>>..>..v.v...>.>..v.v.vv.>......v.>.v.v...>..>>vv..>..v>v......>v.vv..v>>..v...v..>..vv>.>...>..
..v.v>v.vv>vv.>....vvv>..>>.v.v..v.....>>.>.v>>..>.v...>>>>.vv.....vv....>.....>>..v..>..v..v>v>>.v>>>v.v...v>>.....v>>.>.>....v.v.>..>>..>
>v.v>v.vv..v...vvvv.......>.v>>>v>..v.>.v.vv.vvv.v..>v.>.>.v.v...v.vv......v.v...>.v.>.>>..v..>vv.>....v.>>>v.>..v.>.>>.vvv.>v.....v..>>...
....v>..vv..>..v.>>v.>vv.>.>.>vv.>....>>v.>>v.>...vvvv.>...>>...v>.v.>...>v>...>.>.v>v..v.vvv>v.>v.......>.>>>.v>..>v>.v.>vv>..>>v...v>v.>.
v..>.>>v>.v>vvv....>.>.vv.....vv..v...v>.>>v..v..v>.....>>v.>....v>...>v..>>>..>>.>>...>vvv....v>v.v>>..>.v>.>.>>.vv.>.v.>.>..vv>..>.v...>>
..>..v>.v...>..v..v>v.>>.v>...>.vv.v.>>v.v...v..v..>v.vvv....v>>.v>...>>vvv...>vv.v..v.>v>>>v..>v..>...>>.>.>..>>vv.....>v.>..>.....v..>..>
.>...>>vv.v.v>.>v>>vv>v>>...>v>vv.>.v..vvv>.>.vv..>.v>..vv....v...>vv>v.>vvvv...vv....>...v.v...>..vv..v...v.>>v..>v..v>v.v>.>..vv....vvv..
v..vv>vv>.....>v.>>...v>>...>...>.>v......>.v.>v.>.vv..........v..v>..v.>...>>>.>v>....v..v.>>>..>>.>....>>>..>.v>..v..>>vv.vv..>v>v>.>>.v.
>.....>..v..>...vvv>.>v>..>>v.v.>>.v..v.>>.vv.v>..>>>>.>..>v.>v>..>v.vv>.v.>vvvvv>.>>.>.....>>..vv...v..vvvv.vv....v>...vv....>.v.v>v.>vv..
vv>.>v.>........>>..v.>v..v....>>>v.....v>v..v>v.v>vv....v.v.>.>v>...>>.v>>..>.>.>..>>>>>>...>...>>v>vv..>>...vv>v..>v.......vvvv..>...v...
v.vv.....>v...vv....>v.>>..>.>v.>>vv.v..v.v.....v...v.>....v.v....>.vv...>>>.>>......v>.vv.>.v>>>.>.>..>..>.>..>.>>.>v.>vv........>vv...>v.
.v..>vv>>v....v..>>.>>>v....>>>>>vvvvvvv.>...>>..>vv..>v..vv>>..v>.>.>>..v>v>.v...>.>>....>>...v..v..v..vv>vvv.vv>vv..>....v.......v>.....>
.v>>v...>>.v......>....vv.v..vv.v>.>.vv.>>...>..>.v.>.>>v.>v..>..v..vvv.>.vv....>vv...>.>.....>>.>.v>.>v..>vv.v...v.v>>>...vv..v.v.v....>v>
v..v>>>..>>vv>v..vv>.....vv.vv.v..v.>>.>vv>.vv.v.>...>..>>.vv>..v.>vv....vv.>.v>>v.vv....>>..>v...v>.>.>v.......vv....v..vv...v...>>vv>.v..
vv...>>.>>..v>>...>.....vvv>....v>>v>v>vv.>.>.vv.v.>.>v>.>v....v.v...>>.>..vv>v.>..>>>v.v>..>>v.v..v>.v>>...v.....>>..vv>>>..>.>.>>..v>v..v
>v..>.vvv>.v..>v..v.v.v..>>...v..v>>vv>>...v..v.....v.v>v....v>.>.vv..vv..v..v.v...>>>vvvvv...>..>>v>v.>...v.>>.v.>.>..>>>>.>.vv.>..v......
...v.v.v..>...v...v...>>v>.....>.......vv>...>.>.v>v.....vv.>..>.....>....v..v.....>...>v.>..v>v.>.vvv.>v.>.>....>.vv>.>>.>....>v..v.>v>vv.
....>..vvvv..>v.>v>v.......vv.vv..>.v>v.vv.>.v>>.v..>.vv>>.>v.>>....>v.v.v.>...v..>>v>..>.v.>.vv...>>>vvv..vvv>..>>...........v.v..>v>.v>..
>..>v.>>...v..v>v.>.>>.>..v>v>>.>.>....>.>.>>v.>.>>v.>v.>>..>>>.>.>>..vvv.>v.v>>>>>..>...v>>v...>.>..>.>..>.>vv.v..>v.v..>..>>..v>>v.v..>..
vv.vv..v..v...v....>..>vv..vv>....>vv>.>v.v...v>.>>v>v.......>....v.v...v>..>.v>..vv..>>..v.>...>....>..v....>v>...>>.>v>.v>v.v>.......>>..
.v.>>.....>.>>.v>v.>v>vv>...>........>vv>v..>.v..>v..>...vvv....>>v..vv>v.>....v.>>.>.>.>>.v.v>..>vv>>>.>v.>>>v>>v.>..>...v>.v.>.v>>.v>v..>
>..>.v.vv.v.>vvv>.vv.v.v>...vv>.....v..>..v.vv...v.>>>....v>>..v>.v>v..>>.v.v...v.>.v>.v>.>>v..>.>.>...>>v.....vv..>v....>v.v>.v......>v..v
v.>v...>>.>...>.>v.>>v.>>vv>..vv...v.v.>.v.v>v..v.v.v.v.>....>...vvv.>vv...>.>v>.....v>.v.vv>.>>vv..v.v...vv...vv.....>..>v.>.>..>.>..v>.vv
>>.v>.>>v.....v...>>...v..v>>........>.vv.......>vv..>.v.v>.....>v.>..v....>.>v......v>....>v..v..>>..v>.......>.v..>>.>...v>.vv>.v.>>....v
....v>.v>...v.v>.v.>>v.v.>v.vvv>.vv...>>..>.>.v>.>>....>>>v.vv>vvv.>v>>.vv.>v..v>.vv>..>>.v>.v...>v..v.>...vv......v.v.>>..v>...>.>..>vv..v
.vv>vv.v..vv........>...>.>>.>>>v.vv.>v.v.>....v...v>..>>>v...vvvv...vv..v.v>v....>.>.v..>...v.>>...v....v..>v.>vv..v...v...v.>>.>..>.v.vvv
.vv>vv..v..v.vvv.>>..v..vv.v>>vv....vv>>.v.>v....v.>v>.....vvv.v...>...v..>.>.vv>.>>.>v>>..>....v...>.v>vv....v.>v>.>>v......vv>v.v>..v.>..
.v..v.v>vvv>.>.v...v>....>>>>..v.vv>.>.>...v>>>>..vvv.v.>.v...>..v>..>v.>.v.>>.vvv.>.>.>..>>>>....>....>..>..vv.>.v>>v>vv.v.>>...v.v>>v...v
...v.vv..>>vv..v...v.vv.vv.v.v...v....>....v.v...v.....vv...vv>....>.>...>..>.>..v.....>>>......vv>>.v....v...v>.....vvvv..v>>.......>>v>vv
...>>.>>>v..v...>....>....>.>..v>v....>v.>v..>.>>>v.v....vvvv.>......>v..vvv>.>>..v.vv...v>.>v>>vv.v..v>.v.....v>.v....>.>..>>...>vv>.>.vvv
.>vv...v>>v..vv....v..>vv.>v....v...>>..>.v...v..v..vv....>..>...vv.>v..>>v...v..>v.v>...v>v>>.vv.>>....v>>vv>v>v.>.>>....>..vv..>..>>>>>v.
v....>>>.vv..vv>>v>...v>.....>>v.>.>.>.v..v>v>.>v.v.vv...v..v....v>>.v......>..>..>.v.v..vv.v...>vv.>...>..v.>.v.......v>v>.>>>vvv..>>..>>.
v>vv.>..>>...>.v>v>....>.>..vv.v>>...>>>..v..vv>v>>>...>v.>..v>...>.vv>v.>>v.v..>>>v>v>v..v.>..v..>>.v.>.v.>.>>..vv.>>>>>>.>..v>.>v.>.v>>>v
v.>>>v.v.>v>v>..v>>..........v.vv>.v>>v.vvvv.>...>v>.v..>>v.vv..v........>v.>v.v.....>.v..vv>>.vv.>v.vv>.>vvv.vv..>>vv.>>>....>v...v.>>>>>v
>.>.v..v..>......>..>>>v.vv..>>>v>.v>>v>>>>.v..v..>>v>vv.>..v.>>v..vv.vvv..>.v...v....>.>v..v...v.v.v..v.vv>vv.v....>.v.v.>>.v....>....>.>v
v......>.>..v.v..>>..>v.vv>.>..v..v.vvv.>......vv>..v....>.>v.v.v.>..vv>>v.v..>.>>...>.vvv>>>>vv..v.>vv.vvvv.v.vv>.>vv.>>.>v.v....>>>..>...
.v.v..>....v>.>v.v.vv.v.>.>.v>...>...>.v..>v.>.>vv>.>>...>.>v>v..>>>>.v.>vvvv...v>>....v..>..v>...>...>.>>>....v>...vv..>..>>>..v>...>..>>>
..vv>..v.v.v.>.>>.>v>vv..v.v.vv>.vvv.....v.>>.v..v>vv.vvv.v.v.>..v>...>.>v.>v.vvv..>v.....>vv>v.>.....v>.>vv..>v.vv.vv..v.>v.vv..v>.>..v.>.
.v........v.vv.v.vv.>>v.>v>>vvvv>..v>>.>v>..>...vv>>v.vv..>.>v.v>..v..v.>.>..>v>>..v....v>>>>.>v>....>>v.vvv..>...v>>vv.>..vvv..vv..>.>..>.
...>>v>>.v>>..>vvv..v>...>..v.v>....v..v>.vv>.v..>..>v.>..v.>..v>.v..>vv.>>.>.>vv.v>>>>>v..>v...v.....v>>..>v.v>..>...v>vvv>v..>>>>.....v..
>...>v.>vv...>.........>>>..vv...>.>>...v>...>v>..>>>...>v.>.....>v>>.....vv.v..v>..>>v.vv.v.vv.v..vvvv.>.>.>......>.....v>.>v>.v...>v..vv.
..v.v>.>.>>.v>....vv>...>.>>>..>>v>>.vv>..>.v>.>..>.>.v>v......>.v>>...>......>.v.v.......>.>.v>>>>..vv>>...v>vv>.>......v...>...v.v>..v..v
>>.....v>..>..v.>...>.vvv>vvvv.vv.>v.>.......v.>.>.v.......>...>>>>.v>>.v...vv>.vvv.v..>.v.v...>.v.vvvv...vv.v..>>..v..>vvv.v.>..>.>..v>>>>
v..vv.>v>...v>.v.>.>>..v>.>.>.>>v>>v....vv.v...v..vvv.vv>v...>>..v..v..>..>..>v..vv....>>>>.>v..v...v..>>.>..>.v..>v>>v..vvv>>...v>v..>>v..
>vv.....v>..v>v....v......>.>>.v>v.>v>.>.vv>v>>>.>vv>.>.v>v.v.>v..v....v>v.>.>....v>>>.>....v..>v....>>>v.v.....>.>..v.v.>.>>>vv....v.>..vv
.v.>.v...>..v..v>.v>..>v......>vv.v...v>vv....>.v>>>.>vvv..v.v.>.>.>v>.v.v...v>...v>..v..>>.v.>..>v.v>>v..vv.vvv>>v>v>v>.>.....>.>>..>.v.>.
.>>.>>v>vv.>>..>v..>v.vvv.v.>..>.>..>.>..v..>.v>v>.>v.>.v...vv..>.v>....v.v..>.>....v>v.>.vv>vv...v.v>.>..>>.>>...>vv.>.v.v.>..v...>>v.vv>.
>>.>v...>..>...v....vv..........v..v.v.>v>>v>...v.vv.v.v....vv..v>.v...>.v>.vv...>v....>>.vv.........vv.>..vvv..vv.vvv..>>..>v.>vvv..>.v..>
..>v.>..vv..v>>>>>...>>v.v.v..v>>v>..v....>>.v>..vv....>>v.>>vv.v..>..v.v>v....v.>.....>>....>v..>.v..>.>v.vv....v...v.vv>.v>.>...v>.v.>v>.
>.v....>.>.>..v.vv...v>>v...v..v..>>.>......>v..v.>v.>>.v..>v>>..>...>.>v.>..>>.v>vv>...>....>..v..v.vv.vvvvv..v.>vvv>v.>...v>>v>v.>>...>.v
>.v..>..>..v.v.v>v.vv..v.v....vvv>v>>..>....v.v>>..v>....>.>v.>.v....>.>.vv.>...vv>>vv...v...>.v..v>.v...>v>vv>v>..>>.v>.>.>.v>.v>vv..>v>>.
.vv>..v>>v...>.>vvv.v.>.v..>>>>.>v>v>..vvv.>.v>.>...v....v..vv>>..>.>..>.>v.>.>v>vv.>>>vv.>....v.v.vv>v...v..>........v...v>.>vvvv.....v...
.>.v.>....>..>.>........v>.v>>v..>.v.>v.v..v>...v.v..v>vv.v..v>v>.....>>v.......v>v>v.v.>.....vv..vv...>v..>>>.>..v>vv.>v...>.>..v>vv.>>v..
.vv.>...v.>...>..>....v...>>>.......>>......>vvv..v.v..>v..>.>>.>.v...v...>..>>..>v..vv...>..vv........>>..>...vv>.>.v.v.v.v...v.v.>.v.....
.vv..........>v>..v.>.....v.>.v..>v....>..v..v>>..>.>.v....>..>>.vv>.vv.v.v.>.v..>>..vv..>.vv>..>.>>.>......v.>..>>>vv>>.>.v.vv>...>>v.v...
.v>vv.v.vv......>vv.v>.>.>>v>v....>>v...>.v..>..vv>>.v.....>>>..v..>v.v.>.v>>>.vvv.>.>..v.v..>.>...>.>>vv.>.>v.....v.vv.vvv>...v..v>vvvvv..
vvv..v>..>...>>>v>v..v........>...>.>v>>>v>>>>.>>.>.....>>.>v>.>v..>....>>.>..v>v>>.>v...vv..>v.>>v>..v.>v.v.>.....v.>vv.vvv>.vv>>..v.v..>>
v>......>>v>>.>.>..>..>.>.>v>>>v>..>>>>>vv..>v>....v...v......>vvv>.>....vv>>>..vv.>...v>>v....>>......>..v.v>v.>>.....>..v.>.v..>v.vvvvv..
.>>v>v...v....v.v.>>.>.vv...vvv.>>..vv..>...v..v>............>.>>.v>v.>.vv.>...>.....v.v...vv.v.v...v>v>v.v>.>v>v>>v>..>.>v....v.>..>v.v...
...v.v....>...v.>.....vv..>..v..>>.>.>.....v.v.v>.>.vv.>...v..>......v>..vv.vv.>.>v..>.>.>v>.v.>.>.>.....>..>v...>.v>v..>....v.>.v>..v.>>..
v>vvv>>..v.>>>.>>..v>.v>.v....v.v....v...vv.....vvv>.>........>>>..v>>vv...v..v.>>>.vv.v....vvvvv>..v..vvv>..>.>vvv.>....v..v.>v>.v.vv...>.
v>v..vv....>.>v.v.>..v..v.........>.v.v>.v.>.v.v.>vv.>>>v.>>.>.v..v....>vv>.>v.....>.>>...v>....v...>>v.vv.....v>v.vv.>..v>.v..>...v>vvv...
....v>>>>..vv>>v..vvvv.>..vv..v.>>.v>vvv>.vv..>....>.>.v...v>v>...>>>.v......>..........>>v.v..>v>vv...v...>.>>..v>v>.vv...v...>....>>>v..v
.......vv>v.>>v>v.>............>v>v>vvvv>>>...vv..>v.......>.vvv>.......v>.>vv..>>.>...v..........v...>v>v>>..v..v.v..>>>>..>..>.>>>.v....>
.>vvv>.....v...>v>v.v>...v..>vv...vv..v>vv...>.v.vv.v>.v..>v..v..>vv.>...v.v>v.vv.>v>>>....v>.>v..>..v.>>vvv>..vv...>...vv.v..vvv>...v.v.>v
..v>.v.v.>>>...>.>v.vvv...>>v.....>.>.>v.>v.>...>v>vv>>.v>>.>....>>v..vvv..>.v.vv..>>>.vv..v.>.>..>.......>>..v.v>v>v.vv....>vvvvvv..>>>>vv
...v.>.v.vv..>vv...>v...>>.vvvvv.vv.>v.>>..v.>>..>>.......>.v......v>v>>.>v>.>....v....>..>v.v>.>.>>v.>v..>..>.vvv.>.vv>..v>>.....v.>.v..>>
....vv.vv.>.v>v........vv.>>v>....v.>>..>..v>.>v.....v...vv..v>.......v.....v>>>.>.v>.v..>.v....>.v>v>.....v>>v.>..>.vv>vv>v.>>....v>.>..v.
v.>>>>v>.....>.>v.>.>vv..>v.vv..v.>.>v>vv.vv>..>.>....>..v>....>.>>v.v..>.>>v>........>.vv>>vv........>v.v>.>..v...v....v..>v>>.v>>.v...v>.
...v.>>.>.v>...>.....>v>...>>.......>>>.v.vv>...>...>v>v>.>.....v.>..>>vv..>vvvv.>...>..v>.v..vv..>>..>.>.>.vv..>vvv..>.>>..v...>.....v.>.>
v>.v>..v.v...>.v..v..>.>>.>v>vv.>.vvv.....>v>.vv....vv...v>v>>>.>>v.v....v.....v.v..v...vv>v..v....vv.>v..>>v....>v..>v>v.>vv..>v..v>v.v>>.
v......v..v...v..>.v>..........>.vv...v.>v.>..>v..v.v.>.vv.>...>>....v>v>>.>>.>v..vv.v......>.>>.vv.....>>v>vv..>..>v>...vv>....v>.>.>.>.v.
.v>>>..v>.>.....>.>.v.>v>>..>v.>.....>.>.>.vv..v>.v....v>>v..v.v...vv>...>v..v.>v......v..v.>>..>.>v..>v.v>vv>v...v>.>>.v>vv>..v..v..v>>...
....>v..vv..>>>v.v>....>v...>vv>.>.>.....v....vv>.>vvv.>.>v>>v.>.v.>>..>..>>vv..v>.v>>.....>>.v>v.v>.v..>.vv.>vv>>.v..>.....>>..vv>>....>v.
v...v..vv..>v.v..v....v.v.v>>..>v.v.v...>>>v>.>vv.v>....>v>.v...v......v..>vv>.v...>.>v..v>v>...vv.>>>....>>>v.v>>>v.v>vv.vv.....>>.v...>.v
>.v...vv>v....>v.>.>>.v.>.>v.vv>>>>>vv>v..>.v>>v>...v>v.v.....v....v..>...>.>..>vv>..v.>v...v..v..>>.v>>>...>v.v>>..v>....>>.>v>..v.v>>.>>.
..v>.v.v>vv.>>.>vvvv..v>.v..>.>.>>>>>vvvvv..>..>.>.>...vv.....>...>...v>.vv>>..>vv..v.>..v..>....v.v...vv.>..>>.>>>..v.>>.v.v>>.>.>..>>v...
v.>.v>v>....>vvv>>>.vv.v.v.>>...>v.v>..>..vv..>.>>>....>.v>.>>vv.>>>v..v>v..vv.>>.v>>.>>vv.v>v.>.v>.v>..>.v...>>>....>vv..>.>>v.v>..v.v.v>v
v.>v..>.v..>v..v>vvv>vvvv....>v.v..v..v>.>>>.v..>...v.....>..>v>.>..v....>..>.>.v>.......v...>....>.>.v>.vv.v>.....>>.....>.>>>.>.>v>vv.>>.
..>>.v.v.>..v..v>.>>....>v.v>...v.>.v.>..v.v..>.......>..v>.>.v.>...vv>v>v>..v.>.....>v..>..vv...>v...v.....>.>..v..v.>v>.v.v.vv.vv>v..>>.>
..v....>v..v.v..>...>.v>.vv>v>v>vvv..>..>vvv..v>...vv.>v>>...>.v...vvv.>>.v...v>.>........v>..>.>.v..v>....>>.>.>...v.v..>>>>v>vv...v...>>v
......v>........>vv>>....vv.>.......vv......v>vv.v..>..v.>v...v..vv.>......v...>..>.>>vvvv.v...>.v....v....v>.v.....v.>>v>..v.v>>>..v.>v...
..>v>.>>>.v.v>>>>>>v......vv....vv>v>v..v.v.......>.vv.>.v>..>.............vv.>v>..>v.....>>vvv.v>vv.......v..v.v.v..v>v.>.>v..>>.>>v.vv.v>
v.>.v..>>..>>v..v>.>.>>.v.>>v>.>>..v>.v.vvv.v>.vvvv.>>v.vvv...v..v.>v....v>.v>v........>v>>.vv.v.>..>..>v.>..>..>.>>>>>..v..>.v....>...>..>
..vvv>..v..>v>>...v.v...>v.>.>v.v.....v..>.>v>v.v.>..vv.vv>>.>v.v>v.>..v>v>..v>v.>.>>v>...>v>>>.>..>.>>v>v...vv.>v>>>..>.v>...>v>.v.>>.>vv.
v>v....v...v.>vv.>>>.>>>.>vvv..v.v.vv......v.v...>v..>.v>>>...>.....>....>.v>...vv.....>...v.>....>v.vv...>..v..>...>..vvv>.>>>..v>.>>>>>..
v..v>.vv.>.v...v.>v.vvv....v>v>>>.v.vv..v....>v..v.v>.>v.>>v..>....vv......>vv.vvv..>v>v.>v.v>v.v.>v.v.....v>.>...>>.>v>..>.v>v.>..>.v.v..v
.>>.v...v.v>..v>.v.>v....>..>.v.v..>...>v>..>.>..v........v>.v>vvv.vv..>v>.....>..v..v.>v.v....>v.v.>>.>.vvv.vv.>.vvv>.v>...>v.>v>..vv.>...
v>.v..>.>vv.>v>...>..vvv.>>.v>>vvvv.v.>.vv>.v..v.>v.v.>...v>v..v.v>vv.v>>.>.>......>.>>>v>v>.........vv>vv.>>vvv...>>v>.>.>>...>v....>...v.
...v...>v>v.v>>v>vvv...v>>...vvvv>..v.>>v..>.v>..>v>v..>..v.v>.>..vv>..>.vv....v>v>>>v>.v.v.v>.vv.....>>....>..>>.>v.>v>v.>.>>...>vv.v..v..
...>.v>...>...>vv...vv>..vv.>.vv...v>vv>vv..v>v>>v>>>>.v>v.>v....vvv.vv....>>v>...v..vvv..>....v.v.>>.v.>>..vv.....v>>v>.v...vvv>.....>v..v
.>.>>v.v.v>....vv.>>.>v.>.....v...>vv..>>.>..>v.>...vv..>.>>.v>>......v>.>.v>>...>.v.v>>..v.vv.vv....>.vv>>>v.>......>>.>.v>vv>...vv>>>>.vv
.vv..vv...v>v..v.v..vv.>..vvv.vvv...>.>>v>>vv.v>..>...v>...vvv.>.>.>.>>>>>.>.>.v..v.v.....v..>>>>v......vv.v...vvv........>.vv..>>>..v>..>.
....v.>v.v>.v.>....v>>v>.v..>.>.v>..>v..v.v>.....v.vv..>.>v>...vv>>...v>.....vv>...>>>......>>v.>>.>.>...>v.>....v>..>.>>....v>vvvv.>.>.>..
>v.....>>.v.vv.v.>>v>v>.vv>>v>vv...>.>.vv>..vvvvv>>...v>>v.v..v..>.>.>>.v..>.....v>v.v..>>v>v.v>.>...v>.v...>>...vv.v>vv...>>>......>>...v.
...>...>.v.>>v..>....>v.v.>>..v..vv..>.v>.v.v.>.v>>..v>>.>.v.>vv>..>........v>...>>.>>>>v.>v.v..v.v>.>>>v.>>..v....v.v..>.>>v>.>.>...v>.>>.
>.v.>...>>..>v>>v.vvv>..v.v.v.....v.>...>vv.>.>vv.v.....>>>vv....>v>v>v.v>.v>v.v>..>>...v..>v>.v.vv>.v.v.vv...v.v>v.vvvv..>vvv>.>.vv.>v...>
v.>v......v.v.v....>..v.v>>..v...>...>>v..vv.>.vv>>v>>vvvv>>.v..v.v.v.v....>v>..vv.>...v..>>>v>..v.>.>v..>.v.>>..v.vvv.v...v>...>...>>..v..
.>v>.>.....>>...v.>.>.vv..v>>.v..>.v..>vv..v..>v>>..>vv..>v.>.>....v...v..>v..>.>>>vv.....vv...>.v>.>.....v>...>>..v.v.>..vv.v...>>...v.>.>
...>>..v..>..vv.>.>>v>..>..>.....>.v>v..v.>>>..v..v>.>>...>>..v>v.>>.>..v..v.v.v>vv>.v...v>.v..v>.>.>..>.>.>.>....v...v>...>..>v.>....>.>v.
v..v.v.vv.>...>v..>..v.......vv>.>>.>.v>.......>..v.>v.v>>.vvv...>>.>..>..>>.>..>v>....vv..v..v>v.>vvv..v.v.>....v..>v.......v......v>v..v>
v>.v....>v..>.>v>v..v..>v.vv.>v.v.>.v..>v.>vv>>..>v>..v>v..v.>..vv..>vv>.v.......>..v.v>>v.v.....>>vv....v..>.v...v.v>..>.>.....>v....v...v
..>.v....>>.>..>.>>v.>.v..>v...v>.>..>.vv>.>vv>v>.>>v>..>.v.>.>..........>.>vvv...v..v.v.>>.>>>....v>..>.vv...v...v.vv>v.>v>..vv..>>>>>vvv.
...v.>.>v>.>>...>v.>>v.......>.>>v>v...v>vv....v.>....>.>..v.vvv.>..>.vvvv.v.v.>>.>.vvvvv..v...vvv.v.>>v>.v>>v.>v..v.>v>v..vv>...v>.v.v...>
v.v....>v>..>>.>>....vv...>..v..>>..>.....v>.>v>..>....>>>vv>>.vv...v....>..vv.v>>..>.v.>.v>.v>>.....>v....vv>..>..vv.>.v.v>..vv..>v>.v.>>.
.v.vv>....vv.>v>v.>.v.>>v.>>v>.v>..v.......>..>>>vv.vv....>>....>v.vv.v..vv..>.>..v......v.v>>v>v>..vv.v.vv.v.v.>>v.>.v..>...vv>......vv>.v
.vv>..v...vv>>v>....vvv..>v>..>..v.....vv....>..>v.v..v>.>>.v>v..>.v>v.vvv.v.>>..>......>v.v..vv.>..vv>>.vvv>v>.v>.>.........>v..vv>>..vvv.
.v.>v.vv.vv.vv.>>>vvv.v..v..>>v>.vv..v>.v>.....v>v..>v>>>>v>..vv..v...v>..>.v..>...v..vvv...>..v.>>..v.v...>.v>vvv..>.v.>>..vv>>v..v.vv.>.v
>>...v.>.>..>v>.>>.>v.vv>....v.>v.>>>.vv.>>.v.v...v>.v>>.v...>vv>>vvvv.>...>v>..>...>.>v..>.v..vv>....>>.vv>..>.v.....>v.v..>.>.>.>..v>..v.
.v>>.v........>v>vvv.>>.>vvv>>v.>.v>..>....v>vv...>.>v>.>>>>v.>>....vv>v.vv>.v........>.v>.....v..>>v.v.........>v>.>>.vv.>>.v...vvv>v.>.v.
>..>..>..vv.>>>>v...>>>vvv.>>.>v>..>...v..v..>>>v...>>.>.v...>.>...>...v>>vv.v.....>.v>v.vv>v..v.v.v>..>..v.>>.v.>v>>.v.v>....v.>..>v>vv...
...>>..>>vv..v>v>>vv..>vv..v>....vv...>v..>v...vv.v....>>>......v>v>>..v..>..v>>.>vv>v..>..>v..v>>v>...v>.>...>.v>..>.vv>v>....vvvv.v..vv..
.....v>v>>>v.v..v>v.>.v.>>>.>.>>>..v.>.v.v>vv>..>.vv.v>...vv..v.>v....v..>vv.>.vv>v.>>>v>v....>v>>v...>v.>v..vv..v>.>v..>.v.v.>..>......v..
>>....v>v>v..>>v......v.>...vvvv.v.....v..v.>>.>.vv.>>>..v>.>v>v>>........>vv.>v..v..>.>v.>>...v>>.v>v..>.v..>>.v>..>>>.v.>.v.v>.v.vvvvv...
v>...v>..vv.>v..v....>..>.>>.>>.vv..>>>.>>..v.>v.vv...>>v.v.>.v..v.>...>>...vv..>v.v.>...>>...>>.vvv...v...>v>....vv....>...>..v.v..>>.v>v.";
}