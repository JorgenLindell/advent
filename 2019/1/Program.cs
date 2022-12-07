using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using common;

//https://adventofcode.com/2019/day/1

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
        long sum = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            var number = inpLine.ToLong()!.Value;
            var res1 = CalcFuel2(number);
            sum += res1;
        }
        Debug.WriteLine($"result1: {sum}");
    }

    private static long CalcFuel2(long number)
    {
        long sum = 0;
        var res = CalcFuel1(number);
        while (res > 0)
        {
            sum += res;
            res = CalcFuel1(res);
        }
        return sum;
    }

    private static void FirstPart(TextReader stream)
    {
        long sum = 0;
        while (stream.ReadLine() is { } inpLine)
        {
            var number = inpLine.ToLong()!.Value;
            var res1 = CalcFuel1(number);
            sum += res1;
        }
        Debug.WriteLine($"result1: {sum}");
    }

    private static long CalcFuel1(long number)
    {
        var res1 = (number / 3L) - 2;
        return res1;
    }
}