using System;
using System.Collections.Generic;

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

    public void Read(NetworkReader reader) {
        Id = reader.ReadInt32();
        Position.Read(reader);

        var len = reader.ReadByte();
        StatOffset = StatsPoolIndex;
        StatCount = len;

        if (StatsPoolIndex + len > StatsPool.Length)
            Array.Resize(ref StatsPool, (StatsPoolIndex + len) * 2);

        for (int i = 0; i < len; i++)
            StatsPool[StatsPoolIndex++].Read(reader);
    }

    public void Write(NetworkWriter writer) {
        writer.Write(Id);
        Position.Write(writer);

        writer.Write((byte)StatCount);

        for (var i = 0; i < StatCount; i++) {
            StatsPool[StatOffset + i].Write(writer);
        }
    }

    public override string ToString() {
        return $"Id: {Id}, Position: {Position}";
    }
}