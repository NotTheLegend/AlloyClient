namespace MonoClient.Networking.Packets.Incoming;

public class WeaponMasterShoot : IncomingPacket<WeaponMasterShoot> {
    public int OwnerId;
    public int PlayerId;
    public int ContainerType;
    public float[] Angles;
    public int[] Damages;
    public string ProjDesc = "{}";

    public override PacketId PacketId => PacketId.WeaponMasterShoot;

    public override void Reset() {
        OwnerId = 0;
        PlayerId = 0;
        ContainerType = 0;
        Angles = null;
        Damages = null;
        ProjDesc = null;
    }

    public override void Read(NetworkReader reader) {
        OwnerId = reader.ReadInt32();
        PlayerId = reader.ReadInt32();
        ContainerType = reader.ReadInt32();

        Angles = new float[reader.ReadInt16()];

        for (var i = 0; i < Angles.Length; i++) {
            Angles[i] = reader.ReadSingle();
        }

        Damages = new int[reader.ReadInt16()];

        for (var i = 0; i < Damages.Length; i++) {
            Damages[i] = reader.ReadInt32();
        }

        ProjDesc = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"OwnerId: {OwnerId}, PlayerId: {PlayerId}, ContainerType: {ContainerType}, Angles: {Angles}, Damages: {Damages}, ProjDesc: {ProjDesc}";
    }
}