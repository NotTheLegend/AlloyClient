using System;
using System.IO;
using System.Net;
using System.Numerics;
using System.Text;

namespace RealmClient.Networking;

public class NetworkReader : BinaryReader
    {
        private readonly bool _littleEndian;

        public NetworkReader(Stream s, bool littleEndian = true) : base(s, Encoding.UTF8) // Little endian is for network, big endian is for maps
        {
            _littleEndian = littleEndian;
        }

        public override short ReadInt16()
        {
            if (!_littleEndian)
                return IPAddress.NetworkToHostOrder(base.ReadInt16());
            else
                return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (!_littleEndian)
                return IPAddress.NetworkToHostOrder(base.ReadInt32());
            else
                return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            if (!_littleEndian)
                return IPAddress.NetworkToHostOrder(base.ReadInt64());
            else
                return base.ReadInt64();
        }

        public override ushort ReadUInt16()
        {
            if (!_littleEndian)
                return (ushort)IPAddress.NetworkToHostOrder((short)base.ReadUInt16());
            else
                return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            if (!_littleEndian)
                return (uint)IPAddress.NetworkToHostOrder((int)base.ReadUInt32());
            else
                return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            if (!_littleEndian)
                return (ulong)IPAddress.NetworkToHostOrder((long)base.ReadUInt64());
            else
                return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            if (!_littleEndian)
            {
                var arr = base.ReadBytes(4);
                Array.Reverse(arr);
                return BitConverter.ToSingle(arr, 0);
            }
            else
                return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            if (!_littleEndian)
            {
                var arr = base.ReadBytes(8);
                Array.Reverse(arr);
                return BitConverter.ToDouble(arr, 0);
            }
            else
                return base.ReadDouble();
        }

        public string ReadNullTerminatedString()
        {
            var ret = new StringBuilder();
            var b = ReadByte();
            while (b != 0)
            {
                ret.Append((char)b);
                b = ReadByte();
            }

            return ret.ToString();
        }

        public string ReadUTF()
        {
            return Encoding.UTF8.GetString(ReadBytes(ReadInt16()));
        }

        public string Read32UTF()
        {
            return Encoding.UTF8.GetString(ReadBytes(ReadInt32()));
        }
    }

public class NetworkWriter : BinaryWriter
{
    private readonly bool _littleEndian;

    public NetworkWriter(Stream s, bool littleEndian = true) : base(s, Encoding.UTF8) // Little endian is for network, big endian is for maps
    {
        _littleEndian = littleEndian;
    }

    public override void Write(short value)
    {
        if (!_littleEndian)
            base.Write(IPAddress.HostToNetworkOrder(value));
        else
            base.Write(value);
    }

    public override void Write(int value)
    {
        if (!_littleEndian)
            base.Write(IPAddress.HostToNetworkOrder(value));
        else
            base.Write(value);
    }

    /// <summary>
    /// Writes array length as ushort and array to stream.
    /// Use base method to write without length.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public void Write<T>(T[] value)
    where T : struct, INumber<T>
    {
        Write((ushort)value.Length);

        if (value is byte[] bytes)
        {
            base.Write(bytes);
            return;
        }

        for (var i = 0; i < value.Length; i++)
        {
            switch (value[i])
            {
                case long l: Write(l); break;
                case float f: Write(f); break;
                case double d: Write(d); break;
                case decimal dec: Write(dec); break;
                case short s: Write(s); break;
                case ushort us: Write(us); break;
                case uint ui: Write(ui); break;
                case ulong ul: Write(ul); break;
                case int integer: Write(integer); break;
            }
        }
    }
    public void Write(string[] value)
    {
        Write((ushort)value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            Write(value[i]);
        }
    }
    public override void Write(long value)
    {
        if (!_littleEndian)
            base.Write(IPAddress.HostToNetworkOrder(value));
        else
            base.Write(value);
    }

    public override void Write(ushort value)
    {
        if (!_littleEndian)
            base.Write((ushort)IPAddress.HostToNetworkOrder((short)value));
        else
            base.Write(value);
    }

    public override void Write(uint value)
    {
        if (!_littleEndian)
            base.Write((uint)IPAddress.HostToNetworkOrder((int)value));
        else
            base.Write(value);
    }

    public override void Write(ulong value)
    {
        if (!_littleEndian)
            base.Write((ulong)IPAddress.HostToNetworkOrder((long)value));
        else
            base.Write(value);
    }

    public override unsafe void Write(float value)
    {
        if (!_littleEndian)
            for (var i = 3; i >= 0; i--)
                base.Write(((byte*)&value)[i]);
        else
            base.Write(value);
    }

    public override unsafe void Write(double value)
    {
        if (!_littleEndian)
            for (var i = 7; i >= 0; i--)
                base.Write(((byte*)&value)[i]);
        else
            base.Write(value);
    }

    public void WriteNullTerminatedString(string str)
    {
        base.Write(Encoding.UTF8.GetBytes(str));
        Write((byte)0);
    }

    public void WriteUTF(string str)
    {
        if (str == null)
            Write((ushort)0);
        else
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            Write(bytes);
        }
    }
    public override void Write(string str)
    {
        WriteUTF(str);
    }
    /*public void Write(WorldPosData wp)
    {
        Write(wp.X);
        Write(wp.Y);
    }*/
    public void Write32UTF(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Write(bytes.Length);
        base.Write(bytes);
    }
}