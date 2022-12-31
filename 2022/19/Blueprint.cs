using common;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace _19;

internal enum Resource
{
    None, Ore, Clay, Obsidian, Geode
}

internal class Blueprint
{
    public int Id { get; }


    public static Blueprint Parse(string text)
    {
        //    0     1  2    3    4    5    6  7     8    9     10    11  12 13    14    15      16    17  18 19   20 21 22     23   24   25    26    27 28  29 30 31      
        //Blueprint 2:Each ore robot costs 2 ore.  Each clay robot costs 3 ore.  Each obsidian robot costs 3 ore and 8 clay.  Each geode robot costs 3 ore and 12 obsidian."

        var parts = text.Split(": .".ToCharArray(),
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var id = parts[1].ToInt()!.Value;
        var ore = new OreRobot(parts[6]);
        var clay = new ClayRobot(parts[12]);
        var obsidian = new ObsidianRobot(parts[18], parts[21]);
        var geode = new GeodeRobot(parts[27], parts[30]);
        var robotTypes = new Dictionary<Resource, Robot>
        {
            { ore.Produces, ore },
            { clay.Produces, clay },
            { obsidian.Produces, obsidian },
            { geode.Produces, geode },
        };
        var bp = new Blueprint(id, robotTypes);
        return bp;
    }

    public Dictionary<Resource, Robot> RobotTypes { get; }
    public ResourceCounts AvailableResources = new();

    public Dictionary<Resource, ResourceCounts> Cost { get; }
    
    private Blueprint(int id, Dictionary<Resource, Robot> robotTypes)
    {
        Id = id;
        RobotTypes = robotTypes;

        var costs = new Dictionary<Resource, ResourceCounts>();
        foreach (var robotType in RobotTypes.OrderBy(x => x.Key))
        {
            costs[robotType.Key] = robotType.Value.Price;
        }
        Cost = costs;
    }

    public virtual long Evaluate(int forTime)
    {
        var workingRobots = new ResourceCounts();
        var cachedResults = new Dictionary<(int time, ResourceCounts robots), long>();
        var minCachedLevel = forTime + 1;
        var lastBestCachedOnLevel = 0L;
        (long best, ResourceCounts robots) cacheAnalysis = (best: 0L, robots: new ResourceCounts());
        workingRobots.Add(Resource.Ore, 1);
        return Run(workingRobots, 1, AvailableResources, 0) / 1_000_000_000_000_000;

        void SaveToCache(
            Dictionary<(int time, ResourceCounts robots), long> cache,
            (int time, ResourceCounts robots) cacheKey,
            long best)
        {
            cache[cacheKey] = best;
        }

        long Run(ResourceCounts robots, int time, ResourceCounts availableResources, long best)
        {
            if (time > 24)
            {
                //9,223,372,036,854,775,807
                var totalResult = CalculateResult(robots, availableResources);
                return totalResult;
            }

            var cacheKey = (time, robots);
            if (cachedResults.ContainsKey(cacheKey))
            {
                var cachedResult = cachedResults[cacheKey];
                if (time < minCachedLevel || (time == minCachedLevel && cachedResult < lastBestCachedOnLevel))
                {
                    lastBestCachedOnLevel = cachedResult;
                    minCachedLevel = time;
                    Console.WriteLine($"Mincached level={time}  {cachedResult} {cacheAnalysis.robots}");
                }
                return cachedResult;
            }

            var actions = AvailableActions(forTime - time, robots, availableResources).ToList();

            actions.Insert(0, Resource.None); //always try not buying a robot first
            foreach (var action in actions)
            {
                var robotsList = robots;
                var available = availableResources;
                var performedAction = false;

                if (action == Resource.None)
                {
                    performedAction = true;
                }
                else
                {
                    performedAction = SubtractPrice(ref available, Cost[action]);
                }


                if (performedAction)
                {
                    //only if any of the action was performed
                    robotsList.ForEach((r, i) => available.Add(r.Resource, r.Value));

                    var result = Run(robotsList, time + 1, available, best);
                    best = Math.Max(best, result);
                }
            }

            SaveToCache(cachedResults, cacheKey, best);
            return best;
        }

        long CalculateResult(ResourceCounts robots, ResourceCounts availableResources)
        {
            var totalResult = availableResources[Resource.Geode] * 1_000_000_000_000_000L
                              + availableResources[Resource.Obsidian] * 10_000_000_000_000L
                              + availableResources[Resource.Clay] * 100_000_000_000L
                              + availableResources[Resource.Ore] * 1_000_000_000L;
            var multiplier = 1_000_000_000L / 100;
            var additional = robots.OrderBy(x => x.Resource).Sum(r =>
            {
                multiplier /= 25;
                return r.Value * multiplier;
            });
            totalResult += additional;
            if (cacheAnalysis.best < totalResult)
            {
                cacheAnalysis.best = totalResult;
                cacheAnalysis.robots = robots;
                Console.WriteLine($"Mincached = {cacheAnalysis.robots}");
            }

            return totalResult;
        }
    }

    List<Resource> AvailableActions(int timeLeft, ResourceCounts commonProduction, ResourceCounts available)
    {

        var list = new List<Resource>();

        var maxCostInOre = RobotTypes.Keys
            .Where(x => x > Resource.Ore)
            .Max(x => Cost[x][Resource.Ore]);
        if (commonProduction[Resource.Ore] < maxCostInOre
            && timeLeft > 4
           )
            list.Add(Resource.Ore);

        var maxCostInClay = RobotTypes.Keys
            .Where(x => x > Resource.Clay)
            .Max(x => Cost[x][Resource.Clay]);
        if (commonProduction[Resource.Clay] < maxCostInClay
            && timeLeft > 3
            )
            list.Add(Resource.Clay);

        var maxCostInObsidian = RobotTypes.Keys
            .Where(x => x > Resource.Obsidian)
            .Max(x => Cost[x][Resource.Obsidian]);
        if (commonProduction[Resource.Obsidian] < maxCostInObsidian
            && commonProduction[Resource.Clay] > 0
            && timeLeft > 2
            )
            list.Add(Resource.Obsidian);


        if (commonProduction[Resource.Obsidian] > 0)
            list.Add(Resource.Geode);

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var robotType = list[i];
            if (!HasResources(available, RobotTypes[robotType].Price))
            {
                list.RemoveAt(i);
            }
        }
        list.Reverse();

        return list;
    }


    private bool SubtractPrice(ref ResourceCounts available, ResourceCounts price)
    {
        if (!HasResources(available, price))
            return false;
        foreach (var x in price.Keys)
        {
            available.Add(x, -price[x]);
        }
        return true;
    }


    private bool HasResources(ResourceCounts availableResources, ResourceCounts price)
    {
        foreach (var x in price.Keys)
        {
            if (availableResources[x] < price[x]) return false;
        }

        return true;
    }
}