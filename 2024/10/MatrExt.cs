public static class MatrExt
{
    public static MatrPos<T> First<T>(this T[,] matr, T toFind)
        where T : IEquatable<T>
    {
        for (int r = 0; r < matr.GetLength(0); r++)
        {
            for (int c = 0; c < matr.GetLength(0); c++)
            {
                if (matr[r, c].Equals(toFind))
                    return matr.Pos(r, c);
            }
        }

        return matr.Pos(-2, -2);
    }
    public static string DebugPrint<T>(this T[,] matr)
    {
        var str = "";
        for (int r = 0; r < matr.GetLength(0); r++)
        {
            for (int c = 0; c < matr.GetLength(0); c++)
            {
                str += matr[r, c];
            }
            str += "\n";
        }

        return str;
    }
    public class MatrPos<T>(T[,] matr, int r, int c)
    {
        public T? Value
        {
            get
            {
                if (!IsInside)
                    return default;
                return matr[r, c];
            }
            set
            {
                if (IsInside)
                    matr[r, c] = value ?? default;
            }
        }

        public (int r, int c) KeyTuple => (r, c);

        public bool IsInside =>
            !(r < 0 || c < 0 || r >= matr.GetLength(0) || c >= matr.GetLength(1));

        public bool IsValid => IsInside;


        public override string ToString() => $"[{r},{c}]";
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