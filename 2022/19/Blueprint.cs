using common;

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
    public ResourceCounts MaxCostPerType { get; }

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
        var maxesInOrder = RobotTypes.Keys
            .SelectMany(x => Cost[x])
            .GroupBy(cost => cost.Resource)
            .Select(costType => (resource: costType.Key, max: costType.Max(x => x.Value)))
            .OrderBy(m => m.resource)
            .Select(x => x.max)
            .ToArray();
        MaxCostPerType = new ResourceCounts(maxesInOrder);
    }

    public virtual long Evaluate(int forTime, out List<(Resource, int)> builtRobots)
    {
        var workingRobots = new ResourceCounts();
        var cachedResults = new Dictionary<(int timeLeft, Resource robotToBuild, ResourceCounts pRobots, ResourceCounts pAvailableResources), (long best, List<(Resource, int)> robots)>();

        workingRobots.Add(Resource.Ore, 1);
        builtRobots = new();
        var run = Run(workingRobots, forTime, Resource.None, AvailableResources, 0, ref builtRobots);

        return run;


        long Run(ResourceCounts pRobots, int timeLeft, Resource robotToBuild, ResourceCounts pAvailableResources, long best, ref List<(Resource, int)> pRobotsBuilt)
        {

            var robotsBuilt = pRobotsBuilt.ToList();
            var cacheKey = (timeLeft, robotToBuild, pRobots, pAvailableResources);
            if (cachedResults.ContainsKey(cacheKey))
            {
                pRobotsBuilt = cachedResults[cacheKey].robots.ToList();
                return cachedResults[cacheKey].best;
            }
            ResourceCounts robots = pRobots.Copy();
            ResourceCounts availableResources = pAvailableResources.Copy();
            if (robotToBuild != Resource.None)
            {
                timeLeft -= 1; //pass time for building
                if (!SubtractPrice(availableResources, Cost[robotToBuild])) throw new InvalidDataException("Can't subtract more of resource than is available");
                foreach ((Resource Resource, int Value) x in robots)
                {
                    availableResources.Add(x.Resource, x.Value);
                }

                robots.Add(robotToBuild, 1);
                robotsBuilt.Add((robotToBuild, forTime - timeLeft));
            }

            if (GeodesPossible(timeLeft, robots, availableResources) <= best)
            {
                cachedResults[cacheKey] = (0, new List<(Resource, int)>());
                return 0; // best is better than a theoretical max based on calculation, that is good, prune this branch
            }

            pRobotsBuilt = robotsBuilt.ToList();


            long result;
            foreach (var robotType in availableResources.Keys)
            {
                var robotsBuiltInner = pRobotsBuilt.ToList();
                //check all types
                var timeToWait = TurnsToGet(robotType, timeLeft, robots, availableResources);
                if (timeToWait == -1)
                    continue;
                var availableInner = availableResources.Copy();
                foreach ((Resource Resource, int Value) x in robots)
                    availableInner.Add(x.Resource, x.Value * timeToWait);

                var timeLeftAfterWait = timeLeft - timeToWait;

                result = Run(
                    robots,
                    timeLeftAfterWait,
                    robotType,
                    availableInner,
                    best,
                    ref robotsBuiltInner
                );
                if (result > best)
                {
                    robotsBuilt = robotsBuiltInner.ToList();
                    best = result;
                }
            }
            // at the end, nothing is bought, just geode robots producing.
            result = availableResources[Resource.Geode] + robots[Resource.Geode] * timeLeft;
            best = Math.Max(result, best);
            pRobotsBuilt = robotsBuilt.ToList();
            return best;
        }

    }



    private bool SubtractPrice(ResourceCounts available, ResourceCounts price)
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
    private int TurnsToGet(Resource robotType,
        int timeLeft,
        ResourceCounts currentRobots,
        ResourceCounts available)
    {
        if (robotType != Resource.Geode
            && available[robotType] + currentRobots[robotType] * timeLeft >= MaxCostPerType[robotType] * timeLeft)
            return -1;  // Getting one is no help as production would exceed what can be consumed

        var turnsToWait = 0;
        foreach (var resType in available.Keys)
        {   // check each type of resource available and needed for the required robotType
            int avail = available[resType];
            int cost = Cost[robotType][resType];
            int robots = currentRobots[resType];

            if (cost == 0 || avail >= cost)
                continue; // we have that
            if (robots == 0)
                return -1; // Cannot get enough materials by waiting, no one is producing
            turnsToWait = Math.Max(turnsToWait,
                (int)Math.Ceiling(
                    (cost - avail) // how much is missing?
                     / (decimal)robots) //how much is produced each step?
                ); //round up
        }
        if (turnsToWait < timeLeft - 1)
            return turnsToWait; // there is time enough
        return -1; // Time to purchase longer than time remaining
    }


    public int GeodesPossible(int timeLeft, ResourceCounts robots, ResourceCounts available)
    {
        /*
         * (timeLeft * (timeLeft - 1)) / 2
         * if we have timeLeft=5 then formula gives 10
         * step 5 build 1 giving 1 produce 0
         * step 4 build 1 giving 2 produce 1 sum of produced 1
         * step 3 build 1 giving 3 produce 2 sum of produced 3
         * step 2 build 1 giving 4 produce 3 sum of produced 6
         * step 1 build 1 giving 5 produce 4 sum of produced 10
         */

        var maxObsidian = available[Resource.Obsidian] // with what we have
                          + robots[Resource.Obsidian] * timeLeft // what can be produce with current robots
                          + (timeLeft * (timeLeft - 1)) / 2; //if we can build one each step

        var maxNewGeodeRobots = maxObsidian / Cost[Resource.Geode][Resource.Obsidian];
        var productionGeodesCurrent = available[Resource.Geode] + robots[Resource.Geode] * timeLeft;
        if (maxNewGeodeRobots >= timeLeft) // there is time for this
            return productionGeodesCurrent // with what we have
                   + (timeLeft * (timeLeft - 1)) / 2; // if we can build one each step
        return (
            productionGeodesCurrent // with what we have
            + (maxNewGeodeRobots * (maxNewGeodeRobots - 1)) / 2 // if we can build this many in subsequent steps
            + (timeLeft - maxNewGeodeRobots) * maxNewGeodeRobots // and the steps none is built, just producing
        );
    }
}