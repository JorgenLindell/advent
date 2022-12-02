using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using _19;
using common;
using Xunit;

namespace Test.Test
{
    public class Tests3D
    {
        [Fact]
        public void TestRotations()
        {
            (string was, (string, string)) f(string pref, int val, string target)
            {
                var sign = (Math.Sign(val) < 0 ? "-" : "");
                switch ((int)Math.Abs(val))
                {
                    case 1:
                        return ("X", ($"{sign}{pref}X", $"{sign}{pref}{target}"));
                    case 2:
                        return ("Y", ($"{sign}{pref}Y", $"{sign}{pref}{target}"));
                    case 3:
                        return ("Z", ($"{sign}{pref}Z", $"{sign}{pref}{target}"));
                    default:
                        return (target, ("error", "error"));
                }
            }
            var rotations = Util3D.DistinctIntRotations(new Vector3(1, 2, 3));

            var res = "";
            rotations.ForEach(
                a =>
                {
                    var strings = new Dictionary<string, (string, string)>();
                    var s1 = f("v.", a.X, "X");
                    strings.Add(s1.Item1, s1.Item2);
                    var s2 = f("v.", a.Y, "Y");
                    strings.Add(s2.Item1, s2.Item2);
                    var s3 = f("v.", a.Z, "Z");
                    strings.Add(s3.Item1, s3.Item2);
                    var r1 = strings["X"];
                    var r2 = strings["Y"];
                    var r3 = strings["Z"];

                    res += $@"        yield return (v => new({s1.Item2.Item1},{s2.Item2.Item1}, {s3.Item2.Item1}),"
                           + $@"v => new({r1.Item2},{r2.Item2}, {r3.Item2}));" + "\n";
                });
            Debug.WriteLine(res);
        }

        [Fact]
        public void TestRotation()
        {
            var stream = StreamUtils.GetInputStream(testData: TestDataSet1.Data);
            var scanners = ScannerFactory.ParseScanners(stream);

            var first = scanners[0];
            var second = scanners[1];
            var distances1 = first.CalculateDistances(first.CurrentBeacons);
            var distances2 = second.CalculateDistances(second.CurrentBeacons);
            var otherFirst = distances2.First();
            var tested = 0;
            var foundTransforms = new List<(int Transform, Vector3 Offset)>();
            foreach (var d1 in distances1)
                foreach (var d2 in distances2)
                {
                    tested++;
                    var absdiff = Math.Abs(d1.Dist - d2.Dist);
                    if (absdiff < 1.0)
                    {
                        Debug.WriteLine($"{d1.Dist} {d1.Key}  {d1.Beacons[0]},{d1.Beacons[1]}   {d2.Dist} {d2.Key}  {d2.Beacons[0]},{d2.Beacons[1]}   diff:{d1.Key - d2.Key} {absdiff}    {(absdiff < 1 ? "HIT" : "")}");
                        var firstScannerCoords = new[]
                            { first.BeaconCoordsInTransform(d1.Beacons[0]), first.BeaconCoordsInTransform(d1.Beacons[1]) };
                        var found = second.FindMatchingRotation(d2.Beacons, firstScannerCoords);
                        if (found.Transform >= 0)
                        {
                            foundTransforms.Add(found);
                            Debug.WriteLine($"Found matching point for rotation {found} offset:{found.Offset} ");
                            if (foundTransforms.Count > Scanner.NumberOfKeyPoints
                                && foundTransforms.Count(x => x.Offset == found.Offset) == Scanner.NumberOfKeyPoints)
                            {
                                //return found;
                            }
                        }
                    }
                }
        }
    }
}

