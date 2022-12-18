
using System.Diagnostics;
using System.Net;
using common;
using common.SparseMatrix;


//https://adventofcode.com/2022/day/14
internal class Program
{
    private static string _testData =
        @"Sensor at x=2, y=18: closest beacon is at x=-2, y=15
Sensor at x=9, y=16: closest beacon is at x=10, y=16
Sensor at x=13, y=2: closest beacon is at x=15, y=3
Sensor at x=12, y=14: closest beacon is at x=10, y=16
Sensor at x=10, y=20: closest beacon is at x=10, y=16
Sensor at x=14, y=17: closest beacon is at x=10, y=16
Sensor at x=8, y=7: closest beacon is at x=2, y=10
Sensor at x=2, y=0: closest beacon is at x=2, y=10
Sensor at x=0, y=11: closest beacon is at x=2, y=10
Sensor at x=20, y=14: closest beacon is at x=25, y=17
Sensor at x=17, y=20: closest beacon is at x=21, y=22
Sensor at x=16, y=7: closest beacon is at x=15, y=3
Sensor at x=14, y=3: closest beacon is at x=15, y=3
Sensor at x=20, y=1: closest beacon is at x=15, y=3"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        var debug = true;
        FirstPart(GetDataStream(debug), debug);
        SecondPart(GetDataStream(debug), debug);
    }

    private static TextReader GetDataStream(bool debug) =>
        debug
            ? StreamUtils.GetInputStream(testData: _testData)
            : StreamUtils.GetInputStream("input.txt");

    private static void SecondPart(TextReader stream, bool debug)
    {
        var matrix = LoadMatrix(
            stream,
            out var listOfSensors,
            out var listOfBeacons);

        (long minX, long minY, long maxX, long maxY) = matrix.UsedRange()!.Value;
        long fromX = 0;
        long fromY = fromX;
        long toX = debug ? 20 : 4000000;
        long toY = toX;
        Stopwatch sw = new();
        sw.Start();
        var startTime = DateTime.Now;

        for (long yix = fromY; yix < toY + 1; yix++)
        {
            if (sw.Elapsed.Seconds > 10)
            {
                Debug.WriteLine($"{DateTime.Now} {DateTime.Now - startTime}: {yix}");
                sw.Restart();
            }
            var line = new VisibilityLine(yix);


            foreach (var sensor in listOfSensors)
            {
                var visiblePartOfLine = sensor.VisiblePartOfLine(yix);
                if (visiblePartOfLine.start > visiblePartOfLine.end)
                    continue;
                line.Insert(visiblePartOfLine);
            }

            var inverseLine = line.Inverse(fromX, toX);
            if (inverseLine.Intervals.Count > 0)
            {
                Debug.WriteLine($"{yix}: {inverseLine}");
                if (inverseLine.Intervals.Count == 1)
                {
                    var interval = inverseLine.Intervals.First();
                    if (interval.End == interval.Start)
                    {
                        var freq = 4000000 * interval.Start + yix;
                        Debug.WriteLine("Freq=" + freq);
                    }
                }
            }
        }


    }


    private static void FirstPart(TextReader stream, bool debug)
    {
        var matrix = LoadMatrix(
            stream,
            out var listOfSensors,
            out var listOfBeacons);

        (long minX, long minY, long maxX, long maxY) = matrix.UsedRange()!.Value;
        var width = maxX - minX;
        var height = maxY - minY;
        long y = debug ? 10 : 2_000_000;
        var line = new VisibilityLine(y);
        foreach (var sensor in listOfSensors)
        {
            var visiblePartOfLine = sensor.VisiblePartOfLine(y);
            if (visiblePartOfLine.start > visiblePartOfLine.end)
                continue;
            line.Insert(visiblePartOfLine);
        }

        var visibleBeacons = listOfBeacons.Sum(b => line.IsVisibleInLine(b.X, b.Y) ? 1 : 0);
        Debug.WriteLine(line);
        Debug.WriteLine(line.Intervals.Sum(x => x.End - x.Start + 1) - visibleBeacons);

    }

    public class VisibilityLine
    {
        public class Interval
        {
            public long Start;
            public long End;
            public Interval(long start, long end)
            {
                Start = start;
                End = end;
            }
        }

        private long lineY;

        public VisibilityLine Inverse(long start, long end)
        {
            VisibilityLine? newLine;
            if (Intervals.Count == 0)
            {
                newLine = new VisibilityLine(lineY);
                newLine.Intervals.Add(new Interval(start, end));
                return newLine;
            }
            var sorted = Intervals.OrderBy(x => x.Start).ToList();
            newLine = new VisibilityLine(lineY);
            var prev = sorted.First();
            if (start < prev.Start - 1)
            {
                newLine.Intervals.Add(new Interval(start, prev.Start - 1));
            }

            foreach (var interval in sorted.Skip(1))
            {
                if (interval.Start > end) break;
                if (prev.End + 1 <= interval.Start - 1)
                    newLine.Intervals.Add(new Interval(prev.End + 1, interval.Start - 1));
            }

            var last = newLine.Intervals.LastOrDefault();
            if (last != null && last.End > end)
                last.End = end;
            if (last != null && last.Start > last.End)
                newLine.Intervals.Remove(last);
            return newLine;
        }
        public void Insert((long start, long end) visible)
        {
            var midIntervals = Intervals.Where(i => i.Start > visible.start && i.End < visible.end).ToList();
            if (midIntervals.Any())
            {
                //will be eaten
                midIntervals.ForEach(r => Intervals.Remove(r));
            }

            var startInterval = Intervals.FirstOrDefault(i => i.Start <= visible.start && i.End >= visible.start);
            var endInterval = Intervals.FirstOrDefault(i => i.Start <= visible.end && i.End >= visible.end);
            if (startInterval == null && endInterval == null)
            {
                // start/end not in other
                Intervals.Add(new Interval(visible.start, visible.end));
            }
            else if (startInterval != null && startInterval == endInterval)
            {
                // fully inside one
            }
            else if (startInterval != null && endInterval != null && endInterval != startInterval)
            {
                // start in one and end in another
                startInterval.End = endInterval.End;
                Intervals.Remove(endInterval);
            }
            else if (startInterval == null && endInterval != null)
            {
                // ends in one, adjust its start
                endInterval.Start = visible.start;
            }
            else if (startInterval != null && endInterval == null)
            {
                //starts in one, adjust its end
                startInterval.End = visible.end;
            }
            else
                throw new InvalidDataException("Logic for intervals failed");
        }

        public List<Interval> Intervals = new();

        public VisibilityLine(long lineY)
        {
            this.lineY = lineY;
        }

        public override string ToString()
        {
            var sorted = Intervals.OrderBy(x => x.Start);
            return "" + sorted.Select(x => $"({x.Start},{x.End}[{x.End - x.Start + 1}])").StringJoin(",");
        }

        public bool IsVisibleInLine(long bX, long bY)
        {
            if (bY != lineY) return false;
            return Intervals.Any(i => i.Start <= bX && i.End >= bX);
        }
    }
    private static SparseMatrix<CellContent> LoadMatrix(
        TextReader stream,
        out List<Sensor> listOfSensors,
        out List<Beacon> listOfBeacons)
    {
        //   0    1 2 3  4  5    6       7     8  9 0 1   2 3
        //Sensor at x=9, y=16: closest beacon is at x=10, y=16
        var matrix = new SparseMatrix<CellContent>();
        listOfSensors = new List<Sensor>();
        listOfBeacons = new List<Beacon>();
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Split(",=: ".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var beaconX = parts[11].ToLong()!.Value;
            var beaconY = parts[13].ToLong()!.Value;
            var sensorX = parts[3].ToLong()!.Value;
            var sensorY = parts[5].ToLong()!.Value;

            var sensor = new Sensor(sensorX, sensorY, beaconX, beaconY);
            listOfSensors.Add(sensor);

            var beacon = listOfBeacons.FirstOrDefault(x => x.X == beaconX && x.Y == beaconY);
            if (beacon == null)
            {
                beacon = new Beacon(beaconX, beaconY);
                listOfBeacons.Add(beacon);
            }

            matrix.Value(sensor.X, sensor.Y, sensor);
            matrix.Value(beacon.X, beacon.Y, beacon);
        }
        return matrix;
    }
}


internal class Beacon : CellContent
{
    public Beacon(long x, long y)
        : base(x, y)
    {
    }
}
public class Sensor : CellContent
{
    public long BeaconX { get; }
    public long BeaconY { get; }
    public long DistanceToBeacon { get; }

    public Sensor(long x, long y,
        long beaconX, long beaconY)
        : base(x, y)
    {
        BeaconX = beaconX;
        BeaconY = beaconY;
        DistanceToBeacon = DistanceTo(beaconX, beaconY);
    }
    public long DistanceTo(long x, long y) => Math.Abs(X - x) + Math.Abs(Y - y);

    public bool CanSee(long x, long y)
    {
        return (DistanceTo(x, y) <= DistanceToBeacon);
    }
    public (long start, long end) VisiblePartOfLine(long y)
    {
        var widthAtCenter = DistanceToBeacon * 2 + 1;
        var lineOffset = Math.Abs(y - Y);
        var visibleWidth = widthAtCenter - (2 * lineOffset);

        return (start: X - (visibleWidth / 2), end: X + (visibleWidth / 2));
    }
}


public class CellContent
{
    protected CellContent(long x, long y)
    {
        X = x;
        Y = y;
    }
    public long X { get; protected set; }
    public long Y { get; protected set; }
}