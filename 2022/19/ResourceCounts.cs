using System.Collections;
using common;

namespace _19;

internal struct ResourceCounts : IEquatable<ResourceCounts>, IEnumerable<(Resource Resource, int Value)>
{
    private int[] _counts = new int[4];

    private int[] Counts
    {
        readonly get => _counts;
        set => _counts = value;
    }

    public IEnumerable<Resource> Keys => Enumerable.Range((int)Resource.Ore,4).Cast<Resource>();
    public ResourceCounts()
    {
        Counts = new[] { 0, 0, 0, 0 };
    }
    public ResourceCounts(ResourceCounts other)
    {
        Counts = other.Counts.ToArray();
    }

    public ResourceCounts(int[] toArray)
    {
        Counts = toArray.ToArray();
    }

    public int this[Resource index]
    {
        get => Counts[((int)index) - 1];
        private set => Counts[((int)index) - 1] = value;
    }
    public int this[int ix]
    {
        get => Counts[ix];
        private set => Counts[ix] = value;
    }

    public ResourceCounts Add(Robot robot)
    {
        var newCounts = new ResourceCounts(this);
        newCounts[robot.Produces]++;
        return newCounts;
    }
    public void Add(Resource res, int amount)
    {
        this[res] += amount;
    }


    public bool Equals(ResourceCounts other)
    {
        return Counts.Equals(other.Counts);
    }
    public override bool Equals(object? obj)
    {
        return obj is ResourceCounts other && Equals(other);
    }
    public override int GetHashCode()
    {
        return Counts.GetHashCode();
    }
    public static bool operator ==(ResourceCounts left, ResourceCounts right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(ResourceCounts left, ResourceCounts right)
    {
        return !left.Equals(right);
    }

    public IEnumerator GetEnumerator()
    {
        foreach (var res in Keys)
        {
            yield return (Resource:res,Value:this[res]);
        }
    }
    IEnumerator<(Resource Resource, int Value)> IEnumerable<(Resource Resource, int Value)>.GetEnumerator()
    {
        foreach (var res in Keys)
        {
            yield return (Resource: res, Value: this[res]);
        }
    }
}

