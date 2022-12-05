using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using common;

namespace _5;
//https://adventofcode.com/2019/day/2

internal class Program
{
    private static string _testData =
        @""
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
        var cells = stream.ReadLine()!
            .Split(',')
            .Select(x => x.ToLong()!.Value)
            .ToArray();

        var target = 19_690_720L;
        var limit = 70;
        for (int i = 0; i < limit; i++)
        {
            for (int j = 0; j < limit; j++)
            {
                var result = evaluate1(cells, i, j);
                if (result[0] == target)
                {
                    Debug.WriteLine($"result1: {result[0]} {i}  {j}");
                    return;
                }
                else
                {
                    Debug.WriteLine($"test: {result[0]} {i}  {j}");
                }
            }
        }
    }


    private static void FirstPart(TextReader stream)
    {
        var cells = stream.ReadLine()!
            .Split(',')
            .Select(x => x.ToLong().Value)
            .ToArray();
        var result = evaluate1(cells, 12, 2);
        Debug.WriteLine($"result1: " + result[0]);
    }

    private static long[] evaluate1(long[] inputCells, int noun, int verb)
    {
        var cells = inputCells.ToArray();
        cells[1] = noun;
        cells[2] = verb;
        var a = 0;
        while (a < cells.Length && cells[a] != 99)
        {
            switch (cells[a])
            {
                case 1: //add
                    {
                        var r = cells[cells[++a]] + cells[cells[++a]];
                        cells[cells[++a]] = r;
                    }
                    break;
                case 2: //mul
                    {
                        var r = cells[cells[++a]] * cells[cells[++a]];
                        cells[cells[++a]] = r;
                    }
                    break;

            }

            ++a;
        }
        return cells;
    }

}