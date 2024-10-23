using System;
using System.Collections.Generic;
using MonoClient.DungeonModifier;
using static MonoClient.DungeonModifier.DungeonModifiersUtil;

namespace MonoClient.Networking.Structs.DataObjects;

public struct DungeonModifierData(DungeonModifierType id) : IDataObject {
    // TODO: Dungeon Modifier Data
    public EnemyDifficulty EnemyDifficulty;
    public BossDifficulty BossDifficulty;
    public GlobalDifficulty GlobalDifficulty;
    public List<string> ExtraInfo;
    public PercentRangeValue PercentRangeValue;

    public void Reset() {
        EnemyDifficulty = new EnemyDifficulty();
        BossDifficulty = new BossDifficulty();
        GlobalDifficulty = new GlobalDifficulty();
        ExtraInfo = null;
        PercentRangeValue = new PercentRangeValue();
    }

    public void Read(NetworkReader reader) {
        switch (id) {
            case DungeonModifierType.None:
                break;
            case DungeonModifierType.BadEggs:
                break;
            case DungeonModifierType.Swarm:
                break;
            case DungeonModifierType.DeathBunnies: // Only one that reads packet data
                var lbPool = reader.ReadSingle();
                var totalBunnies = reader.ReadInt32();
                var totalBlueBunnies = reader.ReadInt32();
                break;
            case DungeonModifierType.Giants:
                break;
            case DungeonModifierType.ItBurns:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
    }

    public void Write(NetworkWriter writer) {
        switch (id) {
            case DungeonModifierType.None:
                break;
            case DungeonModifierType.BadEggs:
                break;
            case DungeonModifierType.Swarm:
                break;
            case DungeonModifierType.DeathBunnies: // Only one that writes packet data
                writer.Write(0f);
                writer.Write(0);
                writer.Write(0);
                break;
            case DungeonModifierType.Giants:
                break;
            case DungeonModifierType.ItBurns:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
    }

    public override string ToString() {
        return
            $"EnemyDifficulty: {EnemyDifficulty}, BossDifficulty: {BossDifficulty}, GlobalDifficulty: {GlobalDifficulty}," +
            $" ExtraInfo: {ExtraInfo}, PercentRangeValue: {PercentRangeValue}";
    }
}