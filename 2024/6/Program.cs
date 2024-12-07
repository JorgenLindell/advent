using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection;
using common;

var data = StreamUtils.GetLines();
////data =
////    @"
////....#.....
////.........#
////..........
////..#.......
////.......#..
////..........
////.#..^.....
////........#.
////#.........
////......#...
////".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

char[,] matrix = new char[data.Length, data[1].Length];
data.ForEach((x, ix) => x.ToCharArray()
    .ForEach((y, iy) => matrix[ix, iy] = y));

var guard = new Mover(matrix.First('^'), Mover.Dir.U);
var initial = guard.Pos.KeyTuple;
var guardLocations = new HashSet<(int r, int c)> { initial };

while (guard.NextPos().IsInside)
{
    if (guard.IsNextFree)
    {
        guard.Move();
        if (guard.Pos.IsInside)
        {
            guardLocations.Add(guard.Pos.KeyTuple);
        }
    }
    else
    {
        guard.TurnR();
    }
}
Console.WriteLine(guardLocations.Count);

guardLocations.Remove(initial);
var loopPaths = new Dictionary<(int r, int c), HashSet<(int r, int c, Mover.Dir)>>();

foreach (var obstaclePos in guardLocations)
{
    matrix = new char[data.Length, data[1].Length];

    data.ForEach((x, ix) => x.ToCharArray()
        .ForEach((y, iy) => matrix[ix, iy] = y));

    guard = new Mover(matrix.First('^'), Mover.Dir.U);

    matrix[obstaclePos.r, obstaclePos.c] = '0';
    HashSet<(int r, int c, Mover.Dir)> path = [];

    while (guard.NextPos().IsInside)
    {
        while (!guard.IsNextFree)
        {
            guard.TurnR();
        }

        var nextPos = guard.NextPos();
        if (path.Contains((nextPos.KeyTuple.r, nextPos.KeyTuple.c, guard.Direction)))
        {
            loopPaths.Add((obstaclePos.r, obstaclePos.c), path);
            break;
        }

        guard.Move();
        if (guard.Pos.IsInside)
        {
            var r = guard.Pos.KeyTuple.r;
            var c = guard.Pos.KeyTuple.c;

            //  if (guard.Direction.In(Mover.Dir.L, Mover.Dir.R) && matrix[r, c] == '|') matrix[r, c] = '+';
            //  if (guard.Direction.In(Mover.Dir.U, Mover.Dir.D) && matrix[r, c] == '-') matrix[r, c] = '+';
            //  if (matrix[r, c] != '+')
            //  {
            //      if (guard.Direction.In(Mover.Dir.L, Mover.Dir.R)) matrix[r, c] = '-';
            //      if (guard.Direction.In(Mover.Dir.U, Mover.Dir.D)) matrix[r, c] = '|';
            //  }

            path.Add((r, c, guard.Direction));
        }
    }
}
Console.WriteLine(loopPaths.Count);









return;

public class Mover
{
    public enum Dir
    {
        U,
        R,
        D,
        L
    }

    public static char[] Symbols = ['^', '>', 'v', '<'];

    public static Dictionary<Dir, ((int ro, int co) offs, char symbol)> Directions = new()
    {
        [Dir.U] = ((-1, 0), Symbols[(int)Dir.U]),
        [Dir.R] = ((0, +1), Symbols[(int)Dir.R]),
        [Dir.D] = ((+1, 0), Symbols[(int)Dir.D]),
        [Dir.L] = ((0, -1), Symbols[(int)Dir.L]),
    };


    public MatrExt.MatrPos<char> Pos { get; set; }
    public Dir Direction { get; set; }
    public char Symbol => Symbols[(int)Direction];

    public Mover(MatrExt.MatrPos<char> first, Dir dir1)
    {
        Pos = first;
        Direction = dir1;
    }

    public void TurnR()
    {
        Direction += 1;
        if (Direction > Dir.L) Direction = Dir.U;
    }

    public void TurnL()
    {
        Direction -= 1;
        if (Direction < Dir.U) Direction = Dir.L;
    }

    public bool IsNextFree => GetNextValue().In(".X+-|^v<>".ToCharArray());

    public void Move()
    {
        Pos = NextPos();
        Pos.Value = Symbols[(int)Direction];
    }
    public char GetNextValue()
    {
        var pos = NextPos();
        return pos.IsValid ? pos.Value : '\0';
    }

    public MatrExt.MatrPos<char> NextPos()
    {
        var pos = Pos;
        pos = Direction switch
        {
            Dir.U => pos.U,
            Dir.R => pos.R,
            Dir.D => pos.D,
            _ => pos.L
        };
        return pos;
    }
}