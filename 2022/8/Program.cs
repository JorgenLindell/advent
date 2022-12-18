using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using common;

namespace _8;
//https://adventofcode.com/2022/day/8
public struct CellStruct
{
    public int height;
    public int visible;
    public int view;

    public CellStruct(int height, int visible, int view)
    {
        this.height = height;
        this.visible = visible;
        this.view = view;

    }
    public static implicit operator CellStruct((int, int, int) v)
    {
        return new CellStruct(v.Item1, v.Item2, v.Item3);
    }
}

public class Program
{
    private static string _testData =
        @"30373
25512
65332
33549
35390"
            .Replace("\r\n", "\n");


    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }
    private static void SecondPart(TextReader stream)
    {
        var best = (0, 0, 0);
        var trees = CellStructs(stream, out var size);

        for (var r = 1; r < trees.GetLength(0) - 1; r++)
            for (var c = 1; c < trees.GetLength(1) - 1; c++)
            {
                var view = (int)CheckViewable(trees, r, c);
                if (view > best.Item1)
                {
                    best = (view, r, c);
                };
            }

        Debug.WriteLine($"result2 :{best.Item1} [{best.Item2},{best.Item3}]");


        long CheckViewable(CellStruct[,] trees, int r, int c)
        {
            var height = trees[r, c].height;
            var up = HowFar((-1, 0));
            var down = HowFar((1, 0));
            var left = HowFar((0, -1));
            var right = HowFar((0, 1));

            var prod = up * down * left * right;
            return prod;

            long HowFar((int r, int c) delta)
            {
                var seen = 0;
                var deltaR = r + delta.r;
                var deltaC = c + delta.c;
                while (true)
                {
                    var limitedCell = LimitedCell(trees, deltaR, deltaC);
                    var foundHeight = limitedCell.height;
                    if (foundHeight < int.MaxValue) // not outside
                        seen++;

                    if (foundHeight >= height) // found a blocker
                    {
                        break;
                    }
                    deltaR += delta.r;
                    deltaC += delta.c;

                }
                return seen;
            }

             CellStruct LimitedCell(CellStruct[,] trees, int deltaR, int deltaC)
            {
                if (deltaR >= 0 && deltaR < trees.GetLength(0) &&
                    deltaC >= 0 && deltaC < trees.GetLength(1))
                    return trees[deltaR, deltaC];

                return new CellStruct(int.MaxValue, 0, 0);
            }

        }
    }

    private static void FirstPart(TextReader stream)
    {
        var trees = CellStructs(stream, out var size);

        for (var r = 0; r < size; r++)
        {
            trees[r, 0].visible = 1;
            trees[r, size - 1].visible = 1;
            trees[0, r].visible = 1;
            trees[size - 1, r].visible = 1;
        }


        for (var r = 1; r < size - 1; r++)
        {
            for (var c = 1; c < size - 1; c++)
            {
                trees[r, c].visible = CheckVisibility(trees, r, c);
            }
        }

        long sum = trees.Cast<CellStruct>().Sum(i => i.visible);

        Debug.WriteLine($"result1 :{sum} ");

        int CheckVisibility(CellStruct[,] trees, int r, int c)
        {
            var height = trees[r, c].height;
            var hiddenDirections = 0;

            for (var r2 = r - 1; r2 >= 0; --r2)
            {
                if (trees[r2, c].height < height) continue;
                hiddenDirections++;
                break;
            }
            for (var r2 = r + 1; r2 <= trees.GetUpperBound(0); ++r2)
            {
                if (trees[r2, c].height < height) continue;
                hiddenDirections++;
                break;
            }
            for (var c2 = c - 1; c2 >= 0; --c2)
            {
                if (trees[r, c2].height < height) continue;
                hiddenDirections++;
                break;
            }
            for (var c2 = c + 1; c2 <= trees.GetUpperBound(1); ++c2)
            {
                if (trees[r, c2].height < height) continue;
                hiddenDirections++;
                break;
            }
            return hiddenDirections == 4 ? 0 : 1; //visible from outside
        }
    }

    private static CellStruct[,] CellStructs(TextReader stream, out int size)
    {
        CellStruct[,] trees = null;
        size = 0;

        var lineIndex = 0;
        var inpLine = stream.ReadLine();
        size = inpLine?.Length ?? 0;
        trees = new CellStruct[size, size];
        if (size == 0) return trees;
        do
        {
            var index = lineIndex;
            inpLine!.ForEach((t, i) =>
            {
                trees[index, i] = (t - '0', 0, 0);
            });
            ++lineIndex;
        } while (null != (inpLine = stream.ReadLine()));

        return trees;
    }


}
