using System;
using System.IO;
using System.Net;
using System.Text;

namespace MonoClient.Networking;

public class NetworkReader(Stream s) : BinaryReader(s, Encoding.UTF8) {
    public override short ReadInt16()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt16());
    }
    public override int ReadInt32()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt32());
    }
    public override long ReadInt64()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt64());
    }
    public override ushort ReadUInt16()
    {
        return (ushort)IPAddress.NetworkToHostOrder((short)base.ReadUInt16());
    }
    public override uint ReadUInt32()
    {
        return (uint)IPAddress.NetworkToHostOrder((int)base.ReadUInt32());
    }
    public override ulong ReadUInt64()
    {
        return (ulong)IPAddress.NetworkToHostOrder((long)base.ReadUInt64());
    }
    public override float ReadSingle()
    {
        var arr = base.ReadBytes(4);
        Array.Reverse(arr);
        return BitConverter.ToSingle(arr, 0);
    }
    public override double ReadDouble()
    {
        var arr = base.ReadBytes(8);
        Array.Reverse(arr);
        return BitConverter.ToDouble(arr, 0);
    }

    public string ReadNullTerminatedString()
    {
        StringBuilder ret = new StringBuilder();
        byte b = ReadByte();
        while (b != 0)
        {
            ret.Append((char)b);
            b = ReadByte();
        }
        return ret.ToString();
    }

    public string ReadUtf()
    {
        return Encoding.UTF8.GetString(ReadBytes(ReadInt16()));
    }

    public string Read32Utf()
    {
        return Encoding.UTF8.GetString(ReadBytes(ReadInt32()));
    }
}

public class NetworkWriter(Stream s) : BinaryWriter(s, Encoding.UTF8) {
    public override void Write(short value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }
    public override void Write(int value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }
    public override void Write(long value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }
    public override void Write(ushort value)
    {
        base.Write((ushort)IPAddress.HostToNetworkOrder((short)value));
    }
    public override void Write(uint value)
    {
        base.Write((uint)IPAddress.HostToNetworkOrder((int)value));
    }
    public override void Write(ulong value)
    {
        base.Write((ulong)IPAddress.HostToNetworkOrder((long)value));
    }
    public override void Write(float value)
    {
        var b = BitConverter.GetBytes(value);
        Array.Reverse(b);
        base.Write(b);
    }
    public override void Write(double value)
    {
        var b = BitConverter.GetBytes(value);
        Array.Reverse(b);
        base.Write(b);
    }

    public void WriteNullTerminatedString(string str)
    {
        Write(Encoding.UTF8.GetBytes(str));
        Write((byte)0);
    }

    public void WriteUtf(string str)
    {
        if (str == null)
            Write((short)0);
        else
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            Write((short)bytes.Length);
            Write(bytes);
        }
    }

    public void Write32Utf(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Write(bytes.Length);
        Write(bytes);
    }
}