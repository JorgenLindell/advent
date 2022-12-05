using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using common;
using Xunit;

namespace Test.Test21
{
    public class Test21
    {
        public static int NewPos(int start, int roll)
        {
            start = BoardPos(start);
            return (start - 1 + roll) % 10 + 1;
        }

        private static int BoardPos(int start)
        {
            return (start - 1) % 10 + 1;
        }

        public static int ScoreM(int start, params int[] rolls)
        {
            int m = 0;
            int p = start;
            for (int i = 0; i < rolls.Length; i++)
            {
                 p = NewPos(p, rolls[i]);
              Debug.WriteLine(p);
            }

            return 0;

        }

        [Fact]
        public void Test1()
        {
            Debug.WriteLine(ScoreM(1, 3, 3, 3, 3, 3, 3));
        }
        [Fact]
        public void Testadd1()
        {
            var x = 7;
            x.AddOneBasedModular(3, 10);
            x.AddOneBasedModular(1, 10);
            Debug.WriteLine(x.AddOneBasedModular(1, 10));
        }
        [Fact]
        public void TestBoard()
        {
            for (int i = 0; i < 20; i++)
            {
                Debug.Write(" " + BoardPos(i));


            }
        }
        public void Generate(int level, List<int> arr)
        {
            if (level < 20)
            {
                for (int i = 0; i < 3; i++)
                {
                    arr.Add(i+1);
                    Generate(level+1,arr);
                    
                }
            }

        }
        [Fact]
        public void TestGenerate()
        {
            var l = new List<int>();
            Generate(0,l);
        }
    }
}
