using common.SparseMatrix;
using System.Xml.Linq;

namespace _22;

internal class SideConnection
{
    public int Turn { get; }
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