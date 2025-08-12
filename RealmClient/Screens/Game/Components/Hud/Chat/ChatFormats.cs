using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Enums;

namespace RealmClient.Screens.Game.Components.Hud.Chat;

public static class ChatFormats {
    public static readonly TextConfig NormalFormat;
    public static readonly TextConfig ServerFormat;
    public static readonly TextConfig ClientFormat;
    public static readonly TextConfig HelpFormat;
    public static readonly TextConfig ErrorFormat;
    public static readonly TextConfig EnemyFormat;
    public static readonly TextConfig AdminFormat;
    public static readonly TextConfig PlayerFormat;
    public static readonly TextConfig SepFormat;
    public static readonly TextConfig TellFormat;
    public static readonly TextConfig GuildFormat;

    static ChatFormats() {
        NormalFormat = DefaultFormat().WithColor(16777215);
        ServerFormat = DefaultFormat().WithColor(16776960);
        ClientFormat = DefaultFormat().WithColor(255);
        HelpFormat   = DefaultFormat().WithColor(16734981);
        ErrorFormat  = DefaultFormat().WithColor(16711680);
        EnemyFormat  = DefaultFormat().WithColor(16754688);
        AdminFormat  = DefaultFormat().WithColor(16776960);
        PlayerFormat = DefaultFormat().WithColor(65280);
        SepFormat    = DefaultFormat().WithColor(3552822);
        TellFormat   = DefaultFormat().WithColor(61695);
        GuildFormat  = DefaultFormat().WithColor(10944349);
    }

    private static TextConfig DefaultFormat() {
        return new TextConfig {
            Text = "Placeholder",
            FontSize = 18,
            OutlineThickness = 3,
        };
    }

    private static TextConfig WithColor(this TextConfig config, uint color) {
        config.Color = color;
        return config;
    }
}