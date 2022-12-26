using System;
using System.IO;

public enum Offset
{
    N = 0,
    E,
    S,
    W
}

public class PositionBase : IEquatable<PositionBase>
{
    public long X;
    public long Y;

    public PositionBase(long x, long y)
    {
        X = x;
        Y = y;
    }

    public PositionBase((long x, long y) coord)
        : this(coord.x, coord.y)
    {
    }

    protected PositionBase()
    {
    }

    public bool Equals(PositionBase? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is PositionBase @base) return Equals(@base);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }

    public void Deconstruct(out long X, out long Y)
    {
        X = this.X;
        Y = this.Y;
    }

    public static implicit operator (long, long)(PositionBase p)
    {
        return (p.X, p.Y);
    }

    public static bool operator ==(PositionBase left, PositionBase right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PositionBase left, PositionBase right)
    {
        return !left.Equals(right);
    }
}


public class Position<TSubType> : PositionBase, IEquatable<Position<TSubType>>
    where TSubType : Position<TSubType>, new()
{
    public static readonly Position<TSubType> North = new(0, +1);
    public static readonly Position<TSubType> East = new(+1, 0);
    public static readonly Position<TSubType> South = new(0, -1);
    public static readonly Position<TSubType> West = new(-1, 0);
    public static readonly Position<TSubType> NorthWest = North + West;
    public static readonly Position<TSubType> NorthEast = North + East;
    public static readonly Position<TSubType> SouthWest = South + West;
    public static readonly Position<TSubType> SouthEast = South + East;

    public static Position<TSubType>[] Offsets =
    {
        North,
        East,
        South,
        West
    };


    public static Position<TSubType>[] AllOffsets =
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    };

    public Position((long x, long y) coord)
        : base(coord)
    {
    }

    public Position(long x, long y)
        : base(x, y)
    {
    }

    protected Position()
    {
    }


    public Position<TSubType> PosNorth => this + North;
    public Position<TSubType> PosSouth => this + South;
    public Position<TSubType> PosWest => this + West;
    public Position<TSubType> PosEast => this + East;
    public Position<TSubType> PosNorthWest => this + NorthWest;
    public Position<TSubType> PosNorthEast => this + NorthEast;
    public Position<TSubType> PosSouthWest => this + SouthWest;
    public Position<TSubType> PosSouthEast => this + SouthEast;



    // Conversions
    protected virtual TSubType FromBase<TSub>(TSub pos)
        where TSub : Position<TSubType>
    {
        return pos.FromBase<TSubType>(pos);
    }

    public static implicit operator Position<TSubType>((long, long) tuple)
    {
        return new Position<TSubType>(tuple);
    }

    public static implicit operator TSubType(Position<TSubType> tuple)
    {
        var x = new TSubType
        {
            X = tuple.X,
            Y = tuple.Y
        };
        return x.FromBase(x);
    }


    // Math
    public static Position<TSubType> operator -(Position<TSubType> p1, Position<TSubType> p2)
    {
        return new Position<TSubType>(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static Position<TSubType> operator +(Position<TSubType> p1, Position<TSubType> p2)
    {
        return new Position<TSubType>(p1.X + p2.X, p1.Y + p2.Y);
    }


    // Equality

    public bool Equals(PositionBase other)
    {
        return X == other.X && Y == other.Y;
    }

    public bool Equals(Position<TSubType>? other)
    {
        return X == other?.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is PositionBase other && Equals(other);
    }

    private int _previousHash = 0;
    public override int GetHashCode()
    {
        var hashCode = HashCode.Combine(X, Y);
        if (_previousHash == 0)
            return hashCode;
        if (_previousHash != hashCode)
            throw new InvalidDataException("HashCode was used and changed");
        return hashCode;
    }
}

public class Position : Position<Position>
{
    public Position(long x, long y) : base(x, y)
    {
    }

    public Position(Position pos) : base(pos.X, pos.Y)
    {
    }

    public Position()
    {
    }


    protected override Position FromBase<TSub>(TSub pos)
    {
        return new Position(pos.X, pos.Y);
    }
}

public class LocalPosition : Position<LocalPosition>
{
    public LocalPosition(long x, long y) : base(x, y)
    {
    }

    public LocalPosition(LocalPosition pos) : base(pos)
    {
    }

    public LocalPosition()
    {
    }

    public static implicit operator LocalPosition((long x, long y) pos)
    {
        return new LocalPosition(pos.x, pos.y);
    }

    public static explicit operator Position(LocalPosition pos)
    {
        return new Position(pos.X, pos.Y);
    }

    public static explicit operator LocalPosition(Position pos)
    {
        return new LocalPosition(pos.X, pos.Y);
    }

    protected override LocalPosition FromBase<TSub>(TSub pos)
    {
        return new LocalPosition(pos.X, pos.Y);
    }
}

public class GlobalPosition : Position<GlobalPosition>
{
    public GlobalPosition(long x, long y) : base(x, y)
    {
    }

    public GlobalPosition()
    {
    }

    public static explicit operator Position(GlobalPosition pos)
    {
        return new Position(pos.X, pos.Y);
    }

    public static explicit operator GlobalPosition(Position pos)
    {
        return new GlobalPosition(pos.X, pos.Y);
    }

    protected override GlobalPosition FromBase<TSub>(TSub pos)
    {
        return new GlobalPosition(pos.X, pos.Y);
    }
}

public class SidePosition : Position<SidePosition>
{
    public SidePosition(long x, long y) : base(x, y)
    {
    }

    public SidePosition()
    {
    }

    public static explicit operator Position(SidePosition pos)
    {
        return new Position(pos.X, pos.Y);
    }

    public static explicit operator SidePosition(Position pos)
    {
        return new SidePosition(pos.X, pos.Y);
    }

    protected override SidePosition FromBase<TSub>(TSub pos)
    {
        return new SidePosition(pos.X, pos.Y);
    }
}