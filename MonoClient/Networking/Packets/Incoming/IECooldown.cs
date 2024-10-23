namespace MonoClient.Networking.Packets.Incoming;

public class IECooldown : IncomingPacket<IECooldown> {
    public int Cooldown;
    public uint Color;
    public bool Backwards;

    public override PacketId PacketId => PacketId.IECooldown;

    public override void Reset() {
        Cooldown = 0;
        Color = 0;
        Backwards = false;
    }

    public override void Read(NetworkReader reader) {
        Cooldown = reader.ReadInt32();
        Color = reader.ReadUInt32();
        Backwards = reader.ReadBoolean();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Cooldown: {Cooldown}, Color: {Color}, Backwards: {Backwards}";
    }
}