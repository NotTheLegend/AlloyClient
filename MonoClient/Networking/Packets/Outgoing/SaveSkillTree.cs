using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class SaveSkillTree : OutgoingPacket<SaveSkillTree> {
    public int[] SkillTree;

    public override PacketId PacketId => PacketId.SaveSkillTree;

    public override void Reset() {
        SkillTree = Array.Empty<int>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)SkillTree.Length);

        foreach (var skillId in SkillTree) {
            writer.Write(skillId);
        }
    }

    public override string ToString() {
        return $"SkillTree: {string.Join(", ", SkillTree)}";
    }
}