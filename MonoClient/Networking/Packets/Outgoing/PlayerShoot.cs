using System;
using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class PlayerShoot : OutgoingPacket<PlayerShoot> {
    public byte[] BulletIds;
    public float[] Angles;
    public byte ExplodeLevel;
    public byte ExplodeTypeValue;
    public int Time;
    public bool Ability;
    public ushort ContainerType;
    public Position MousePosition;

    public override PacketId PacketId => PacketId.PlayerShoot;

    public override void Reset() {
        BulletIds = Array.Empty<byte>();
        Angles = Array.Empty<float>();
        ExplodeLevel = 0;
        ExplodeTypeValue = 0;
        Time = 0;
        Ability = false;
        ContainerType = 0;
        MousePosition.Reset();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)BulletIds.Length);
        foreach (var bulletId in BulletIds)
            writer.Write(bulletId);

        writer.Write((short)Angles.Length);
        foreach (var angle in Angles)
            writer.Write(angle);

        writer.Write(ExplodeLevel);
        writer.Write(ExplodeTypeValue);
        writer.Write(Time);
        writer.Write(Ability);
        writer.Write(ContainerType);
        MousePosition.Write(writer);
    }

    public override string ToString() {
        return
            $"BulletIds: {BulletIds}, Angles: {Angles}, ExplodeLevel: {ExplodeLevel}, ExplodeTypeValue: {ExplodeTypeValue}, Time: {Time}, Ability: {Ability}, ContainerType: {ContainerType}, MousePosition: {MousePosition}";
    }
}