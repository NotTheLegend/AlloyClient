using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class ServerPlayerShoot : IncomingPacket<ServerPlayerShoot> {
    public byte BulletId;
    public int OwnerId;
    public int ContainerType;
    public Position StartingPos;
    public float Angle;
    public short Damage;
    public int SlotId;
    public string ProjDesc;

    public override PacketId PacketId => PacketId.ServerPlayerShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        ContainerType = 0;
        StartingPos.Reset();
        Angle = 0;
        Damage = 0;
        SlotId = 0;
        ProjDesc = null;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        ContainerType = reader.ReadInt32();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        SlotId = reader.ReadInt32();
        ProjDesc = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"BulletId: {BulletId}, OwnerId: {OwnerId}, ContainerType: {ContainerType}, StartingPos: {StartingPos}, Angle: {Angle}, Damage: {Damage}, SlotId: {SlotId}, ProjDesc: {ProjDesc}";
    }
}