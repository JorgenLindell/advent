using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace common
{
    public class RunningAverage
    {
        public ulong Count { get; private set; }
        public double Average { get; private set; }

        public void Add(double n)
        {
            Average = (Count * Average + n) / ++Count;
        }

    }
}
