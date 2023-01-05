
using common.SparseMatrix;

namespace _24;

internal class Walker
{
    public Walker(IPosition startPos, IPosition endPos, int legs)
    {
        _numberOfLegsToGo = legs;
        _distanceStartToEnd = (int)startPos.ManhattanDistance(endPos);
    }

    public static Matrix? Matrix { get; set; }
    private readonly int _numberOfLegsToGo;
    private readonly Dictionary<Position, HashSet<Position>> _neighborsCache = new();
    private readonly Dictionary<(int minute, Position position, int leg), int> _resultsCache = new();
    private readonly int _distanceStartToEnd;

    public HashSet<Position> NeighborsPositions(Position position)
    {
        if (_neighborsCache.ContainsKey(position))
            return _neighborsCache[position];

        var hashSet = _neighborsCache[position] = Position.RawNeighbors(position)
            .Where(p => !p.Outside(Matrix!.Limits))
            .ToHashSet();
        return hashSet;
    }

    public HashSet<Position> Moves(int time, Position walkerPos, Position start, Position end)
    {
        var prospects = NeighborsPositions(walkerPos);
        prospects.Add(walkerPos);

        var blizzardsAtTime = Matrix!.BlizzardsAtTime(time);

        var localBlizzards = blizzardsAtTime
            .Where(x => x.Key.ManhattanDistance(walkerPos) < 2)
            .ToDictionaryWithDuplicates();

        var occupiedPositions = localBlizzards
            .Select(x => x.Key)
            .ToHashSet();

        //remove those occupied
        var positions = prospects.Where(x => !occupiedPositions.Contains(x)).ToHashSet();

        positions.Add(walkerPos); // should be able to wait

        if (walkerPos.ManhattanDistance(end) == 1)
        {  //start and end are outside general matrix area
            positions.Add(end);
        }
        if (walkerPos.ManhattanDistance(start) == 1)
        {  //start and end are outside general matrix area
            positions.Add(start);
        }

        return positions;
    }
    public int DoMoves(int minute, int bestResult, Position walkerPos, int leg, Position start, Position end, int level)
    {
        if (walkerPos == end)
        {
            if (leg < _numberOfLegsToGo)
            {
                Console.WriteLine($"Leg {leg} {minute}");
                leg++;
                (start, end) = (end, start);
            }
            else
            {
                Matrix!.PrintOut(minute, walkerPos, new List<Position>(), level);
                Console.WriteLine("Next Minute=" + minute);

                return minute;
            }
        }

        var distanceLeft = walkerPos.ManhattanDistance(end);

        minute++;
        int minRemaining = minute + (int)distanceLeft + (_numberOfLegsToGo - leg) * _distanceStartToEnd;
        if (minRemaining >= bestResult)
            return int.MaxValue;


        var moves = this.Moves(minute, walkerPos, start, end).OrderBy(x => x.ManhattanDistance(end)).ToList();


        var myBest = bestResult;
        foreach (var position in moves)
        {
            if (position == walkerPos)
            {
                //         Debug.WriteLine("Wait");
            }

            Matrix!.PrintOut(minute, position, moves, level + 1);
            var result = int.MaxValue;
            var key = (minute, position, leg);
            if (!_resultsCache.ContainsKey(key))
            {
                result = DoMoves(minute, myBest, position, leg, start, end, level + 1);
                _resultsCache[key] = result;
            }
            else
            {
                result = _resultsCache[key];
            }
            myBest = Math.Min(myBest, result);
        }

        if (moves.Count == 0)
        {
            //   Debug.Write("NoMoves");
        }

        return Math.Min(bestResult, myBest);
    }

}