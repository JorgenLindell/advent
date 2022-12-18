using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using common;

namespace _4;
//https://adventofcode.com/2022/day/4

internal class Program
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

    private static void FirstPart(TextReader stream)
    {
        char[,] trees = null;
        int[,] visible = null;
        int size = 0;

        var sum = 0L;
        var lineIndex = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            if (trees == null)
            {
                size = inpLine.Length;
                trees = new char[size, size];
                visible = new int[size, size];
            }
            inpLine.ForEach((t, i) => trees[lineIndex, i] = t);
            ++lineIndex;
        }

        for (int r = 0; r < size; r++)
        {
            visible[r, 0] = visible[r, size - 1] = 1;
            visible[0, r] = visible[size - 1, r] = 1;
        }



        for (int r = 1; r < size - 1; r++)
        {
            for (int c = 1; c < size - 1; c++)
            {
                visible[r, c] = checkVisibility(trees,r,c);
            }
        }

        sum = 0;
        foreach (var i in visible)
        {
            sum += i;
        }

        Debug.WriteLine($"result1 :{sum} ");
    }

    private static int checkVisibility(char[,] trees, int r, int c)
    {
        var height = trees[r, c];
        var hiddenDirections = 0;
        for (var r2 = r; r2 >= 0; --r2)
        {
            if (trees[r2, c] >= height)
            {
                hiddenDirections++;
                break;
            }
        }
        for (var r2 = r; r2 < trees.GetUpperBound(0); ++r2)
        {
            if (trees[r2, c] >= height)
            {
                hiddenDirections++;
                break;
            }
        }
        for (var c2 = c; c2 >= 0; --c2)
        {
            if (trees[r, c2] >= height)
            {
                hiddenDirections++;
                break;
            }
        }
        for (var c2 = c; c2 < trees.GetUpperBound(0); ++c2)
        {
            if (trees[r, c2] >= height)
            {
                hiddenDirections++;
                break;
            }
        }
        return hiddenDirections==4?0:1;
    }

    private static void SecondPart(TextReader stream)
    {

        Debug.WriteLine($"result2 :");
    }


}