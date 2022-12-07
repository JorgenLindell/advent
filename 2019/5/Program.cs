using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

using common;


//https://adventofcode.com/2019/day/5

internal class Program
{
    private static string _testData =
        @"1002,4,3,4,33"
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
    }


    private static void FirstPart(TextReader stream)
    {
        var cells = stream.ReadLine()!
            .Split(',')
            .Select(x => x.ToLong()!.Value)
            .ToArray();
        var machine = new IntCodeMachine2019(cells);
        var result= machine.Evaluate();
        Debug.WriteLine($"result1: " + result[0]);
    }
}