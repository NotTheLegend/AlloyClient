namespace AlloyClient.Networking.Packets.Incoming;

public class Failure : IncomingPacket<Failure> {
    public int ErrorId;
    public string ErrorDescription;

    public override PacketId PacketId => PacketId.Failure;

    public override void Reset() {
        ErrorId = 0;
        ErrorDescription = null;
    }

    public override void Read(ref SpanReader reader) {
        ErrorId = reader.ReadInt32();
        ErrorDescription = reader.ReadUTF();
    }

    public override void Handle() {
        Client.Log.Error($"Error: {ErrorId} - {ErrorDescription}");

        Client.Disconnect(ErrorDescription);
    }

    public override string ToString() {
        return $"ErrorId: {ErrorId}, ErrorDescription: {ErrorDescription}";
    }
}