using System.Security.Cryptography;

namespace _16;

public struct XYCoordSystem<T>
{
    private readonly int _offsetX = 0;
    private readonly int _offsetY = 0;
    private readonly int _rHeight = 0;
    private readonly int _rWidth = 0;
    public readonly ((int x, int y) p1, (int x, int y) p2) VirtualSpan => (p1: (x: 0 - _offsetX, y: 0 - _offsetY), p2: (x: _rWidth - _offsetX,y: _rHeight - _offsetY));
    public T[,] Matr { get; }

    public XYCoordSystem(int x, int y)
    {
        Matr = new T[x, y];
        _rHeight = y;
        _rWidth = x;
        _offsetX = 0;
        _offsetY = 0;
    }

    public XYCoordSystem(int rWidth, int rHeight, int offsetX, int offsetY)
        : this(rWidth, rHeight)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;
    }

    public T this[int x, int y]
    {
        get
        {
            var yCoord = _rHeight - (y + _offsetY)-1;
            var xCoord = x + _offsetX;
            return Matr[xCoord, yCoord];
        }
        set
        {
            var yCoord = _rHeight - (y + _offsetY)-1;
            var xCoord = x + _offsetX;
            Matr[xCoord, yCoord] = value;
        }
    }
}