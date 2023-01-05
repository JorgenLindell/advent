using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace _19;

internal class ResourceCounts : IEquatable<ResourceCounts>, IEnumerable<(Resource Resource, int Value)>
{
    private static Resource[] StaticKeys { get; } = Enumerable.Range((int)Resource.Ore, 4).Cast<Resource>().ToArray();
    public int[] Counts { get; } = new int[4];

    public Resource[] Keys => StaticKeys;

    public ResourceCounts()
    {
    }
    public ResourceCounts(ResourceCounts other)
    {
        Counts = other.Counts.ToArray();
    }

    public ResourceCounts(int[] array)
    {
        Counts = array.ToArray();
    }
    public ResourceCounts Copy()=>new (Counts);

    public int this[Resource index] => Counts[index - Resource.Ore];


    public void Add(Resource res, int amount)
    {
        Counts[res - Resource.Ore]+= amount;
    }
    private void Add(int index, int amount)
    {
        Counts[index] += amount;
    }

    public bool Equals(ResourceCounts? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        return Counts.SequenceEqual(other.Counts);
    }
    public override bool Equals(object? obj)
    {
        return obj is ResourceCounts other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Counts[0], Counts[1], Counts[2], Counts[3]);
    }
    public static bool operator ==(ResourceCounts left, ResourceCounts right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(ResourceCounts left, ResourceCounts right)
    {
        return !left.Equals(right);
    }

    public static ResourceCounts operator -(ResourceCounts left, ResourceCounts right)
    {
        var result = new ResourceCounts(left);
        for (int i = 0; i < 4; i++)
        {
            result.Add(i, -right.Counts[i]);
        }

        return result;
    }

    public static ResourceCounts operator +(ResourceCounts left, ResourceCounts right)
    {
        var result = new ResourceCounts(left);
        for (int i = 0; i < 4; i++)
        {
            result.Add(i, right.Counts[i]);
        }
        return result;
    }

    public IEnumerator GetEnumerator()
    {
        foreach (Resource res in Keys)
        {
            yield return (Resource: res, Value: this[res]);
        }
    }
    IEnumerator<(Resource Resource, int Value)> IEnumerable<(Resource Resource, int Value)>.GetEnumerator()
    {
        foreach (Resource res in Keys)
        {
            yield return (Resource: res, Value: this[res]);
        }
    }
}

