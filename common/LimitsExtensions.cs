using System;

namespace common;

public static class LimitsExtensions
{
    public static bool WithIn<T>(this T c, (T lower, T upper) p)
        where T : IComparable<T>
    {
        return WithIn(c, p.lower, p.upper);
    }

    public static bool WithIn<T>(this T c, T limit1, T limit2)
        where T : IComparable<T>
    {
        var (low, upp) = Order((lower: limit1, upper: limit2));
        return WithInOrdered(c, low, upp);
    }

    public static bool WithInOrdered<T>(this T c, T lower, T upper)
        where T : IComparable<T>
    {
        var comp1 = c.CompareTo(lower);
        var comp2 = c.CompareTo(upper);
        return comp1.In(1, 0) && comp2.In(-1, 0);
    }

    public static bool WithInOrdered<T>(this T c, Limits<T> p)
        where T : IComparable<T>
    {
        return WithInOrdered(c, p.lower, p.upper);
    }

    public static bool WithInOrdered<T>(this T c, (T lower, T upper) p)
        where T : IComparable<T>
    {
        return WithInOrdered(c, p.lower, p.upper);
    }

    public static (T lower, T upper) Order<T>(this (T lower, T upper) pair)
        where T : IComparable<T>
    {
        return pair.upper.CompareTo(pair.lower) == -1
            ? (pair.upper, pair.lower)
            : pair;
    }


    public static bool Intersects<T>(this Limits<T>? p1, Limits<T>? p2)
        where T : IComparable<T>
    {
        if (p1 == null || p2 == null) return false;
        return Intersects(p1.Value, p2.Value);
    }

    public static bool Intersects<T>(this Limits<T> p1, Limits<T> p2)
        where T : IComparable<T>
    {
        return p1.lower.WithInOrdered(p2.lower, p2.upper)
               || p2.lower.WithInOrdered(p1.lower, p1.upper);
    }
    public static bool Contains<T>(this Limits<T> bigger, Limits<T> smaller)
        where T : IComparable<T>
    {
        return smaller.InsideOfOrdered(bigger);
    }
    public static bool InsideOfOrdered<T>(this Limits<T> smaller, Limits<T> bigger)
        where T : IComparable<T>
    {
        var compareTo = bigger.lower.CompareTo(smaller.lower);
        var to = bigger.upper.CompareTo(smaller.upper);
        return compareTo.SmallerOrEqual() && to.GreaterOrEqual();
    }

    public static bool Intersects<T>(this Limits<T> p1, (T lower, T upper) p2)
        where T : IComparable<T>
    {
        return p1.lower.WithIn(p2)
               || p2.lower.WithInOrdered(p1.lower, p1.upper);
    }

    public static bool Intersects<T>(this (T lower, T upper) p1, (T lower, T upper) p2)
        where T : IComparable<T>
    {
        return p1.lower.WithIn(p2)
               || p2.lower.WithIn(p1);
    }

    public static Limits<T>? Intersection<T>(this Limits<T>? p1, Limits<T>? p2)
        where T : IComparable<T>
    {
        if (p1 == null || p2 == null) return null;

        return IntersectionOrdered(p1.Value, p2.Value);
    }

    private static Limits<T>? IntersectionOrdered<T>(this Limits<T> p1, Limits<T> p2)
        where T : IComparable<T>
    {
        return IntersectionOrdered((p1.lower, p1.upper), (p2.lower, p2.upper));
    }

    public static (T lower, T upper)? Intersection<T>(this (T lower, T upper) p1,
        (T lower, T upper) p2)
        where T : IComparable<T>
    {
        var p1Ord = p1.Order();
        var p2Ord = p2.Order();
        return IntersectionOrdered(p1Ord, p2Ord);
    }

    public static (T lower, T upper)? IntersectionOrdered<T>(this (T lower, T upper) p1, (T lower, T upper) p2)
        where T : IComparable<T>
    {
        var lower = p1.lower;
        var upper = p1.upper;
        var bothSet = 0;

        if (p1.lower.WithInOrdered(p2.lower, p2.upper))
        {
            lower = p1.lower;
            bothSet++;
        }
        else if (p2.lower.WithInOrdered(p1.lower, p1.upper))
        {
            lower = p2.lower;
            bothSet++;
        }

        if (p1.upper.WithInOrdered(p2.lower, p2.upper))
        {
            upper = p1.upper;
            bothSet++;
        }
        else if (p2.upper.WithInOrdered(p1.lower, p1.upper))
        {
            upper = p2.upper;
            bothSet++;
        }

        if (bothSet < 2) return null;

        return (lower, upper);
    }

    public static bool WithIn<T>(this T c, (T lower, T upper)? p)
        where T : IComparable<T>
    {
        if (p == null) return false;
        return WithIn(c, (p.Value.lower, p.Value.upper));
    }
}