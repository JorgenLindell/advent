using System;
using System.Collections.Generic;
using System.Linq;

namespace common;

public struct Limits3d<T> : IEquatable<Limits3d<T>> 
    where T : IComparable<T>
{
    public bool Equals(Limits3d<T> other) => Nullable.Equals(x, other.x) && Nullable.Equals(y, other.y) && Nullable.Equals(z, other.z);
    public override bool Equals(object? obj) => obj is Limits3d<T> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, y, z);
    public static bool operator ==(Limits3d<T> left, Limits3d<T> right) => left.Equals(right);
    public static bool operator !=(Limits3d<T> left, Limits3d<T> right) => !left.Equals(right);

    public Limits<T>? x;
    public Limits<T>? y;
    public Limits<T>? z;

    public Limits3d()
        : this(null, null, null)
    {
    }
    public Limits3d(Limits<T>? xLimits, Limits<T>? yLimits, Limits<T>? zLimits)
    {
        this.x = xLimits;
        this.y = yLimits;
        this.z = zLimits;
    }
    public Limits3d(T x1, T x2, T y1, T y2, T z1, T z2)
    {
        x = new Limits<T>(x1, x2);
        y = new Limits<T>(y1, y2); ;
        z = new Limits<T>(z1, z2); ;
    }
    public bool Empty => !(x.HasValue && y.HasValue && z.HasValue);
    public T XLow => x!.Value.lower;
    public T XHigh => x!.Value.upper;
    public T YLow => y!.Value.lower;
    public T YHigh => y!.Value.upper;
    public T ZLow => z!.Value.lower;
    public T ZHigh => z!.Value.upper;
    public List<(T x, T y, T z, (int x, int y, int z) dir)> SplitLines(Limits3d<T> ns)
    {
        if (!ns.Empty)
        {
            var me = this;
            var inter = ns.Intersection(me);
            if (inter != null)
            {
                var corners = inter.Value.Corners
                    .Where(c => me.IntersectsWith(c.x, c.y, c.z)).ToList();
                return corners;
            }
        }

        return new List<(T x, T y, T z, (int x, int y, int z) dir)>();
    }

    public (T x, T y, T z, (int x, int y, int z) dir)[] Corners =>
        new[]
        {
            (x:XLow,  y:YLow,  z:ZLow,  dir:(0,0,0)),
            (x:XLow,  y:YLow,  z:ZHigh ,dir:(0,0,1)),
            (x:XLow,  y:YHigh, z:ZLow  ,dir:(0,1,0)),
            (x:XLow,  y:YHigh, z:ZHigh ,dir:(0,1,1)),
            (x:XHigh, y:YLow,  z:ZLow  ,dir:(1,0,0)),
            (x:XHigh, y:YLow,  z:ZHigh ,dir:(1,0,1)),
            (x:XHigh, y:YHigh, z:ZLow  ,dir:(1,1,0)),
            (x:XHigh, y:YHigh, z:ZHigh ,dir:(1,1,1)),
        };

    public override string ToString()
    {
        return $"x:{XLow}..{XHigh},y:{YLow}..{YHigh},z:{ZLow}..{ZHigh}";
    }
}