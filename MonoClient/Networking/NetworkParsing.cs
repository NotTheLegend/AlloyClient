using System.IO;
using System.Text;

namespace MonoClient.Networking;

public class NetworkReader(Stream s) : BinaryReader(s, Encoding.UTF8) {
    public string ReadUtf() {
        return Encoding.UTF8.GetString(ReadBytes(ReadInt16()));
    }

    public string ReadUtf32() {
        return Encoding.UTF8.GetString(ReadBytes(ReadInt32()));
    }
}

public class NetworkWriter(Stream s) : BinaryWriter(s, Encoding.UTF8) {
    public void WriteNullTerminatedString(string str) {
        Write(Encoding.UTF8.GetBytes(str));
        Write((byte)0);
    }

    public void WriteUtf(string str) {
        if (str == null) {
            Write((short)0);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(str);
        Write((short)bytes.Length);
        Write(bytes);
    }

    public void WriteUtf32(string str) {
        var bytes = Encoding.UTF8.GetBytes(str);
        Write(bytes.Length);
        Write(bytes);
    }
}