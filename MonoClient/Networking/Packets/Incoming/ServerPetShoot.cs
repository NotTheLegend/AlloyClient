using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class ServerPetShoot : IncomingPacket<ServerPetShoot> {
    public byte BulletId;
    public int OwnerId;
    public int PetId;
    public int ContainerType;
    public Position StartingPos;
    public float Angle;
    public short Damage;
    public byte BulletType;

    public override PacketId PacketId => PacketId.ServerPetShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        PetId = 0;
        ContainerType = 0;
        StartingPos.Reset();
        Angle = 0;
        Damage = 0;
        BulletType = 0;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        PetId = reader.ReadInt32();
        ContainerType = reader.ReadInt32();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        BulletType = reader.ReadByte();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"BulletId: {BulletId}, OwnerId: {OwnerId}, PetId: {PetId}, ContainerType: {ContainerType}, StartingPos: {StartingPos}, Angle: {Angle}, Damage: {Damage}, BulletType: {BulletType}";
    }
}