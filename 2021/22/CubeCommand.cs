using common;

namespace _22;

public class CubeCommand
{
    public int Cmd { get; set; }
    public int? X1 { get; set; }
    public int? X2 { get; internal set; }
    public int? Y1 { get; internal set; }
    public int? Y2 { get; internal set; }
    public int? Z1 { get; internal set; }
    public int? Z2 { get; internal set; }


    public static IEnumerable<CubeCommand> LoadStream(TextReader stream)
    {
        var res = new List<CubeCommand>();
        if (stream.Peek() != -1)
        {
            do
            {
                var c = new CubeCommand();
                c.Cmd = stream.ReadWord().Trim() == "on" ? 1 : 0;
                stream.SkipOver("x=");
                c.X1 = stream.ReadInt();
                stream.SkipOver("..");
                c.X2 = stream.ReadInt();
                stream.SkipOver(",y=");
                c.Y1 = stream.ReadInt();
                stream.SkipOver("..");
                c.Y2 = stream.ReadInt();
                stream.SkipOver(",z=");
                c.Z1 = stream.ReadInt();
                stream.SkipOver("..");
                c.Z2 = stream.ReadInt();
                res.Add(c);

            } while (stream.Peek() != -1);
        }
        return res;
    }

    public void Execute(Cuboid coreCube)
    {
        var values = new[] { X1, X2, Y1, Y2, Z1, Z2 };
        if (values.All(x => x.HasValue))
        {
            var limits = new Cuboid(X1.Value, X2.Value, Y1.Value, Y2.Value, Z1.Value, Z2.Value);
            var intersection = coreCube.Intersection(limits);
            if (intersection != null)
            {
                var sub = coreCube.MakeSub(intersection.Value, (ECuboidState)Cmd);
                Console.WriteLine(sub);
            }
            else
            {
                Console.WriteLine($"Outside: "+limits);
            }
        }

    }
}