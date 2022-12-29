using System.Diagnostics;
using System.Text;
using common;

internal class Matrix : DictionaryWithDuplicates<Position, Blizzard>
{
    private readonly Dictionary<int, DictionaryWithDuplicates<Position, Blizzard>> _cachedBlizzardsAtTime = new();

    public Matrix(Position limitMin, Position limitMax)
    {
        LimitMin = limitMin;
        LimitMax = limitMax;
        Height = Math.Abs(LimitMax.Y - LimitMin.Y) + 1;
        Width = Math.Abs(LimitMax.X - LimitMin.X) + 1;
        Size = new Position(Width, Height);
    }


    public Matrix(Position limitMax)
        : this(new Position(0, 0), limitMax)
    {
    }

    public Position? Min { get; private set; }
    public Position? Max { get; private set; }

    public Position LimitMin { get; }
    public Position LimitMax { get; }
    public (Position min, Position max) Limits => (LimitMin, LimitMax);
    public long Height { get; }
    public long Width { get; }
    public Position Size { get; set; }
    public List<Blizzard> Blizzards { get; } = new();

    public override HashSet<Blizzard> Value(Position pos, Blizzard newValue)
    {
        var (x, y) = pos;
        if (Min == null)
            Min = new Position(x, y);
        if (Max == null)
            Max = new Position(x, y);
        Min.Y = Math.Min(y, Min.Y);
        Min.X = Math.Min(x, Min.X);
        Max.Y = Math.Max(y, Max.Y);
        Max.X = Math.Max(x, Max.X);
        return base.Value(pos, newValue);
    }

    public DictionaryWithDuplicates<Position, Blizzard> BlizzardsAtTime(int time)
    {
        if (_cachedBlizzardsAtTime.ContainsKey(time))
            return _cachedBlizzardsAtTime[time];

        return _cachedBlizzardsAtTime[time] = Blizzards.Select(b => (pos: b.PositionAtTime(time), blizz: b))
            .ToDictionaryWithDuplicates(x => x.pos, x => x.blizz);
    }

    public void PrintOut(int time, Position walkerPos, IEnumerable<PositionBase> relevant, int level)
    {
        var relevantArr = relevant as PositionBase[] ?? relevant.ToArray();
        var blizzards = BlizzardsAtTime(time);
        var sb = new StringBuilder();
        if (Max!.Y - Min!.Y < 10)
        {
            sb.AppendLine($"------------------------({time}  l:{level})");
            for (var y = Min.Y; y <= Max.Y; y++)
            {
                var moves = relevant as PositionBase[] ?? relevantArr.ToArray();
                for (var x = Min.X; x <= Max.X; x++)
                {
                    string value;
                    var position = new Position(x, y);
                    var hashSet = blizzards.ContainsKey(position)
                        ? blizzards[position]
                        : null;
                    if (position == walkerPos)
                    {
                        value = hashSet != null ? "E" : "W";
                    }
                    else if (hashSet == null)
                    {
                        value = position.In(moves) ? "+" : ".";
                    }
                    else
                    {
                        value = hashSet.Count > 1 
                            ? "" + hashSet.Count 
                            : "" + hashSet.First().Symbol;
                    }

                    sb.Append(value);
                }
                sb.AppendLine();
            }
            Debug.WriteLine(sb.ToString());
        }
    }

}