using common;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace _19;

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
  

    public int TimeFromOreToGeode { get; }
    public int TimeFromOreToGeodeInOre { get; }
    public int TimeFromClayToGeode { get; }
    public int TimeFromClayToGeodeInClay { get; }
    public int TimeFromObsidianToGeode { get; }

    public Dictionary<Resource, ResourceCounts> Cost { get; }


    private Blueprint(int id, Dictionary<Resource, Robot> robotTypes)
    {
        Id = id;
        RobotTypes = robotTypes;

        var costs = new Dictionary<Resource, ResourceCounts>();
        foreach (var robotType in RobotTypes.OrderBy(x=>x.Key))
        {
        
            var cost = new ResourceCounts(robotType.Value.Price.Select(x => x.amount).ToArray());
            costs[robotType.Key] = cost;
        }
        Cost = costs;

        TimeFromOreToGeode = 1 + Cost[Resource.Clay][Resource.Ore]
                              + Cost[Resource.Obsidian][Resource.Clay]
                              + Cost[Resource.Geode][Resource.Obsidian];
        TimeFromOreToGeodeInOre = 1 + Cost[Resource.Obsidian][Resource.Ore]
                                   + Cost[Resource.Geode][Resource.Ore];
        TimeFromClayToGeode = 1 + Cost[Resource.Obsidian][Resource.Clay]
                               + Cost[Resource.Geode][Resource.Obsidian];
        TimeFromClayToGeodeInClay = 1 + Cost[Resource.Obsidian][Resource.Clay];
        TimeFromObsidianToGeode = 1 + Cost[Resource.Geode][Resource.Obsidian];

    }

    public virtual long Evaluate(int forTime)
    {
        var workingRobots = new RobotList { new OreRobot(0) };
        var cachedResults = new Dictionary<(int time, RobotList robots), long>();
        var minCachedLevel = forTime + 1;
        var lastBestCachedOnLevel = 0L;
        (long best, RobotList robots) cacheAnalysis = (0, new RobotList());

        return Run(workingRobots, 1, AvailableResources, 0) / 1_000_000_000_000_000;

        void SaveToCache(Dictionary<(int time, RobotList robots), long> cache, (int time, RobotList robots) cacheKey, long best)
        {
            cache[cacheKey] = best;
        }

        long Run(RobotList robots, int time, ResourceCounts availableResources, long best)
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

            actions.Insert(0, new List<Resource> { Resource.None }); //always try not buying a robot first
            foreach (var actionList in actions)
            {
                var robotsList = robots.ToList();
                var available = availableResources;
                var performedAction = false;
                foreach (var action in actionList)
                {
                    if (action == Resource.None)
                    {
                        performedAction = true;
                        break;
                    }

                    performedAction |= TryBuyRobot(time, action, robotsList, ref available);
                }

                if (performedAction)
                {
                    //only if any of the action was performed
                    robotsList.ForEach(r => r.DoProduce(time, ref available));

                    var result = Run(robotsList, time + 1, available, best);
                    best = Math.Max(best, result);
                }
            }

            SaveToCache(cachedResults, cacheKey, best);
            return best;
        }

        long CalculateResult(RobotList robots, ResourceCounts availableResources)
        {
            var robotTypes = robots.ByType();
            var totalResult = availableResources[Resource.Geode] * 1_000_000_000_000_000L
                              + availableResources[Resource.Obsidian] * 10_000_000_000_000L
                              + availableResources[Resource.Clay] * 100_000_000_000L
                              + availableResources[Resource.Ore] * 1_000_000_000L;
            var multiplier = 1_000_000_000L / 100;
            var additional = robotTypes.OrderBy(x => x.Key).Sum(r =>
            {
                multiplier /= 25;
                return r.Count() * multiplier;
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

    private bool TryBuyRobot(int time, Resource action, RobotList robotslist, ref ResourceCounts available)
    {
        if (!SubtractPrice(ref available, RobotTypes[action].Price))
            return false;

        Robot newRobot = action switch
        {
            Resource.Ore => new OreRobot(time),
            Resource.Clay => new ClayRobot(time),
            Resource.Obsidian => new ObsidianRobot(time),
            Resource.Geode => new GeodeRobot(time),
            Resource.None => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
        robotslist.Add(newRobot);
        return true;
    }

    List<List<Resource>> AvailableActions(int timeLeft, RobotList robotList,
        ResourceCounts pAvailable)
    {
        var commonProduction = robotList.AllProduction();

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

        //  list.Add(Resource.Ore);
        //  list.Add(Resource.Clay);
        //  list.Add(Resource.Obsidian);
        if (commonProduction[Resource.Obsidian]>0 )
         list.Add(Resource.Geode);

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var robotType = list[i];
            if (!HasResources(pAvailable, RobotTypes[robotType].Price))
            {
                list.RemoveAt(i);
            }
        }
        //list.Add(Resource.None);
        var availableActions = new List<List<Resource>>();
        list.Reverse();
        list.ForEach(x=>availableActions.Add(new List<Resource>(){x}));

    //  var permutations = list.GetPermutations().Select(x => x.ToList()).ToList();
    //  foreach (var permutation in permutations)
    //  {
    //      for (int i = 0; i < permutation.Count; i++)
    //      {
    //          if (permutation[i] == Resource.None)
    //          {
    //              permutation.RemoveRange(i, permutation.Count - i);
    //          }
    //      }
    //      if (permutation.Count > 0)
    //          availableActions.Add(permutation);
    //  }
     //   RemoveDuplicateActions(availableActions);
     //   if (availableActions.Count > 2)
     //       availableActions = availableActions.OrderBy(x => x.Count * 10 + (9 - x[0])).ToList();
        return availableActions;


        //local
        void RemoveDuplicateActions(List<List<Resource>> actionListList)
        {
            for (int i = 0; i < actionListList.Count; i++)
            {
                for (int j = i + 1; j < actionListList.Count; j++)
                {
                    if (actionListList[i].Count == actionListList[j].Count)
                    {
                        var k = 0;
                        bool diff = false;
                        foreach (var x in actionListList[j])
                        {
                            if (actionListList[i][k] != x)
                            {
                                diff = true;
                                break;
                            }
                            k++;
                        }
                        if (diff == false)
                        {
                            actionListList.RemoveAt(j);
                            j--;
                        }
                    }
                    //else
                    //{
                    //    var k = 0;
                    //    bool diff = false;
                    //    foreach (var x in actionListList[i])
                    //    {
                    //        if (actionListList[j][k] != x)
                    //        {
                    //            diff = true;
                    //            break;
                    //        }
                    //        k++;
                    //    }
                    //    if (diff == false)
                    //    {
                    //        actionListList.RemoveAt(i);
                    //        i--;
                    //        j = int.MaxValue - 1;
                    //    }
                    //}
                }
            }
        }

    }

    private bool IsOfUse(int timeLeft, Resource resource, Dictionary<Resource, int> available,
        Dictionary<Resource, int> commonProduction)
    {
        switch (resource)
        {
            case Resource.Ore:
                {
                    var costOfClay = Cost[Resource.Clay][Resource.Ore];
                    timeLeft -= 1;// to build 
                    timeLeft -= costOfClay;
                    return IsOfUse(timeLeft, Resource.Clay, available, commonProduction);
                }
            case Resource.Clay:
                {
                    var costOfObsidian = Cost[Resource.Obsidian][Resource.Clay];
                    timeLeft -= 1;// to build 
                    timeLeft -= costOfObsidian; //produce 
                    return IsOfUse(timeLeft, Resource.Obsidian, available, commonProduction);
                }
            case Resource.Obsidian:
                {
                    var costOfGeode = Cost[Resource.Geode][Resource.Obsidian];
                    timeLeft -= 1;// to build 
                    timeLeft -= costOfGeode; //produce 
                    return IsOfUse(timeLeft, Resource.Geode, available, commonProduction);
                }
            case Resource.Geode:
                {
                    timeLeft -= 1;// to build 
                    timeLeft -= 1;// to produce 
                    return timeLeft > 0;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
        }
    }


    private Dictionary<Resource, int> CloneAvailable(Dictionary<Resource, int> availableResources)
    {
        return availableResources.ToDictionary(x => x.Key, x => x.Value);
    }

    private bool SubtractPrice(ref ResourceCounts available, List<(int amount, Resource resource)> price)
    {
        if (!HasResources(available, price))
            return false;
        foreach (var x in price)
        {
            available.Add(x.resource, -x.amount);
        }
        return true;
    }


    private bool HasResources(ResourceCounts availableResources,
        List<(int amount, Resource resource)> price)
    {
        foreach (var pr in price)
        {
            if (availableResources[pr.resource] < pr.amount) return false;
        }
        return true;
    }
}