using System;
using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class PlayerShoot : OutgoingPacket<PlayerShoot> {

    public float Angle;

    public override PacketId PacketId => PacketId.PlayerShoot;

    public override void Reset() {
        Angle = 0f;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(Angle);
    }

    public override string ToString() {
        return $"Angle: {Angle}";
    }
}