using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace common;

public struct DistanceKey
{
    public float Dist;
    public Vector3 Key;
    public int[] Beacons;

    public DistanceKey(float dist, Vector3 vector, int[] beacons)
    {
        Dist = dist;
        Key = vector;
        Beacons = beacons;
    }
}
public static class Util3D
{
    public static Func<Vector3, Vector3>[] Rotations { get; }
    public static Func<Vector3, Vector3>[] ReverseRotations { get; set; }
    static Util3D()
    {
        Rotations = GenerateRotations().Select(x => x.Item1).ToArray();
        ReverseRotations = GenerateRotations().Select(x => x.Item2).ToArray();
    }



    public static DistanceKey DistanceAsKey(Vector3 p1, Vector3 p2, int beacon1, int beacon2)
    {
        var dist = p1 - p2;
        var dlist = new[]
        {
            Math.Abs(dist.X ),
            Math.Abs(dist.Y ),
            Math.Abs(dist.Z ),
        }
            .OrderByDescending(x => x)
            .Select(x => (int)x).ToArray();
        return new DistanceKey(Distance(p1, p2), new Vector3(dlist[0], dlist[1], dlist[2]), new[] { Math.Min(beacon1, beacon2), Math.Max(beacon1, beacon2) });
    }

    private static float Distance(Vector3 p1, Vector3 p2)
    {
        return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
    }


    private static IEnumerable<(Func<Vector3, Vector3>, Func<Vector3, Vector3>)> GenerateRotations()
    {
        yield return (v => new(v.X, v.Y, v.Z), v => new(v.X, v.Y, v.Z));
        yield return (v => new(v.Z, v.Y, -v.X), v => new(-v.Z, v.Y, v.X));
        yield return (v => new(-v.X, v.Y, -v.Z), v => new(-v.X, v.Y, -v.Z));
        yield return (v => new(-v.Z, v.Y, v.X), v => new(v.Z, v.Y, -v.X));
        yield return (v => new(v.X, -v.Z, v.Y), v => new(v.X, v.Z, -v.Y));
        yield return (v => new(v.Y, -v.Z, -v.X), v => new(-v.Z, v.X, -v.Y));
        yield return (v => new(-v.X, -v.Z, -v.Y), v => new(-v.X, -v.Z, -v.Y));
        yield return (v => new(-v.Y, -v.Z, v.X), v => new(v.Z, -v.X, -v.Y));
        yield return (v => new(v.X, -v.Y, -v.Z), v => new(v.X, -v.Y, -v.Z));
        yield return (v => new(-v.Z, -v.Y, -v.X), v => new(-v.Z, -v.Y, -v.X));
        yield return (v => new(-v.X, -v.Y, v.Z), v => new(-v.X, -v.Y, v.Z));
        yield return (v => new(v.Z, -v.Y, v.X), v => new(v.Z, -v.Y, v.X));
        yield return (v => new(v.X, v.Z, -v.Y), v => new(v.X, -v.Z, v.Y));
        yield return (v => new(-v.Y, v.Z, -v.X), v => new(-v.Z, -v.X, v.Y));
        yield return (v => new(-v.X, v.Z, v.Y), v => new(-v.X, v.Z, v.Y));
        yield return (v => new(v.Y, v.Z, v.X), v => new(v.Z, v.X, v.Y));
        yield return (v => new(-v.Y, v.X, v.Z), v => new(v.Y, -v.X, v.Z));
        yield return (v => new(v.Z, v.X, v.Y), v => new(v.Y, v.Z, v.X));
        yield return (v => new(v.Y, v.X, -v.Z), v => new(v.Y, v.X, -v.Z));
        yield return (v => new(-v.Z, v.X, -v.Y), v => new(v.Y, -v.Z, -v.X));
        yield return (v => new(-v.Y, -v.X, -v.Z), v => new(-v.Y, -v.X, -v.Z));
        yield return (v => new(-v.Z, -v.X, v.Y), v => new(-v.Y, v.Z, -v.X));
        yield return (v => new(v.Y, -v.X, v.Z), v => new(-v.Y, v.X, v.Z));
        yield return (v => new(v.Z, -v.X, -v.Y), v => new(-v.Y, -v.Z, v.X));
    }
    private static IEnumerable<Quaternion> AllQuaternion90()
    {
        var rot1 = Quaternion.CreateFromYawPitchRoll(0f, 0f, 90f * MathF.PI / 180f);
        var yaw1 = Quaternion.CreateFromYawPitchRoll(90f * MathF.PI / 180f, 0f, 0f);
        var pit1 = Quaternion.CreateFromYawPitchRoll(0f, 90f * MathF.PI / 180f, 0f);

        var rot = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f); ;
        for (int r = 0; r < 4; r++)
        {
            var pit = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f); ;
            for (int p = 0; p < 4; p++)
            {
                var yaw = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f); ;
                for (int j = 0; j < 4; j++)
                {
                    yield return (yaw * pit * rot);
                    yaw *= yaw1;
                }
                pit *= pit1;
            }
            rot *= rot1;
        }
    }

    public static List<(int X, int Y, int Z, Quaternion Q, Vector3 V)> DistinctIntRotations(Vector3 vector3)
    {
        return AllQuaternion90()
            .Select(
                q => (
                    Q: q,
                    T: vector3.Transform(q))
            )
            .Select(a => (
                X: (int)MathF.Round(a.T.X),
                Y: (int)MathF.Round(a.T.Y),
                Z: (int)MathF.Round(a.T.Z),
                Q: a.Q,
                V: a.T))
            .DistinctBy(a => $"{a.X}{a.Y}{a.Z}")
            .ToList();
    }
}