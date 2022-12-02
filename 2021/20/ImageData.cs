using System.Diagnostics;
using common;

namespace _20;

public class ImageData
{
    public int[] Enhancer = Array.Empty<int>();
    public Matris<int> Image = new Matris<int>(5, 5, x => 0);

    public string ExtractPoints(int r, int c, int size)
    {
        var points = "";
        int offset = size / 2;
        for (int r2 = r - offset; r2 < r + offset + 1; r2++)
        {
            for (int c2 = c - offset; c2 < c + offset + 1; c2++)
            {
                points += $"{Image.Value((r2, c2))}";
            }
        }
        return points;
    }

    public void Deconstruct(out int[] enhancer, out Matris<int> image)
    {
        enhancer = this.Enhancer;
        image = this.Image;
    }
}