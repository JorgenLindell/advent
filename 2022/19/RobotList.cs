using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Runtime.Versioning;
using common;

namespace _19;

internal struct ResourceCounts : IEquatable<ResourceCounts>
{
    private int[] Counts { get; set; } = new int[4];
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
}

internal class RobotList : List<Robot>, IEquatable<RobotList>
{

    public RobotList()
    {
    }
    public RobotList(RobotList robotList)
    {
        foreach (var robot in robotList)
        {
            this.Add(robot);
        }
    }

    public List<Robot> OfType(Resource type)
    {
        return this.Where(x => x.Produces == type).ToList();
    }

    public RobotList ToList()
    {
        return new RobotList(this);
    }
    public bool Equals(RobotList? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (other.Count != this.Count) return false;
        for (var index = 0; index < this.Count; index++)
        {
            if (!this[index].Equals(other[index])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RobotList)obj);
    }

    public override int GetHashCode()
    {
        var first = this.FirstOrDefault();
        if (first == null) return 0;
        var hash = first.GetHashCode();
        for (int i = 1; i < this.Count; i++)
        {
            hash = HashCode.Combine(hash, this[i].GetHashCode());
        }
        return hash;
    }

    public static bool operator ==(RobotList? left, RobotList? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RobotList? left, RobotList? right)
    {
        return !Equals(left, right);
    }

    public Dictionary<Resource, int> AllProduction()
    {
        Dictionary<Resource, int> production = new()
        {
            {Resource.Ore,0},
            {Resource.Clay,0},
            {Resource.Obsidian,0},
            {Resource.Geode,0},
        };
        this.GroupBy(y => y.Produces).ForEach((x, _) => production[x.Key] = x.Count());
        return production;
    }
    public ResourceCounts Counts()
    {
        var counts = new ResourceCounts();
        this.GroupBy(y => y.Produces).ForEach((x, _) => counts.Add(x.Key, x.Count()));
        return counts;
    }

    public string ToString2() => $"({this.Select(x => "" + x.Produces).StringJoin(", ") ?? string.Empty})";
    public override string ToString() => $"({this.Select(x => "" + x.Produces + x.CreatedTime).StringJoin(", ") ?? string.Empty})";

    public IEnumerable<IGrouping<Resource, Robot>> ByType()
    {
        return this.GroupBy(x => x.Produces).ToList();
    }
}