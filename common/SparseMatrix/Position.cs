using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;

namespace common.SparseMatrix;

public enum Direction
{
    N = 0,
    E,
    S,
    W
}

public static class DirectionExtensions
{
    public static IEnumerable<Direction> Values()
    {
        yield return Direction.N;
        yield return Direction.E;
        yield return Direction.S;
        yield return Direction.W;
    }
    public static Direction Turn(this Direction start, int steps)
    {
        var length = (int)Direction.W + 1;
        var result = ((int)start + steps) % length;
        if (result < 0) result += length;
        return (Direction)result;
    }

    public static Direction Invert(this Direction start) => start.Turn(2);

}


public interface IPosition
{
    public long X { get; set; }
    public long Y { get; set; }
    bool Equals(object? obj);
    int GetHashCode();
    string ToString();
    void Deconstruct(out long x, out long y);
    bool Outside(IPosition min, IPosition max);
    bool Outside((IPosition min, IPosition max) limits);
    long ManhattanDistance(IPosition other);
}

public class PositionBase : IPosition, IEquatable<IPosition>, IEquatable<PositionBase>
{
    public long X { get; set; }
    public long Y { get; set; }
    public static bool NorthIsNegative { get; set; } = false;
  
    private int _previousHash = 0; // used to verify that X Y has not changed in getHashCode as that would fail dictionary lookups.


    public PositionBase(long x, long y)
    {
        X = x;
        Y = y;
    }

    public PositionBase((long x, long y) coord)
        : this(coord.x, coord.y)
    {
    }

    protected PositionBase(PositionBase pos)
        : this(pos.X, pos.Y)
    {
    }
    protected PositionBase()
    {
    }
    public override string ToString()
    {
        return $"({X},{Y})";
    }

    public void Deconstruct(out long x, out long y)
    {
        x = this.X;
        y = this.Y;
    }

    public Direction? ToDirection()
    {
        int directionV = Y switch
        {
            > 0 => (int)(NorthIsNegative ? Direction.S : Direction.N),
            < 0 => (int)(NorthIsNegative ? Direction.N : Direction.S),
            _ => -1
        };

        var directionH = X switch
        {
            > 0 => (int)Direction.E,
            < 0 => (int)Direction.W,
            _ => -1
        };

        if (directionV > -1 && directionH > -1)
            return null;

        if (directionV > -1)
            return (Direction)directionV;

        if (directionH > -1)
            return (Direction)directionH;

        return null;
    }


    public bool Outside(IPosition min, IPosition max)
    {
        return (X < min.X || X > max.X || Y < min.Y || Y > max.Y);
    }
    public bool Outside((IPosition min, IPosition max) limits)
    {
        return Outside(limits.min, limits.max);
    }
    public long ManhattanDistance(IPosition other) => Math.Abs(other.X - this.X) + Math.Abs(other.Y - this.Y);


//  public static PositionBase operator -(PositionBase p1, PositionBase p2)
//  {
//      return new PositionBase(p1.X - p2.X, p1.Y - p2.Y);
//  }
//
//  public static PositionBase operator +(PositionBase p1, PositionBase p2)
//  {
//      return new PositionBase(p1.X + p2.X, p1.Y + p2.Y);
//  }
//  public static PositionBase operator *(PositionBase p1, long factor)
//  {
//      return new PositionBase(p1.X * factor, p1.Y * factor);
//  }
//  public static PositionBase operator *(PositionBase p1, PositionBase factorPosition)
//  {
//      return new PositionBase(p1.X * factorPosition.X, p1.Y * factorPosition.Y);
//  }
//  public static PositionBase operator %(PositionBase p1, long factor)
//  {
//      return new PositionBase(p1.X % factor, p1.Y % factor);
//  }
//  public static PositionBase operator %(PositionBase p1, PositionBase factorPosition)
//  {
//      return new PositionBase(p1.X % factorPosition.X, p1.Y % factorPosition.Y);
//  }
//  public static PositionBase operator /(PositionBase p1, long factor)
//  {
//      return new PositionBase(p1.X / factor, p1.Y / factor);
//  }
//  public static PositionBase operator /(PositionBase p1, PositionBase factorPosition)
//  {
//      return new PositionBase(p1.X / factorPosition.X, p1.Y / factorPosition.Y);
//  }

    // Equality

    public bool Equals(PositionBase? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }


    public bool Equals(IPosition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is PositionBase other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = HashCode.Combine(X, Y);
        if (_previousHash != 0 && _previousHash != hashCode)
            throw new InvalidOperationException("hashCode has changed");
        return hashCode;
    }
    public static bool operator ==(PositionBase? left, PositionBase? right)
    {

        if (ReferenceEquals(left, right))
            return true;
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(PositionBase left, PositionBase right)
    {
        return !left.Equals(right);
    }

    // Conversion
    public static implicit operator (long, long)(PositionBase p)
    {
        return (p.X, p.Y);
    }

    public static implicit operator Vector2(PositionBase p)
    {
        return new Vector2((float)p.X, (float)p.Y);
    }

}



public abstract class Position<TSubType> : PositionBase
    where TSubType : Position<TSubType>, new()
{
    public static TSubType N => North;
    public static TSubType E => East;
    public static TSubType S => South;
    public static TSubType W => West;

    public static TSubType North { get; } = CreateSubtype(0, PositionBase.NorthIsNegative ? -1 : 1);
    public static TSubType East { get; } = CreateSubtype(+1, 0);
    public static TSubType South { get; } = CreateSubtype(0, PositionBase.NorthIsNegative ? 1 : -1);
    public static TSubType West { get; } = CreateSubtype(-1, 0);
    public static TSubType NorthWest { get; } = North + West;
    public static TSubType NorthEast { get; } = North + East;
    public static TSubType SouthWest { get; } = South + West;
    public static TSubType SouthEast { get; } = South + East;

    public TSubType PosNorth => this + North;
    public TSubType PosSouth => this + South;
    public TSubType PosWest => this + West;
    public TSubType PosEast => this + East;
    public TSubType PosNorthWest => this + NorthWest;
    public TSubType PosNorthEast => this + NorthEast;
    public TSubType PosSouthWest => this + SouthWest;
    public TSubType PosSouthEast => this + SouthEast;

    public static implicit operator Position<TSubType>(Direction dir)
    {
        return dir switch
        {
            Direction.N => North,
            Direction.E => East,
            Direction.S => South,
            Direction.W => West,
            _ => throw new ArgumentException()
        };
    }
    public static implicit operator (long, long)(Position<TSubType> p)
    {
        return (p.X, p.Y);
    }

    public TSubType Wrap(PositionBase min, PositionBase max)
    {
        var pos = CreateSubtype(this);
        var width_ = ((max.X - min.X) + 1);
        var height = ((max.Y - min.Y) + 1);

        if (pos.X < min.X) pos.X = min.X + (((pos.X - min.X) % width_) + width_);
        if (pos.X > max.X) pos.X = min.X + (((pos.X - min.X) % width_));
        if (pos.Y < min.Y) pos.Y = min.Y + (((pos.Y - min.Y) % height) + height);
        if (pos.Y > max.Y) pos.Y = min.Y + (((pos.Y - min.Y) % height));
        return pos;
    }

    public TSubType Wrap((PositionBase min, PositionBase max) limits)
    {
        return Wrap(limits.min, limits.max);

    }
    public static TSubType[] Directions { get; } =
    {
        North,
        East,
        South,
        West
    };
    public static TSubType[] AllDirections { get; } =
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

    public Position(PositionBase pos)
        : base(pos)
    {
    }

    public Position()
    {
    }

    private static TSubType CreateSubtype(long p1X, long p1Y)
    {
        var x = new TSubType
        {
            X = p1X,
            Y = p1Y
        };
        return x;
    }
    private static TSubType CreateSubtype(PositionBase pos)
        => CreateSubtype(pos.X, pos.Y);

    // Conversions
    public static implicit operator Position<TSubType>((long, long) tuple)
        => CreateSubtype(tuple.Item1, tuple.Item2);
    public static implicit operator TSubType(Position<TSubType> pos)
        => CreateSubtype(pos);


    // Math --------------
    // subtraction
    public static TSubType operator -(TSubType p1, Position<TSubType> p2)
        => CreateSubtype(p1.X - p2.X, p1.Y - p2.Y);
    // addition
    public static TSubType operator +(TSubType p1, Position<TSubType> p2)
        => CreateSubtype(p1.X + p2.X, p1.Y + p2.Y);
    // multiplication
    public static Position<TSubType> operator *(Position<TSubType> p1, long factor)
        => CreateSubtype(p1.X * factor, p1.Y * factor);
    public static TSubType operator *(TSubType p1, Position<TSubType> factorPosition)
        => CreateSubtype(p1.X * factorPosition.X, p1.Y * factorPosition.Y);
    // division
    public static Position<TSubType> operator /(Position<TSubType> p1, long divisor)
        => CreateSubtype(p1.X / divisor, p1.Y / divisor);
    public static TSubType operator /(TSubType p1, Position<TSubType> divisorPosition)
        => CreateSubtype(p1.X / divisorPosition.X, p1.Y / divisorPosition.Y);
    // modulo
    public static Position<TSubType> operator %(Position<TSubType> p1, long factor)
        => CreateSubtype(p1.X % factor, p1.Y % factor);
    public static TSubType operator %(TSubType p1, Position<TSubType> factorPosition)
        => CreateSubtype(p1.X % factorPosition.X, p1.Y % factorPosition.Y);


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

    public static IEnumerable<Position> RawNeighbors(Position position)
    {
        return Position.Directions
            .Select(o => (new Position(position + o)));
    }
}

public class Position : Position<Position>
{
    public Position(long x, long y) : base(x, y)
    {
    }

    public Position(PositionBase pos) : base(pos.X, pos.Y)
    {
    }

    public Position() : base()
    {
    }
}
