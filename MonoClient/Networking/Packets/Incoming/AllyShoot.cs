namespace MonoClient.Networking.Packets.Incoming;

public class AllyShoot : IncomingPacket<AllyShoot> {
    public int OwnerId;
    public bool IsAbility;
    public int BulletType;
    public float[] Angles;

    public override PacketId PacketId => PacketId.AllyShoot;

    public override void Reset() {
        OwnerId = 0;
        IsAbility = false;
        BulletType = 0;
        Angles = null;
    }

    public override void Read(NetworkReader reader) {
        OwnerId = reader.ReadInt32();
        IsAbility = reader.ReadBoolean();
        BulletType = reader.ReadInt32();

        Angles = new float[reader.ReadInt16()];

        for (var i = 0; i < Angles.Length; i++) {
            Angles[i] = reader.ReadSingle();
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"OwnerId: {OwnerId}, IsAbility: {IsAbility}, BulletType: {BulletType}, Angles: {string.Join(", ", Angles)}";
    }
}