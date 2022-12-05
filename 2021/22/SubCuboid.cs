using common;

namespace _22;

public class SubCuboid : Cuboid
{
    private ECuboidState _state;

    public SubCuboid(Cuboid parent, int x1, int x2, int y1, int y2, int z1, int z2)
        : base(x1, x2, y1, y2, z1, z2)
    {
        Parent = parent;
    }

    public SubCuboid(Cuboid parent, Limits3d<int> limits) : base(limits)
    {
        Parent = parent;
    }

    public SubCuboid(Cuboid parent, Limits<int> x, Limits<int> y, Limits<int> z)
        : base(x, y, z)
    {
        Parent = parent;
    }

    //   public override ECuboidState State
    //   {
    //       get => _state == ECuboidState.Inverted
    //           ? Parent.State == ECuboidState.On
    //               ? ECuboidState.Off
    //               : ECuboidState.On
    //           : _state;
    //       set => _state = value;
    //   }

    public Cuboid Parent { get; }

    public List<SubCuboid> Split(Limits3d<int> newSub)
    {
        List<SubCuboid> replacementCubes = SplitOneCube(Limits, State, newSub);

        return replacementCubes;

        List<SubCuboid> SplitOneCube(Limits3d<int> subToSplitLimits, ECuboidState eCuboidState, Limits3d<int> newSub)
        {
            List<SubCuboid> subCuboids = new List<SubCuboid>();

            Divide2(subToSplitLimits.x!.Value, newSub.x.Value)
                .ForEach(
                    (a, _) =>
                    {
                        var qx = new SubCuboid(Parent, a, Limits.y!.Value, Limits.z!.Value);
                        Divide2(qx.Limits.y!.Value, newSub.y.Value)
                            .ForEach(
                                (b, _) =>
                                {
                                    var qy = new SubCuboid(Parent, qx.Limits.x!.Value, b, qx.Limits.z!.Value);
                                    Divide2(qy.Limits.z!.Value, newSub.z.Value)
                                        .ForEach((c, _) =>
                                        {
                                            var qz = new SubCuboid(Parent, qy.Limits.x!.Value, qy.Limits.y!.Value, c);

                                            qz.State = eCuboidState;
                                            subCuboids.Add(qz);
                                        });
                                });
                    });


            return subCuboids;
        }

        IEnumerable<Limits<int>> Divide2(Limits<int> toSplit, Limits<int> newLimits)
        {
            if (toSplit.lower < newLimits.lower && newLimits.upper < toSplit.upper)
            {//Split in 3, new cuts through
                yield return new Limits<int>(toSplit.lower, newLimits.lower - 1);
                yield return new Limits<int>(newLimits.lower, newLimits.upper);
                yield return new Limits<int>(newLimits.upper + 1, toSplit.upper);
            }
            else if ((toSplit.lower >= newLimits.lower && toSplit.upper <= newLimits.upper)
                     || (toSplit.lower > newLimits.upper)
                     || (toSplit.upper < newLimits.lower))
            {//no split tosplit is fully inside or outside new
                yield return new Limits<int>(toSplit.lower, toSplit.upper);

            }
            else if (toSplit.lower < newLimits.lower && toSplit.upper <= newLimits.upper)
            {// split in 2, new overlaps start
                yield return new Limits<int>(toSplit.lower, newLimits.lower - 1);
                yield return new Limits<int>(newLimits.lower, toSplit.upper);

            }
            else if (newLimits.lower <= toSplit.lower && newLimits.upper <= toSplit.upper)
            {// split in 2, new overlaps end
                yield return new Limits<int>(toSplit.lower, newLimits.upper);
                yield return new Limits<int>(newLimits.upper + 1, toSplit.upper);
            }
            else
            {
                yield return new Limits<int>(toSplit.lower, toSplit.upper);
            }
        }

    }
}