using MonoClient.Networking.Structs.DataObjects;
using Newtonsoft.Json;

namespace MonoClient.Networking.Packets.Incoming;

public class PotStorageGetResult : IncomingPacket<PotStorageGetResult> {
    public PotStorageDetails Details;

    public override PacketId PacketId => PacketId.PotStorageGetResult;

    public override void Reset() {
        Details.Reset();
    }

    public override void Read(NetworkReader reader) {
        var detailsJson = reader.ReadUtf();
        Details = JsonConvert.DeserializeObject<PotStorageDetails>(detailsJson);
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Details: {Details}";
    }
}