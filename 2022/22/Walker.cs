internal class Walker
{
    private readonly MonkeyMap _map;
    public List<GlobalPosition> Track = new();
    public Walker(GlobalPosition startPosition, Offset offset, MonkeyMap map)
    {
        _map = map;
        Pos = startPosition;
        Offset = offset;
    }

    public GlobalPosition Pos { get; set; }
    public Offset Offset { get; private set; }

    public void Execute(Instruction instr)
    {
        var increment = GlobalPosition.Offsets[(int)Offset];
        for (int i = 0; i < instr.Steps; i++)
        {
            var next = Pos + increment;
            var isEmpty = _map.IsEmpty(next);
            if (!isEmpty)
            {
                var tile = _map.Value(next);
                if (tile!.Typ == Tile.Types.Edge)
                    tile = tile.GetRealTile(increment);
                if (tile.Typ == Tile.Types.None)
                {
                    Pos = tile.Pos;
                    Track.Add(Pos);
                }
                else break;
            }
            if (isEmpty)
            {
                // now we are outside map...
                throw new Exception("Crazy");
            }
        }

        int intOffset = (int)Offset;
        intOffset += ((int)instr.Turn) + Position.Offsets.Length * 2;
        intOffset %= Position.Offsets.Length;
        Offset = (Offset)(intOffset);
    }

}