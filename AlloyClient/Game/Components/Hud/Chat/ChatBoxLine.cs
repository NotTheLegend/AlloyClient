using AlloyClient.Ui.Components.Elements;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Enums;

namespace AlloyClient.Game.Components.Hud.Chat;

public class ChatBoxLineData {
    public readonly double Time;
    public readonly string Name;
    public readonly int NumStars;
    public readonly string Recipient;
    public readonly bool ToMe;
    public readonly string Text;

    public readonly ChatBoxLine Sprite;

    public ChatBoxLineData(double time, string name, int numStars, string recipient, string text) {
        Time = time;
        Name = name;
        NumStars = numStars;
        Recipient = recipient;
        ToMe = recipient == Map.LocalPlayer.Name;
        Text = text;

        Sprite = new ChatBoxLine(this);
    }
}

public class ChatBoxLine : Container {
    
    private const string ServerChatName = "";
    private const string ClientChatName = "*Client*";
    private const string ErrorChatName = "*Error*";
    private const string HelpChatName = "*Help*";
    private const string GuildChatName = "*Guild*";
    private const char EnemyNameChar = '#';
    private const char AdminNameChar = '@';

    private const uint DefaultColor = 0xFFFFFF;
    private const uint PlayerColor = 0x00FF00;
    private const uint ServerColor = 0xFFFF00;
    private const uint ClientColor = 0x0000FF;
    private const uint ErrorColor = 0xFF0000;
    private const uint HelpColor = 0xFF5B05;
    private const uint GuildColor = 0xA6FF5D;
    private const uint EnemyColor = 0xFFA800;
    private const uint AdminColor = 0xFFFF00;
    private const uint TellColor = 0x00F0FF;

    private const int LineHeight = 18;

    public ChatBoxLine(ChatBoxLineData data) : base(new ContainerConfig { Width = ChatBox.MaxWidth, Height = LineHeight }) {
        var x = 0;

        if (TryGetStar(LineHeight, data, out var fameStar)) {
            AddChild(fameStar);

            x += fameStar.Width + 2;
        }

        if (TryGetPm(data, out var pm)) {
            pm.X = x;
            pm.Y = 2;
            AddChild(pm);
            x += pm.Width;
        }

        if (TryGetName(data, out var name)) {
            name.X = x;
            name.Y = 2;
            AddChild(name);
            x += name.Width;
        }

        GetText(data, ChatBox.MaxWidth - x, out var text);

        text.X = x;
        text.Y = 2;
        text.OffsetLineWrapBy(-(x - 2));
        AddChild(text);
    }

    private static bool TryGetStar(int size, ChatBoxLineData data, out FameStar star) {
        if (data.NumStars < 0 || data.Recipient != string.Empty && !data.ToMe) {
            star = null;
            return false;
        }

        star = new FameStar(size, data.NumStars);
        return true;
    }

    private static bool TryGetPm(ChatBoxLineData data, out SimpleText text) {
        if (data.ToMe || data.Recipient == GuildChatName || data.Recipient == string.Empty) {
            text = null;
            return false;
        }

        text = CreateText("To: ", DefaultColor);
        return true;
    }

    private static bool TryGetName(ChatBoxLineData data, out SimpleText text) {
        var color = PlayerColor;

        var name = data.Name;

        switch (name) {
            case ServerChatName:
            case ClientChatName:
            case ErrorChatName:
            case HelpChatName:
                text = null;
                return false;
        }

        if (name.StartsWith(EnemyNameChar)) {
            color = EnemyColor;
            name = name.Substring(1);
        }
        
        if (name.StartsWith(AdminNameChar)) {
            color = AdminColor;
            name = name.Substring(1);
        }

        if (data.Recipient == GuildChatName) {
            color = GuildColor;
        } else if (data.Recipient != string.Empty) {
            if (!data.ToMe) {
                name = data.Recipient;
            }
        }

        text = CreateText($"<{name}>  ", color);
        return true;
    }
    
    private static void GetText(ChatBoxLineData data, int maxWidth, out SimpleText text) {
        var color = DefaultColor;

        var name = data.Name;

        color = name switch {
            ServerChatName => ServerColor,
            ClientChatName => ClientColor,
            ErrorChatName => ErrorColor,
            HelpChatName => HelpColor,
            _ => color
        };
        
        if (name.StartsWith(AdminNameChar)) {
            color = AdminColor;
        }

        if (data.Recipient == GuildChatName) {
            color = GuildColor;
        } else if (data.Recipient != string.Empty) {
            color = TellColor;
        }

        text = CreateText(data.Text, color, maxWidth);
    }

    private static SimpleText CreateText(string text, uint color, int maxWidth = -1) {
        return new SimpleText(new TextConfig {
            Text = text,
            FontSize = LineHeight,
            FontType = FontType.Bold,
            Color = color,
            OutlineThickness = 3,
            MaxWidth = maxWidth
        });
    }
    
}