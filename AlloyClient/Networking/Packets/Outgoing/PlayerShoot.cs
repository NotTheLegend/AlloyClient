using System;
using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class PlayerShoot : OutgoingPacket<PlayerShoot> {

    public int Time;
    public byte BulletId;
    public ushort ContainerType;
    public Position StartingPos;
    public float Angle;

    public override PacketId PacketId => PacketId.PlayerShoot;

    public override void Reset() {
        Time = 0;
        BulletId = 0;
        ContainerType = 0;
        StartingPos.Reset();
        Angle = 0f;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        writer.Write(BulletId);
        writer.Write(ContainerType);
        StartingPos.Write(writer);
        writer.Write(Angle);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, Angle: {Angle}, Time: {Time}, ContainerType: {ContainerType}";
    }
}