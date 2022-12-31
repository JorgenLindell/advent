using System.Diagnostics;
using common;

namespace _19;
internal enum Resource
{
    None, Ore, Clay, Obsidian, Geode
}

internal abstract class Robot : IEquatable<Robot>
{
    private static int robotSequence = 1;
    public int Id { get; }
    public abstract Resource Produces { get; }
    public List<(int amount, Resource resource)> Price { get; } = new();
    
    protected Robot(int time)
    {
        CreatedTime = time;
        Id = robotSequence++;
    }

    public int CreatedTime { get; }

    public bool Equals(Robot? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Produces == other.Produces  ;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Robot robot ) return false;
        return Equals(robot);
    }

    public override int GetHashCode()
    {
        return (int)Produces;
    }

    public static bool operator ==(Robot? left, Robot? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Robot? left, Robot? right)
    {
        return !Equals(left, right);
    }

    public void DoProduce(int time, ref ResourceCounts availableResources)
    {
        if (time > CreatedTime)
            availableResources.Add( Produces, 1);
       
    }
}

internal class OreRobot : Robot
{
    public OreRobot(string priceOre) : base(0)
    {
        Price.Add((priceOre.ToInt()!.Value, Resource.Ore));
    }
    public OreRobot(int time ) : base(time)
    {
    }

    public override Resource Produces => Resource.Ore;
}

internal class ClayRobot : Robot
{
    public ClayRobot(int time ) : base(time)
    {
    }

    public ClayRobot(string priceOre) : base(0)
    {
        Price.Add((priceOre.ToInt()!.Value, Resource.Ore)); ;
    }

    public override Resource Produces => Resource.Clay;
}

internal class ObsidianRobot : Robot
{
    public ObsidianRobot(int time) : base(time)
    {
    }

    public ObsidianRobot(string priceOre, string priceClay ) : base(0)
    {
        Price.Add((priceOre.ToInt()!.Value, Resource.Ore));
        Price.Add((priceClay.ToInt()!.Value, Resource.Clay));
    }

    public override Resource Produces => Resource.Obsidian;

}
internal class GeodeRobot : Robot
{
   public GeodeRobot(int time) : base(time)
    {
    }    public GeodeRobot(string priceOre, string priceObsidian) : base(0)
    {
        Price.Add((priceOre.ToInt()!.Value, Resource.Ore));
        Price.Add((priceObsidian.ToInt()!.Value, Resource.Obsidian));
    }

    public override Resource Produces => Resource.Geode;

}