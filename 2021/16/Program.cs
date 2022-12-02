using common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace _16
{
    class Program
    {
        private static string[] _testData = new[]{
            "A0016C880162017C3686B18A3D4780",
            "D2FE28",
            "38006F45291200",
            "EE00D40C823060",
            "8A004A801A8002F478",
            "620080001611562C8802118E34",
            "C0015000016115A2E0802F182340",
             };

        private static string template;
        static void Main()
        {

            //   foreach (var data in _testData)
            {

                var stream = StreamUtils.GetInputStream(file: "input.txt");
                //var stream = StreamUtils.GetInputStream(testData: _testData[0]);
                var testData = stream.ReadLine().ToBin();
                var p = Packet.PacketFactory(testData);
                Console.WriteLine(p.ToString());
                int summa = 0;
                p.MeAndSubPackets(pa =>
                {
                    summa = summa + (int)pa.Version;
                });
                Console.WriteLine($"TotVersion= {summa}");
                Console.WriteLine($"CalculationResult= "+p.NumValue);
            }
        }


        private static IEnumerable<string> LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            while (stream.Peek() != -1 || !inputLine.IsNullOrEmpty())
            {
                if (inputLine is { } && inputLine != "")
                {
                    yield return inputLine;
                }
                inputLine = stream.ReadLine();
            }
        }
    }
}