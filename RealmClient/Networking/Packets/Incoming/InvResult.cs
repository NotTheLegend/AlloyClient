using System;

namespace RealmClient.Networking.Packets.Incoming;

public class InvResult : IncomingPacket<InvResult> {
    public int Result;

    public override PacketId PacketId => PacketId.InvResult;

    public override void Reset() {
        Result = 0;
    }

    public override void Read(NetworkReader reader) {
        Result = reader.ReadInt32();
    }

    public override void Handle() {
        //todo
    }

    public override string ToString() {
        return $"Result: {Result}";
    }
}