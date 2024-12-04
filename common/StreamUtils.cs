using System;
using System.IO;

namespace common
{
    public static class StreamUtils
    {
        public static string[] GetLines(string file= "../../../input.txt")
        {
            return StreamUtils.GetInputStream(file).ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static TextReader GetInputStream(string file = "", string testData = "")
        {
            TextReader reader;
            if (file == "")
            {
                reader = new StringReader(testData);
            }
            else
            {
                Console.WriteLine("Loading from file " + file);
                reader = File.OpenText(file);
            }

            return reader;
        }
    }
}