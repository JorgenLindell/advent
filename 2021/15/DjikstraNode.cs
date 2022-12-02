using System.Diagnostics;
using common;

namespace _15;
[DebuggerDisplay("({Pos.r},{Pos.c}):{Value}")]
public  class DjikstraNode :ICell
{
    Matris<DjikstraNode>? _matris;
    public (int r, int c) Pos { get; set; }
    public int Value { get; set; }
    private List<DjikstraNode>? _nextCells;
    public List<DjikstraNode> NextCells
    {
        get
        {
            if (_nextCells == null)
            {
                _matris = _matris ?? throw new InvalidDataException("null matris");
                _nextCells = Pos.GetAdjacentStraight(_matris)
                    .Select(x => (DjikstraNode)_matris.Value(x))
                    .OrderBy(c => c.Value)
                    .ToList()!;
            }

            return _nextCells;
        }
        set => _nextCells=value;
    }

    public DjikstraNode CameFrom { get; set; }
    public int SumValue { get; set; }

    public DjikstraNode()
    {
        Pos = (-1, -1);
        Value = -1;
        CameFrom = null;
        SumValue = Int32.MaxValue;
    }
    public DjikstraNode((int r, int c) pos, int risk, Matris<DjikstraNode> matris)
    {
        Pos = pos;
        Value = risk;
        _matris = matris;
    }


}