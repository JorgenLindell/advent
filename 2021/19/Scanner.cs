using System.Diagnostics;
using System.Numerics;
using common;

namespace _19;

public class Scanner
{
    public int Number = 1;
    public const int NumberOfKeyPoints = 5;
    public List<Vector3[]> Beacons { get; set; } = new List<Vector3[]>();
    public List<Vector3> CurrentBeacons
    {
        get
        {
            return Beacons.Select(x => x[Transform]+Offset).ToList();
        }
    }
    public Vector3 BeaconCoordsInTransform(int b, int t = -1) => Beacons[b][t < 0 ? Transform : t] + Offset;
    public List<DistanceKey> Distances { get; set; } = new();
    public bool Solved { get; set; }

    public int Transform = 0;
    public Vector3 Offset = Vector3.Zero;
    public Scanner()
    {
    }

    public Scanner(int number)
    {
        this.Number = number;
    }

    public Scanner(Scanner other)
    {
        Number = other.Number;
        Beacons = other.Beacons.Select(b => b).ToList();
        Distances = other.Distances.Select(d => new DistanceKey(d.Dist, d.Key, d.Beacons.ToArray())).ToList();
    }


    public void AddBeacon(Vector3 coord, List<Vector3> rotations)
    {
        rotations.Insert(0, coord);
        Beacons.Add(rotations.ToArray());
    }

    public void CalculateDistances()
    {
        Distances = CalculateDistances(CurrentBeacons);
    }

    public List<DistanceKey> CalculateDistances(List<Vector3> beacons)
    {
        return beacons.SelectMany(
                (b1, i1) => beacons
                    .Select((b2, i2)
                        => Util3D.DistanceAsKey(b1, b2, i1, i2)))
            .OrderBy(x => x.Dist)
            .Where(x => x.Dist > 1)
            .DistinctBy(x => new { a = x.Beacons[0], b = x.Beacons[1] })
            .ToList();
    }

    public (int Transform, Vector3 Offset) MatchKeyPointDistances(Scanner other)
    {
        var first = this;
        var second = other;
        Debug.WriteLine($"Compare:{Number} to {other.Number}");
        var distances1 = first.Distances;
        var distances2 = second.Distances;
        var tested = 0;
        var matched = 0;
        var otherFirst = distances2.First();
        var foundTransforms = new List<(int Transform, Vector3 Offset)>();
        foreach (var d1 in distances1)
        {
            foreach (var d2 in distances2)
            {
                tested++;
                var absDiff = Math.Abs(d1.Dist - d2.Dist);
                if (absDiff < 1.0)
                {
                    matched++;
                    Debug.WriteLine($"{d1.Dist} {d1.Key}  {d1.Beacons[0]},{d1.Beacons[1]}   {d2.Dist} {d2.Key}  {d2.Beacons[0]},{d2.Beacons[1]}   diff:{d1.Key - d2.Key} {absDiff}    {(absDiff < 1 ? "HIT" : "")}");
                    var firstScannerCoords = new[]
                        { first.BeaconCoordsInTransform(d1.Beacons[0]), first.BeaconCoordsInTransform(d1.Beacons[1]) };
                    var found = second.FindMatchingRotation(d2.Beacons, firstScannerCoords);
                    if (found.Transform >= 0)
                    {
                        foundTransforms.Add(found);
                        Debug.WriteLine($"Found matching point for rotation {found} offset:{found.Offset} ");
                        if (foundTransforms.Count > Scanner.NumberOfKeyPoints
                            && foundTransforms.Count(x => x.Offset.Equals(found.Offset)) >= Scanner.NumberOfKeyPoints)
                        {
                            
                            return found;
                        }
                    }
                }

                if (tested > Scanner.NumberOfKeyPoints * 10)
                {
                    //break;
                }
            }
        }

        return (-1, Vector3.Zero);
    }

    public Scanner Clone()
    {
        return new Scanner(this);
    }

    public (int Transform, Vector3 Offset) FindMatchingRotation(int[] d2Beacons, Vector3[] firstScannerCoord)
    {
        for (int i = 0; i < 2; i++)
        {
            var beacon1 = Beacons[d2Beacons[i]];
            var beacon2 = Beacons[d2Beacons[1-i]];
            for (int rot = 0; rot < beacon1.Length; rot++)
            {
                {
                    var coo0 = beacon1[rot];
                    var coo1 = beacon2[rot];
                    var diff =firstScannerCoord[0] - coo0  ;
                    var moved = firstScannerCoord[1] - diff;
                    if (moved == coo1)
                    {
                        return (Transform: rot, Offset: diff);
                    }
                }
            }
        }

        return (-1, Vector3.Zero);
    }


}