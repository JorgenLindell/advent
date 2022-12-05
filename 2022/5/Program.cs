using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using common;

namespace _5;
//https://adventofcode.com/2022/day/5

internal class Program
{
    private static string _testData =
        @"    [D]    
[N] [C]    
[Z] [M] [P]
 1   2   3 

move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2"
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
        var stacks = LoadStacks(stream);
        var moves = GetMoves(stream);
        foreach (var move in moves)
        {
            for (int i = 0; i < move.cnt; i++)
            {
                stacks[move.to].Push(stacks[move.from].Pop());
            }
        }

        var final = GetResult(stacks);
        Debug.WriteLine($"result1 :{final} ");

    }
    private static void SecondPart(TextReader stream)
    {
        var stacks = LoadStacks(stream);
        var moves = GetMoves(stream);
        foreach (var move in moves)
        {
            var tempStack = new Stack<string>();
            for (int i = 0; i < move.cnt; i++)
            {
                tempStack.Push(stacks[move.from].Pop());
            }

            for (int i = 0; i < move.cnt; i++)
            {
                stacks[move.to].Push(tempStack.Pop());
            }
        }

        var final = GetResult(stacks);
        Debug.WriteLine($"result2 :{final} ");
    }

    private static string GetResult(Dictionary<string, Stack<string>> stacks)
    {
        var final = "";
        foreach (var stack in stacks)
        {
            final += stack.Value.Peek();
        }

        return final;
    }

    private static List<(int cnt, string from, string to)> GetMoves(TextReader stream)
    {
        var moves = new List<(int cnt, string from, string to)>();
        while (stream.ReadLine() is { } inpLine)
        {
            var parts = inpLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            moves.Add((
                parts[1].ToInt()!.Value,
                parts[3].Trim(),
                parts[5].Trim()
            ));
        }

        return moves;
    }

    private static Dictionary<string, Stack<string>> LoadStacks(TextReader stream)
    {
        var loadstacks = new List<string>();
        while (stream.ReadLine() is { } inpLine)
        {
            if (inpLine.Trim() == "") break;
            loadstacks.Add(inpLine);
        }

        var lastLine = loadstacks.Last().Trim();
        var stackNames = lastLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var stackCount = stackNames.Length;
        var entryLength = 4;
        var stacks = new Dictionary<string, Stack<string>>();
        foreach (var n in stackNames)
            stacks[n] = new Stack<string>();
        loadstacks.RemoveAt(loadstacks.Count - 1);
        loadstacks.Reverse();

        foreach (var stackLine in loadstacks)
        {
            var entries = stackLine.Chunk(entryLength);
            entries.ForEach((e, i) =>
            {
                if (e[1] != ' ')
                    stacks["" + (i + 1)].Push("" + e[1]);
            });
        }

        return stacks;
    }



}