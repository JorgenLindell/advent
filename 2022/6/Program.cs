using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using common;

namespace _5;
//https://adventofcode.com/2022/day/6

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
        //var stream = StreamUtils.GetInputStream("input.txt");
        var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }
    private static void SecondPart(TextReader stream)
    {
        Debug.WriteLine($"result2 :");
    }

    private static void FirstPart(TextReader stream)
    {
       
        Debug.WriteLine($"result1 :");

    }
}