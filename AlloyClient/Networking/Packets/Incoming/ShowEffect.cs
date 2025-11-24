using AlloyClient.Networking.Enums;
using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Incoming;

public class ShowEffect : IncomingPacket<ShowEffect> {
    public EffectType EffectType;
    public int TargetObjectId;
    public Position Pos1;
    public Position Pos2;
    public ARGB Color;

    public override PacketId PacketId => PacketId.ShowEffect;

    public override void Reset() {
        EffectType = default;
        TargetObjectId = 0;
        Pos1.Reset();
        Pos2.Reset();
        Color.Reset();
    }

    public override void Read(NetworkReader reader) {
        EffectType = (EffectType)reader.ReadByte();

        switch (EffectType) {
            case EffectType.Heal:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                break;
            case EffectType.Teleport:
                Pos1.Read(reader);
                break;
            case EffectType.Stream:
                Pos1.Read(reader);
                Pos2.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Throw:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Nova:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                Pos1.Read(reader);
                break;
            case EffectType.Poison:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                break;
            case EffectType.Trail:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Burst:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Pos2.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Flow:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Trap:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                break;
            case EffectType.Lightning:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Collapse:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Pos2.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.ConeBlast:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Color.Read(reader);
                break;
            case EffectType.Earthquake:
                Pos1.Read(reader);
                break;
            case EffectType.Flashing:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                break;
            case EffectType.ObjectToss:
                TargetObjectId = reader.ReadInt32();
                Pos1.Read(reader);
                Pos2.Read(reader);
                break;
            case EffectType.Vortex:
                TargetObjectId = reader.ReadInt32();
                Color.Read(reader);
                break;
            case EffectType.FadeToBlack:
                break;
            default:
                return;
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"EffectType: {EffectType}, TargetObjectId: {TargetObjectId}, Pos1: {Pos1}, Pos2: {Pos2}, Color: {Color}";
    }
}