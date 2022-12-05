using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.CompilerServices;
using common;

namespace _22;

public enum ECuboidState
{
    Off,
    On, //ordinary
    Inverted,
    Excluded //special for subCuboids
}

public interface ICuboid
{
}

public class Cuboid : ICuboid
{
    public Limits3d<int> Limits = new();

    public List<SubCuboid> SubCuboids { get; } = new();

    public Cuboid(Limits<int> xLimits, Limits<int> yLimits, Limits<int> zLimits)
    {
        Limits = new Limits3d<int>(xLimits, yLimits, zLimits);
    }
    public Cuboid(
        int x1, int x2,
        int y1, int y2,
        int z1, int z2
    )
    {
        Limits = new Limits3d<int>(x1, x2, y1, y2, z1, z2);
    }

    protected Cuboid(Limits3d<int> limits)
    {
        Limits = limits;
    }

    public virtual ECuboidState State { get; set; } = 0;

    public (long X, long Y, long Z) Size =>
           (X: Limits.XHigh - Limits.XLow + 1,
            Y: Limits.YHigh - Limits.YLow + 1,
            Z: Limits.ZHigh - Limits.ZLow + 1);

    public long Count
    {
        get
        {
            var s = this.Size;
            return s.X * s.Y * s.Z;
        }
    }

    public long CountCells(ECuboidState state)
    {
        var subSizes = SubCuboids.Sum(s => s.Count);
        if (State == state)
        {

            var subMatched = SubCuboids.Where(s => s.State == state).Sum(s => s.Count);
            return Count - subSizes + subMatched;
        }
        else
        {
            var subMatched = SubCuboids.Where(s => s.State == state).Sum(s => s.Count);
            return subMatched;
        }

    }

    public bool IntersectsWith(Cuboid other)
    {
        var otherLimits = other.Limits;
        return IntersectsWith(otherLimits);
    }

    public bool IntersectsWith(Limits3d<int> otherLimits)
    {
        var x = Limits.x.Intersects(otherLimits.x);
        var y = Limits.y.Intersects(otherLimits.y);
        var z = Limits.z.Intersects(otherLimits.z);
        return x && y && z;
    }

    public bool IntersectsWith(int x, int y, int z)
    {
        var xb = x.WithInOrdered(Limits.x!.Value);
        var yb = y.WithInOrdered(Limits.y!.Value);
        var zb = z.WithInOrdered(Limits.z!.Value);
        return xb && yb && zb;
    }

    public virtual Limits3d<int>? Intersection(Cuboid other)
    {
        var otherLimits = other.Limits;
        return Intersection(otherLimits);
    }

    public Limits3d<int>? Intersection(Limits3d<int> otherLimits)
    {
        return Limits.Intersection(otherLimits);
    }

    internal SubCuboid? MakeSub(Limits3d<int> newSub, ECuboidState state)
    {
        var found = SubCuboids.Where(x => x.Limits == newSub).ToList();
        SubCuboid? newSubCube = null;
        if (found.Count > 0)
        {
            newSubCube = found.First();
            newSubCube.State = state;
        }
        else
        {
            var subCuboids = SubCuboids.ToList();
            foreach (var subCuboid in subCuboids)
            {
                var splitLines = subCuboid.Limits.Intersection(newSub);
                if (splitLines != null)
                {
                    var replacements = subCuboid.Split(newSub);
                    //      Console.WriteLine($"Remove {subCuboid.State} {subCuboid.Limits}");
                    var collection = replacements.Where(r => r.Count > 0).ToList();
                    //      collection.ForEach((c, i) =>
                    //          Console.WriteLine($"Add {c.State} {c.Limits}")
                    //          );
                    var replacementsCount = collection.Sum(s => s.Count);
                    if (replacementsCount != subCuboid.Count)
                    {
                        Console.WriteLine($"Bad replacement {subCuboid.Count} {replacementsCount}");
                    }
                    collection.ForEach(c =>
                    {
                        if (!c.Limits.InsideOf(subCuboid.Limits))
                        {
                            Console.WriteLine($"Bad replacement replacement outside of parent {c.Limits}  {subCuboid.Limits}");
                        }
                    });
                    VerifyNoOverlaps(collection.Select(x => x.Limits).ToList(), "In split:");

                    SubCuboids.Remove(subCuboid);
                    SubCuboids.AddRange(collection.Where(x=>x.State!=State));
                    //VerifyNoOverlaps(SubCuboids.Select(x => x.Limits).ToList(), "General direct after split");
                }
            }

            var old = SubCuboids.Where(s=>  s.Limits.InsideOf(newSub)).ToList();
            old.ForEach(s =>
            {
                //      Console.WriteLine($"= Remove {s.State} {s.Limits}");
                SubCuboids.Remove(s);
            });
            if (state != State)
            {
                newSubCube = new SubCuboid(this, newSub)
                {
                    State = state
                };
                // Console.WriteLine($"= Add {newSubCube.State} {newSubCube.Limits}");

                SubCuboids.Add(newSubCube);
            }
            // VerifyNoOverlaps(SubCuboids.Select(x => x.Limits).ToList(), "General after split");
        }
        return newSubCube;

    }

    private void VerifyNoOverlaps(List<Limits3d<int>> elements, string message)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            for (int j = i + 1; j < elements.Count; j++)
            {
                if (elements[i].IntersectsWith(elements[j]))
                {
                    Console.WriteLine(
                        $"{message} Overlap {i} {elements[i]} and {j} {elements[j]}  {elements[i].Intersection(elements[j])}");
                }
            }
        }
    }

    public override string ToString()
    {
        return $"{(int)State} {Limits}";
    }

    public ECuboidState CellState(int x, int y, int z)
    {
        //is in subCuboid?
        var subCuboid = SubCuboids.Where(
            s => s.IntersectsWith(x, y, z))
            .ToList();
        if (SubCuboids.Count > 1) throw new InvalidDataException("Point in multiple subCuboids.");
        if (SubCuboids.Count == 0)
        {
            return State;
        }
        return SubCuboids.First().CellState(x, y, z);
    }

    public IEnumerable<(int X, int Y, int Z)> Cells()
    {
        for (int x = Limits.XLow; x <= Limits.XHigh; x++)
        {
            for (int y = Limits.YLow; y <= Limits.YHigh; y++)
            {
                for (int z = Limits.ZLow; z <= Limits.ZHigh; z++)
                {
                    yield return (x, y, z);
                }
            }
        }
    }

    public IEnumerable<((int x, int y, int z) Key, int Value)> GetValues(Limits3d<int>? intersection)
    {
        for (int x = Limits.XLow; x <= Limits.XHigh; x++)
        {
            for (int y = Limits.YLow; y <= Limits.YHigh; y++)
            {
                for (int z = Limits.ZLow; z <= Limits.ZHigh; z++)
                {
                    yield return (Key: (x, y, z), Value: ValueOf(x, y, z));
                }
            }
        }
    }

    private int ValueOf(int x, int y, int z)
    {
        if (!Limits.IntersectsWith(x, y, z)) return 0;
        foreach (var s in SubCuboids)
        {
            if (s.Limits.IntersectsWith(x, y, z))
                return s.State == ECuboidState.On ? 1 : 0;

        }

        return State == ECuboidState.On ? 1 : 0;
    }
}