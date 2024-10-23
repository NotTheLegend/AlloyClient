namespace MonoClient.Networking.Packets.Outgoing;

public class OptionsChanged : OutgoingPacket<OptionsChanged> {
    public byte AllyShots;
    public byte AllyDamage;
    public byte AllyNotifs;
    public byte AllyParticles;
    public bool IgnoreDye;

    public override PacketId PacketId => PacketId.OptionsChanged;

    public override void Reset() {
        AllyShots = 0;
        AllyDamage = 0;
        AllyNotifs = 0;
        AllyParticles = 0;
        IgnoreDye = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(AllyShots);
        writer.Write(AllyDamage);
        writer.Write(AllyNotifs);
        writer.Write(AllyParticles);
        writer.Write(IgnoreDye);
    }

    public override string ToString() {
        return
            $"AllyShots: {AllyShots}, AllyDamage: {AllyDamage}, AllyNotifs: {AllyNotifs}, AllyParticles: {AllyParticles}, IgnoreDye: {IgnoreDye}";
    }
}