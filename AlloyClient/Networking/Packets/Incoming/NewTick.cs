using AlloyClient.Game;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.Utils;
using Common;

namespace AlloyClient.Networking.Packets.Incoming;

public class NewTick : IncomingPacket<NewTick> {
    public int TickId;
    public int TickTime;
    public ObjectStats[] ObjectStats;

    public override PacketId PacketId => PacketId.NewTick;

    public override void Reset() {
        TickId = 0;
        TickTime = 0;
        ObjectStats = null;
    }

    public override void Read(NetworkReader reader) {
        //TickId = reader.ReadInt32();
        //TickTime = reader.ReadInt32();

        ObjectStats = new ObjectStats[reader.ReadInt16()];

        for (var i = 0; i < ObjectStats.Length; i++) {
            ObjectStats[i] = new ObjectStats();
            ObjectStats[i].Read(reader);
        }
    }

    public override void Handle() {
        var isLocalPlayer = false;

        if (Map.LocalPlayer != null) {
            var move = Move.CreatePacket();
            move.NewPosition = new Position {
                X = Map.LocalPlayer.Position.X,
                Y = Map.LocalPlayer.Position.Y
            };

            Client.QueuePacket(move);

            isLocalPlayer = true;

            Map.LocalPlayer.OnMove();
        }

        foreach (var stats in ObjectStats) {
            ProcessObjectStats(stats, isLocalPlayer);
        }

        Map.LastTickId = TickId;
    }

    private void ProcessObjectStats(ObjectStats stats, bool isPlayer) {
        if (!Map.Entities.TryGetValue(stats.Id, out var en)) {
            Logger.Warn($"[NewTick] Unable to lookup id: {stats.Id}");
            return;
        }
        en.UpdateStats(stats.Stats);

        en.OnTickPosition(stats.Position.X, stats.Position.Y, TickTime, TickId, isPlayer);
    }

    public override string ToString() {
        return $"TickId: {TickId}, TickTime: {TickTime}, ObjectStats: {ObjectStats}";
    }
}