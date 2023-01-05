using common.SparseMatrix;
using System.Xml.Linq;

namespace _22;

internal class SideConnection
{
    public int Turn { get; } // number of clockwise turns needed to align the two local coordinate systems
    public Side Side { get; }

    public override string ToString()
    {
        return $"{Side?.Name} {Turn}";
    }
    public SideConnection(int turn, Side side)
    {
        Turn = turn;
        Side = side;
    }
}