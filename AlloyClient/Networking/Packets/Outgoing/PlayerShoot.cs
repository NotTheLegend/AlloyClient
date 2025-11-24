using System;
using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class PlayerShoot : OutgoingPacket<PlayerShoot> {

    public float Angle;
    public Position StartingPos;
    public int Time;
    public bool IsServerShoot;
    public float AngleInc;
    public int[] DamageList;
    public float[] CritList;
    public ushort ItemType;

    public override PacketId PacketId => PacketId.PlayerShoot;

    public override void Reset() {
        Time = 0;
        ItemType = 0;
        StartingPos.Reset();
        Angle = 0f;
        DamageList = null;
        CritList = null;
        AngleInc = 0f;
        IsServerShoot = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Angle);
        StartingPos.Write(writer);
        writer.Write(Time);
        writer.Write(IsServerShoot);
        if (IsServerShoot) {
            writer.Write(AngleInc);
            writer.Write((byte)DamageList.Length);
            for (var i = 0; i < DamageList.Length; i++)
                writer.Write(DamageList[i]);
            writer.Write((byte)CritList.Length);
            for (var i = 0; i < CritList.Length; i++)
                writer.Write(CritList[i]);
            writer.Write(ItemType);
        }
    }

    public override string ToString() {
        return $"Angle: {Angle}, Time: {Time}, ContainerType: {ItemType}";
    }
}