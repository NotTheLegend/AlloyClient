using AlloyClient.Game;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.Utils;
using Alloy.Common;

namespace AlloyClient.Networking.Packets.Incoming;

public class NewTick : IncomingPacket<NewTick> {
    
    public override PacketId PacketId => PacketId.NewTick;
    
    private static ObjectStats[] _statsBuffer = new ObjectStats[256];
    public int ObjectStatsCount;
    public ObjectStats[] ObjectStats => _statsBuffer;

    public override void Reset() {
        ObjectStatsCount = 0;
    }

    public override void Read(ref SpanReader reader) {
        Structs.DataObjects.ObjectStats.StatsPoolIndex = 0;
        
        ObjectStatsCount = reader.ReadInt16();
        EnsureCapacity(ref _statsBuffer, ObjectStatsCount);
        for (int i = 0; i < ObjectStatsCount; i++)
            _statsBuffer[i].Read(ref reader);
    }

    private static void EnsureCapacity<T>(ref T[] array, int needed) {
        if (array.Length < needed)
            array = new T[needed * 2];
    }

    public override void Handle() {
        if (Map.LocalPlayer != null) {
            var move = Move.CreatePacket();
            move.NewPosition = new Position {
                X = Map.LocalPlayer.Position.X,
                Y = Map.LocalPlayer.Position.Y
            };

            Client.QueuePacket(move);
            Map.LocalPlayer.OnMove();
        }

        for (int i = 0; i < ObjectStatsCount; i++)
            ProcessObjectStats(_statsBuffer[i]);
    }

    private void ProcessObjectStats(ObjectStats stats) {
        if (!Map.Entities.TryGetValue(stats.Id, out var en)) {
            Logger.Warn($"[NewTick] Unable to lookup id: {stats.Id}");
            return;
        }
        en.UpdateStats(Structs.DataObjects.ObjectStats.StatsPool, stats.StatOffset, stats.StatCount);

        en.OnTickPosition(stats.Position.X, stats.Position.Y, 0, 0, stats.Id == Map.LocalPlayerId);
    }

    public override string ToString() {
        return $"ObjectStats: {ObjectStats}";
    }
}