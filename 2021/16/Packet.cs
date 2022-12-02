using System.Diagnostics;
using common;

namespace _16;

public class Packet
{
    protected static int IdCounter = 1;

    public int Id { get; protected set; }
    public long Version { get; protected set; }
    public long PacketType { get; protected set; }
    public int Start { get; private set; }
    public int Length { get; private set; }

    public virtual long NumValue => 0;

    public static string SourceString { get; set; }

    protected void SetAdresses(int startpos, int totalRead)
    {
        Start = startpos;
        Length = totalRead - startpos;
    }

    public virtual void MeAndSubPackets(Action<Packet> action)
    {
        action(this);
    }

    protected string GetData()
    {
        return SourceString.Substring(Start, Length);
    }

    public static Packet PacketFactory(string binaryString)
    {
        var totalRead = 0;
        SourceString = binaryString;

        var stream = new StringReader(binaryString);
        return ReadPacket(stream, ref totalRead);
    }

    protected static string ReadBits(StringReader stream, int count, ref int totalRead)
    {
        var res = "";
        for (var cnt = 0; cnt < count && stream.Peek() != -1; cnt++)
        {
            res += (char)stream.Read();
            totalRead++;
        }

        return res;
    }



    protected static Packet ReadPacket(StringReader stream, ref int totalRead)
    {
        var startpos = totalRead;
        var version = ReadBits(stream, 3, ref totalRead).BinToInt64();
        var type = ReadBits(stream, 3, ref totalRead).BinToInt64();
        if (type == 4)
        {
            var literalPacket = new LiteralPacket(version, type, stream, ref totalRead);
            literalPacket.SetAdresses(startpos, totalRead);
            return literalPacket;
        }

        var operatorPacket = new OperatorPacket(version, type, stream, ref totalRead);
        operatorPacket.SetAdresses(startpos, totalRead);

        return operatorPacket;
    }
}

public class LiteralPacket : Packet
{
    public LiteralPacket(long version, long type, StringReader stream, ref int totalRead)
    {
        Id = IdCounter++;
        Version = version;
        PacketType = type;
        var readBits = "";
        string cont;
        do
        {
            cont = ReadBits(stream, 1, ref totalRead);
            var nybble = ReadBits(stream, 4, ref totalRead);
            readBits += nybble;
        } while (cont == "1");

        Value = readBits;
    }

    public string Value { get; set; }
    public override long NumValue => Value.BinToInt64();

    public override string ToString()
    {
        return $"[{Id}] Literal ({PacketType}) (v{Version}) = {Value.BinToInt64()}     Pos:{Start} Len:{Length}  {GetData()}\n";
    }
}

public class OperatorPacket : Packet
{
    public OperatorPacket(long version, long type, StringReader stream, ref int totalRead)
    {
        Id = IdCounter++;
        Version = version;
        PacketType = type;
        var outerWasAt = totalRead - 6;
        var lengthType = ReadBits(stream, 1, ref totalRead);
        if (lengthType == "1")
        {
            var numberOfSubPackets = ReadBits(stream, 11, ref totalRead).BinToInt64();
            while (numberOfSubPackets-- > 0)
            {
                var subp = ReadPacket(stream, ref totalRead);
                SubPackets.Add(subp);
            }
        }
        else
        {
            var totalLengthSub = (int)ReadBits(stream, 15, ref totalRead).BinToInt64();
            var wasAt = totalRead;
            while (totalRead - wasAt < totalLengthSub)
            {
                var subp = ReadPacket(stream, ref totalRead);
                SubPackets.Add(subp);
            }

            if (totalRead - wasAt != totalLengthSub)
                Debug.WriteLine("WrongLength");
        }

        var readBits = totalRead - outerWasAt;
        var bitsOfNybbleRead = readBits % 4;
        //   if (bitsOfNybbleRead!= 0)
        //       ReadBits(stream, 4 - bitsOfNybbleRead, ref totalRead);
    }

    public List<Packet> SubPackets { get; set; } = new();

    public override long NumValue => Calculate();

    private long Calculate()
    {
        var val = SubValues().ToList();
        switch (PacketType)
        {
            case 0: //sum
                return val.Sum(x => x);
            case 1: //prod
                return val.Aggregate(1l, (p, x) => p * x);
            case 2: //min
                return val.Min(x => x);
            case 3: //max
                return val.Max(x => x);
            case 4: //Literal
                throw new Exception("Very strange");
            case 5: //gt
                return val[0] > val[1] ? 1 : 0;
            case 6: //lt
                return val[0] < val[1] ? 1 : 0;
            case 7: //equal
                return val[0] == val[1] ? 1 : 0;
            default:
                throw new Exception("Unknown operator type=" + PacketType);
        }
    }
    public List<long> SubValues()
    {
        return SubPackets.Select(p => p.NumValue).ToList();
    }

    public override void MeAndSubPackets(Action<Packet> action)
    {
        action(this);
        SubPackets.ForEach(x => x.MeAndSubPackets(action));
    }

    public override string ToString()
    {
        var res = "";
        var indent = "".PadLeft(2);
        SubPackets.ForEach(sp =>
        {
            var s = sp.ToString();
            res += s;
        });
        res=res.Split('\n').Select(s => indent + s).StringJoin("\n");
        var my = $"[{Id}] Operator ({PacketType}) (v {Version})     Pos:{Start} Len:{Length} {GetData()}\n";
        return my + res;
    }
}