namespace RealmClient.Networking.Packets.Incoming;

public class AllyShoot : IncomingPacket<AllyShoot> {

    public byte BulletId;
    public int OwnerId;
    public ushort ContainerType;
    public float Angle;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        ContainerType = 0;
        Angle = 0;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        ContainerType = reader.ReadUInt16();
        Angle = reader.ReadSingle();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"OwnerId: {OwnerId}, BulletId: {BulletId}, ContainerType: {ContainerType}, Angle: {Angle}";
    }
}