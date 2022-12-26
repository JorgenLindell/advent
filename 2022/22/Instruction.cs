internal class Instruction
{
    public enum Rotate { Left = -1, Right = +1 }
    public int Steps { get; }
    public Rotate Turn { get; }

    public Instruction(int steps, char turn)
    {
        Steps = steps;
        Turn = turn == 'L' ? Rotate.Left : Rotate.Right;
    }

    public override string ToString() => $"{Steps} {Turn}";
}