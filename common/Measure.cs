using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace common
{
    public class Measure : IDisposable
    {
        readonly TextWriter _outWriter = Console.Out;
        readonly Stopwatch _sw = new();
        public Measure(TextWriter output=null!)
        {
            if (output == null)
                _outWriter =
                    new DebuggerTextWriter();

            _sw.Start();
        }

        public void Dispose()
        {
            _sw.Stop();
            _outWriter.WriteLine("Measure Timer:" + _sw.ElapsedMilliseconds);
            _outWriter.Dispose();
        }
    }
}