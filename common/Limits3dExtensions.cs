using System;

namespace common;

public static class Limits3dExtensions
{
    public static bool IntersectsWith<T>(this Limits3d<T> me, Limits3d<T> other)
        where T : IComparable<T>
    {
        if (me.Empty || other.Empty) return false;
        var x = me.x!.Value.Intersects(other.x!.Value);
        var y = me.y!.Value.Intersects(other.y!.Value);
        var z = me.z!.Value.Intersects(other.z!.Value);
        return x && y && z;
    }
    public static bool InsideOf<T>(this Limits3d<T> me, Limits3d<T> other)
        where T : IComparable<T>
    {
        if (me.Empty || other.Empty) return false;
        var x = me.x!.Value.InsideOfOrdered(other.x!.Value);
        var y = me.y!.Value.InsideOfOrdered(other.y!.Value);
        var z = me.z!.Value.InsideOfOrdered(other.z!.Value);
        return x && y && z;

    }
    public static Limits3d<T>? Intersection<T>(this Limits3d<T> me, Limits3d<T> otherLimits)
        where T : IComparable<T>
    {
        var interX = me.x.Intersection(otherLimits.x);
        if (interX == null) return null;

        var interY = me.y.Intersection(otherLimits.y);
        if (interY == null) return null;

        var interZ = me.z.Intersection(otherLimits.z);
        if (interZ == null) return null;

        return new Limits3d<T>(
            interX.Value,
            interY.Value,
            interZ.Value);
    }
    public static bool IntersectsWith<T>(this Limits3d<T> me, T x, T y, T z)
        where T : IComparable<T>
    {
        if (me.Empty) return false;
        var xb = x.WithInOrdered(me.x!.Value);
        var yb = y.WithInOrdered(me.y!.Value);
        var zb = z.WithInOrdered(me.z!.Value);
        return xb && yb && zb;

    }

    public static (
        T X1,
        T X2,
        T Y1,
        T Y2,
        T Z1,
        T Z2
        )? Intersection<T>(
            this (
                T X1,
                T X2,
                T Y1,
                T Y2,
                T Z1,
                T Z2) a,
            (
                T X1,
                T X2,
                T Y1,
                T Y2,
                T Z1,
                T Z2) b
        )
        where T : IComparable<T>
    {

        var interX = (a.X1, a.X2).Intersection((b.X1, b.X2));
        if (interX == null) return null;
        var interY = (a.Y1, a.Y2).Intersection((b.Y1, b.Y2));
        if (interY == null) return null;
        var interZ = (a.Z1, a.Z2).Intersection((b.Z1, b.Z2));
        if (interZ == null) return null;
        return (
            interX.Value.lower,
            interX.Value.upper,
            interY.Value.lower,
            interY.Value.upper,
            interZ.Value.lower,
            interZ.Value.upper);
    }

   
}