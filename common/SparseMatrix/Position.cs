using System;
using System.Runtime.InteropServices;

public enum Offset
{
    N = 0,
    S,
    W,
    E,
};
public struct Position:IEquatable<Position>
{
    // ReSharper disable InconsistentNaming
    public static readonly Position North      = new(0, +1);
    public static readonly Position South      = new(0, -1);
    public static readonly Position West       = new(-1, 0);
    public static readonly Position East       = new(+1, 0);
    public static readonly Position NorthWest = North + West;
    public static readonly Position NorthEast = North + East;
    public static readonly Position SouthWest = South + West;
    public static readonly Position SouthEast = South + East;
    // ReSharper restore InconsistentNaming

    public static Position[] Offsets = new Position[]
    {
        North,
        South,
        West,
        East,
    };

    public static Position[] AllOffsets = new Position[]
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
    };

    public static Position operator +(Position left, Position right)
    {
        return new Position(left.X + right.X, left.Y + right.Y);
    }

    public long X;
    public long Y;

    public Position PosNorth =>    this + North;
    public Position PosSouth    => this + South    ;
    public Position PosWest     => this + West     ;
    public Position PosEast     => this + East     ;
    public Position PosNorthWest=> this + NorthWest;
    public Position PosNorthEast=> this + NorthEast;
    public Position PosSouthWest=> this + SouthWest;
    public Position PosSouthEast=> this + SouthEast;

    public Position(long x, long y)
    {
        X = x;
        Y = y;
    }

    public Position((long x, long y) coord)
        : this(coord.x, coord.y)
    { }

    public static implicit operator (long, long)(Position p)
    {
        return (p.X, p.Y);
    }

    public static implicit operator Position((long, long) tuple)
    {
        return new Position(tuple);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }

    public bool Equals(Position other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Position other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Position left, Position right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Position left, Position right)
    {
        return !left.Equals(right);
    }
}