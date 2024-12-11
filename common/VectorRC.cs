using System;

namespace common;

public record struct VectorRc(int Row, int Col) : IComparable<VectorRc>
{
    public static readonly VectorRc Zero = new(0, 0);
    public static readonly VectorRc Up = new(-1, 0);
    public static readonly VectorRc Down = new(+1, 0);
    public static readonly VectorRc Left = new(0, -1);
    public static readonly VectorRc Right = new(0, +1);

    public static VectorRc operator +(VectorRc left, VectorRc right) => new(left.Row + right.Row, left.Col + right.Col);
    public static VectorRc operator +(VectorRc left, (int, int) right) => new(left.Row + right.Item1, left.Col + right.Item2);
    public static VectorRc operator -(VectorRc left, VectorRc right) => new(left.Row - right.Row, left.Col - right.Col);
    public static VectorRc operator -(VectorRc val) => new(-val.Row, -val.Col);
    public static VectorRc operator *(VectorRc left, int p) => new(left.Row * p, left.Col * p);

    public readonly int Dot(VectorRc that) => this.Row * that.Row + this.Col * that.Col;
    public readonly VectorRc RotatedLeft() => new(-Col, Row);
    public readonly VectorRc RotatedRight() => new(Col, -Row);

    public readonly int ManhattanMetric() => Math.Abs(Row) + Math.Abs(Col);
    public readonly int ChebyshevMetric() => Math.Max(Math.Abs(Row), Math.Abs(Col));
    public readonly double FlyDistance( VectorRc end)
    {
        var b = Math.Abs(this.Col - end.Col);
        var h = Math.Abs(this.Row - end.Row);
        return Math.Sqrt(b * b + h * h);
    }
    public readonly int StepsDistance(VectorRc end)
    {
        var b = Math.Abs(this.Col - end.Col);
        var h = Math.Abs(this.Row - end.Row);
        return b + h;
    }

    public readonly VectorRc NextUp() => this + Up;
    public readonly VectorRc NextDown() => this + Down;
    public readonly VectorRc NextLeft() => this + Left;
    public readonly VectorRc NextRight() => this + Right;

    public readonly VectorRc[] NextFour() => new[]
    {
        this + Up,
        this + Down,
        this + Left,
        this + Right
    };


    public readonly VectorRc[] NextEight() =>
        new[]
        {
            this + Up + Left,
            this + Up,
            this + Up + Right,
            this + Left,
            this + Right,
            this + Down + Left,
            this + Down,
            this + Down + Right,
        };

    public override int GetHashCode()
    {
        unchecked
        {
            return Row.GetHashCode() ^ (Col.GetHashCode() << 1);
        }
    }


    public int CompareTo(VectorRc other)
    {
        var rowComparison = Row.CompareTo(other.Row);
        if (rowComparison != 0) return rowComparison;
        return Col.CompareTo(other.Col);
    }

    public override string ToString()
    {
        return $"<{Row},{Col}>";
    }

    public bool Inside(int c0, int r0, int c1, int r1)
    {
        return (Col >= c0 && Col <c1 && Row >= r0 && Row < r1);
    }
}
