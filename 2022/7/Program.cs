using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using common;

//https://adventofcode.com/2022/day/7

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
    { Debug.WriteLine($"result2 :{res}");
    }

    private static void FirstPart(TextReader stream)
    {
        Debug.WriteLine($"result1 :{res}");
    }
}