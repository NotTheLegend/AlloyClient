using System.Collections.Generic;

namespace RealmClient.Networking.Structs.DataObjects;

public struct ObjectDef : IDataObject {
    public ushort ObjectType;
    public int Id;
    public Position Position;
    public List<StatData> Stats;

    public void Reset() {
        ObjectType = 0;
        Id = 0;
        Position.Reset();
        Stats = null;
    }

    public void Read(NetworkReader reader) {
        ObjectType = reader.ReadUInt16();
        Id = reader.ReadInt32();
        Position.Read(reader);

        var len = reader.ReadInt16();
        Stats = new List<StatData>(len);

        for (var i = 0; i < len; i++) {
            var statData = new StatData();
            statData.Read(reader);
            Stats.Add(statData);
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write(ObjectType);
        writer.Write(Id);
        Position.Write(writer);

        writer.Write((short)Stats.Count);

        foreach (var statData in Stats) {
            statData.Write(writer);
        }
    }

    public override string ToString() {
        return $"ObjectType: {ObjectType}, Id: {Id}, Position: {Position}, Stats: {Stats}";
    }
}