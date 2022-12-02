using System;
using System.Diagnostics;

namespace common
{
    public class Measure : IDisposable
    {
        Stopwatch sw = new Stopwatch();
        public Measure()
        {
            sw.Start();
        }

        public void Dispose()
        {
            sw.Stop();
            Console.WriteLine("Timer:" + sw.ElapsedMilliseconds);
        }
    }
}