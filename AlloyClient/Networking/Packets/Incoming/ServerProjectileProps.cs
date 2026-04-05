using System;
using System.Threading;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game;
using AlloyClient.Game.Objects.Util;

namespace AlloyClient.Networking.Packets.Incoming;

public class ServerProjectileProps : IncomingPacket<ServerProjectileProps> {
    
    private static int TotalBytes = 0; 
    private static int TotalCount = 0; 
    
    public ushort ContainerType;
    public byte ProjId;
    public string ObjectId;
    public float Lifetime;
    public bool MultiHit;
    public bool PassesCover;
    public bool ArmorPiercing;
    public int Size;
    public (ConditionEffectIndex, int)[] Effects;

    public override PacketId PacketId => PacketId.ServerProjectileProps;

    public override void Reset() {
        ContainerType = 0;
        ProjId = 0;
        ObjectId = null;
        Lifetime = 0;
        MultiHit = false;
        PassesCover = false;
        ArmorPiercing = false;
        Size = 0;
        Effects = null;
    }

    public override void Read(ref SpanReader reader) {
        var start = reader.Position;
        ContainerType = reader.ReadUInt16();
        ProjId = reader.ReadByte();
        ObjectId = reader.ReadUTF();
        Lifetime = reader.ReadSingle();
        MultiHit = reader.ReadBoolean();
        PassesCover = reader.ReadBoolean();
        ArmorPiercing = reader.ReadBoolean();
        Size = reader.ReadInt32();
        Effects = new (ConditionEffectIndex, int)[reader.ReadUInt16()];
        for (var i = 0; i < Effects.Length; i++)
            Effects[i] = ((ConditionEffectIndex)reader.ReadUInt16(), reader.ReadInt32());

        var bytes = reader.Position - start;
        Interlocked.Add(ref TotalBytes, bytes);
        Interlocked.Increment(ref TotalCount);
        Console.WriteLine($"{TotalCount} ServerProjectileProps: {TotalBytes} bytes");
    }

    public override void Handle() {
        var objDesc = ObjectLibrary.TypeToObjectProps[ContainerType];
        objDesc.Projectiles[ProjId] = ProjectileProperties.FromServer(this);
    }

    public override string ToString() {
        return $"ContainerType: {ContainerType}; ProjId: {ProjId}; ObjectId: {ObjectId}; Lifetime: {Lifetime};";
    }
}