﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using common;

namespace _5;
//https://adventofcode.com/2022/day/6

internal class Program
{
    private static string _testData =
        @"zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw" //11
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
        var line = stream.ReadLine().ToArray();
        var res = FirstUnique(14, line);
        Debug.WriteLine($"result2 :{res}");
    }

    private static void FirstPart(TextReader stream)
    {
        var line = stream.ReadLine().ToArray();
        var res=FirstUnique(4, line);
        Debug.WriteLine($"result1 :{res}");

    }

    private static int FirstUnique(int diffLength, char[] line)
    {
        for (int j = 0; j < line.Length; j++)
        {
            var different =
                line.Skip(j).Take(diffLength).GroupBy(x => x).Count();
            if (different == diffLength)
            {
                return j+diffLength;
            }

        }

        return -1;
    }
}