using System;
using System.Collections.Generic;

namespace common;

public readonly struct Limits<T> : IEquatable<Limits<T>> where T : IComparable<T>
{
    public bool Equals(Limits<T> other)
    {
        return EqualityComparer<T>.Default.Equals(upper, other.upper) && EqualityComparer<T>.Default.Equals(lower, other.lower);
    }

    public override bool Equals(object? obj)
    {
        return obj is Limits<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(upper, lower);
    }

    public static bool operator ==(Limits<T> left, Limits<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Limits<T> left, Limits<T> right)
    {
        return !left.Equals(right);
    }

    public readonly T upper;
    public readonly T lower;
    public Limits((T lower, T upper) p)
    {
        var p2 = p.Order();
        lower = p2.lower;
        upper = p2.upper;
    }
    public Limits(T lower, T upper)
        : this((lower, upper))
    {
    }
    public Limits(Limits<T> p)
    :this((p.lower,p.upper))
    {
        
    }

    public static implicit operator Limits<T>?((T, T)? p)
        => p == null ? 
            null : 
            new Limits<T>(p.Value.Item1, p.Value.Item2);
}