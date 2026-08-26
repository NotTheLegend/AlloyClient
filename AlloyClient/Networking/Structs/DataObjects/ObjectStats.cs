using System;

namespace AlloyClient.Networking.Structs.DataObjects;

public struct ObjectStats : IDataObject {
    public int Id;
    public Position Position;
    public int StatOffset;
    public int StatCount;
    
    public static StatData[] StatsPool = new StatData[4096];
    public static int StatsPoolIndex = 0; // resets each packet

    public void Reset() {
        Id = 0;
        Position.Reset();
        StatOffset = 0;
        StatCount = 0;
    }

    public void Read(ref SpanReader reader) {
        Id = reader.ReadInt32();
        Position.Read(ref reader);

        var len = reader.ReadByte();
        StatOffset = StatsPoolIndex;
        StatCount = len;

        if (StatsPoolIndex + len > StatsPool.Length)
            Array.Resize(ref StatsPool, (StatsPoolIndex + len) * 2);

        for (int i = 0; i < len; i++)
            StatsPool[StatsPoolIndex++].Read(ref reader);
    }

    public void Write(ref SpanWriter writer) {
        writer.Write(Id);
        Position.Write(ref writer);

        writer.Write((byte)StatCount);

        for (var i = 0; i < StatCount; i++) {
            StatsPool[StatOffset + i].Write(ref writer);
        }
    }

    public override string ToString() {
        return $"Id: {Id}, Position: {Position}";
    }
}