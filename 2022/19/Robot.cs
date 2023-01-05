using common;

namespace _19;

internal interface IRobot
{
    Resource Produces { get; }
    ResourceCounts Price { get; }
}

internal abstract class Robot : IRobot
{
    protected Robot()
    {
    }

    public abstract Resource Produces { get; }
    public ResourceCounts Price { get; }  = new();
}

internal class OreRobot : Robot
{
    public OreRobot(string priceOre)  => Price.Add(Resource.Ore, priceOre.ToInt()!.Value);
    public override Resource Produces => Resource.Ore;
}

internal class ClayRobot : Robot
{
    public ClayRobot(string priceOre) => Price.Add(Resource.Ore, priceOre.ToInt()!.Value);
    public override Resource Produces => Resource.Clay;
}

internal class ObsidianRobot : Robot
{
    public ObsidianRobot(string priceOre, string priceClay)
    {
        Price.Add(Resource.Ore, priceOre.ToInt()!.Value);
        Price.Add(Resource.Clay, priceClay.ToInt()!.Value);
    }
    public override Resource Produces => Resource.Obsidian;

}
internal class GeodeRobot : Robot
{
    public GeodeRobot(string priceOre, string priceObsidian) : base()
    {
        Price.Add(Resource.Ore, priceOre.ToInt()!.Value);
        Price.Add(Resource.Obsidian, priceObsidian.ToInt()!.Value);
    }
    public override Resource Produces => Resource.Geode;
}