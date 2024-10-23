using MonoClient.Data;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.State.Input;

namespace MonoClient.Networking.Packets.Incoming;

public class MapInfo : IncomingPacket<MapInfo> {
    public int Width;
    public int Height;
    public string Name;
    public string DisplayName;
    public int Difficulty;
    public uint Seed;
    public int Background;
    public bool AllowPlayerTeleport;
    public bool ShowDisplays;
    public string Music;
    public float Darkness;

    public override PacketId PacketId => PacketId.MapInfo;

    public override void Reset() {
        Width = 0;
        Height = 0;
        Name = null;
        DisplayName = null;
        Difficulty = 0;
        Seed = 0;
        Background = 0;
        AllowPlayerTeleport = false;
        ShowDisplays = false;
        Music = null;
        Darkness = 0f;
    }

    public override void Read(NetworkReader reader) {
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
        Name = reader.ReadUtf();
        DisplayName = reader.ReadUtf();
        Difficulty = reader.ReadInt32();
        Seed = reader.ReadUInt32();
        Background = reader.ReadInt32();
        AllowPlayerTeleport = reader.ReadBoolean();
        ShowDisplays = reader.ReadBoolean();
        Music = reader.ReadUtf();
        Darkness = reader.ReadSingle();
    }

    public override void Handle() {
        Map.Reset();

        Map.InitMap(Width, Height, Name, DisplayName, Difficulty, Seed, Background,
            AllowPlayerTeleport, ShowDisplays, Music, Darkness);

        LoadOrCreate();

        Sound.Music.PlayMusic(Music);
        InputHandler.Reconnecting = false;
    }

    private static void LoadOrCreate() {
        var charList = CharacterList.Model.Characters;
        if (charList is { Length: > 0 }) {
            var load = Load.CreatePacket();
            load.CharId = Account.SelectedCharacterId;
            Client.QueuePacket(load);
        }
        else {
            var create = Create.CreatePacket();
            create.ClassType = 0x030e;
            create.SkinType = 0;
            Client.QueuePacket(create);
        }
    }

    public override string ToString() {
        return
            $"Width: {Width}, Height: {Height}, Name: {Name}, DisplayName: {DisplayName}, Difficulty: {Difficulty}, Seed: {Seed}, Background: {Background}, AllowPlayerTeleport: {AllowPlayerTeleport}, ShowDisplays: {ShowDisplays}, Music: {Music}, Darkness: {Darkness}";
    }
}