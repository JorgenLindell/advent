
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using common;

var data = StreamUtils.GetLines();
//data =
//    @"
//2333133121414131402
//".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

var file = true;
var files = new DictionaryWithDefault<long, List<long>>(x => []);
var holeSpans = new SortedList<long,List<long>>();
var holes = new SortedList<long, long>();
var fileIx = 0L;
var diskPos = 0L;

var image = "";
foreach (int length in data[0].Select(c => ("" + c).ToInt() ?? 0))
{
    if (file)
    {
        image += "".PadRight(length, ("" + fileIx)[0]);
        for (int i = 0; i < length; i++)
        {
            files[fileIx].Add(diskPos++);
        }

        fileIx++;
    }
    else
    {
        image += "".PadRight(length, '.');
        var start = diskPos;
        holeSpans.Add(start,new List<long>());
        for (int i = 0; i < length; i++)
        {
            var key = diskPos;
            holeSpans[start].Add(key);
            holes.Add(key, key);
            diskPos++;
        }
    }
    file = !file;
}

// part1
//foreach (var filePair in files.Reverse())
//{
//    var fileBlocks = new Stack<long>(filePair.Value);
//    var newFileBlocks = new List<long>();
//    while (fileBlocks.Count > 0)
//    {
//        var fileBlock = fileBlocks.Pop();
//        holes.Add(fileBlock, fileBlock);
//        var block = holes.First();
//        newFileBlocks.Insert(0, block.Key);
//        holes.Remove(block.Key);
//    }
//    files[filePair.Key] = newFileBlocks;
//}

//part2
foreach (var filePair in files.Reverse())
{
    var len = filePair.Value.Count;
    var hole = holeSpans.FirstOrDefault(x => x.Value.Count >= len);
    if (hole.Value == null)
        continue;
    var holeRange = hole.Value.GetRange(0, len).ToList();
    if (holeRange[0] < files[filePair.Key][0])
    { // only towards start
        files[filePair.Key] = holeRange;
        hole.Value.RemoveRange(0, len);
        if (hole.Value.Count == 0)
        {
            holeSpans.Remove(hole.Key);
        }
    }

}


long sum =0;
foreach (var filePair in files)
{
    foreach (var pos in filePair.Value)
    {
        sum += filePair.Key * pos;
    }
}

Console.WriteLine(sum);

return;

