using System.Diagnostics.CodeAnalysis;
using common;

var data = StreamUtils.GetLines();
////data =
////    @"
////.M.S......
////..A..MSMS.
////.M.S.MAA..
////..A.ASMSM.
////.M.S.M....
////..........
////S.S.S.S.S.
////.A.A.A.A..
////M.M.M.M.M.
////..........
////".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

char[,] matrix = new char[data.Length, data[1].Length];
data.ForEach((x, ix) => x.ToCharArray().ForEach((y, iy) => matrix[ix, iy] = y));

var delta = new int[] { -1, 0, 1 };
var directions = delta.SelectMany(x => delta.Select(y => (row: x, col: y))).Where(x => x != (0, 0)).ToArray();

char[] xmas = "XMAS".ToCharArray();

var tot1 = 0;
var tot2 = 0;
for (int r = 0; r < matrix.GetLength(0); r++)
{
    for (int c = 0; c < matrix.GetLength(1); c++)
    {
        if (HasCross(matrix, r, c, "MAS".ToCharArray()))
            tot2++;

        tot1 += directions.Count(dir1 => HasMatch(matrix, r, c, dir1, xmas));
    }
}

Console.WriteLine(tot1);
Console.WriteLine(tot2);
return;

bool HasCross(char[,] matr, int r, int c, char[] mas)
{
    var a = mas[1];
    if (matr[r, c] != a) return false;

    var m = mas[0];
    var s = mas[2];
    
    var pos = matr.Pos(r, c);

    return (   (pos.LU.Value == m && pos.RD.Value == s)
            || (pos.LU.Value == s && pos.RD.Value == m)) 
          && ((pos.LD.Value == m && pos.RU.Value == s)
           || (pos.LD.Value == s && pos.RU.Value == m));
}


bool HasMatch(char[,] matr, int r, int c, (int row, int col) dir, char[] match)
{

    if (match[0] != matr[r, c]) return false;

    for (int m = 1; m < match.Length; m++)
    {
        var dirRow = r + dir.row * m;
        if (dirRow < 0 || dirRow >= matr.GetLength(0))
            return false;

        var dirCol = c + dir.col * m;
        if (dirCol < 0 || dirCol >= matr.GetLength(1))
            return false;

        if (matr[dirRow, dirCol] == match[m])
            continue;

        return false;
    }
    return true;
}

public static class MatrExt
{
    public class MatrPos<T>(T[,] matr, int r, int c)
    {
        public T? Value
        {
            get
            {
                if (r < 0 || c < 0 || r >= matr.GetLength(0) || c >= matr.GetLength(1))
                    return default;
                return matr[r, c];
            }
        }

        public MatrPos<T> LU => new(matr, r - 1, c - 1);
        public MatrPos<T> LD => new(matr, r + 1, c - 1);
        public MatrPos<T> RU => new(matr, r - 1, c + 1);
        public MatrPos<T> RD => new(matr, r + 1, c + 1);
        public MatrPos<T> L => new(matr, r, c - 1);
        public MatrPos<T> R => new(matr, r, c + 1);
        public MatrPos<T> D => new(matr, r + 1, c);
        public MatrPos<T> U => new(matr, r - 1, c);
    }

    public static MatrPos<T> Pos<T>(this T[,] matr, int r, int c)
    {
        return new MatrPos<T>(matr, r, c);
    }
}
