using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Incoming;

public class ServerPlayerShoot : IncomingPacket<ServerPlayerShoot> {
    public byte BulletId;
    public int OwnerId;
    public int ContainerType;
    public Position StartingPos;
    public float Angle;
    public short Damage;

    public override PacketId PacketId => PacketId.ServerPlayerShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        ContainerType = 0;
        StartingPos.Reset();
        Angle = 0;
        Damage = 0;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        ContainerType = reader.ReadInt32();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, OwnerId: {OwnerId}, ContainerType: {ContainerType}, StartingPos: {StartingPos}, Angle: {Angle}, Damage: {Damage}";
    }
}