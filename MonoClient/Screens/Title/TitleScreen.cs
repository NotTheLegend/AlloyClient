using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.MapEditor;
using MonoClient.Screens.Title.Components;
using MonoClient.Screens.Title.Components.Panels;
using MonoClient.Screens.Title.ServerListScreen;
using MonoClient.Screens.Title.ServersListScreen;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.UiLib;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens.Title;

public class TitleScreen : TitleScreenBase {
    
    public const int PlayFontSize = 57;
    public const int FontSize = 35;
    
    public TitleScreen() : base(true) {
        var playButton = new MenuBarButton("play", PlayFontSize, () => {
            if (!Account.LoggedIn) {
                Logger.Info("[TODO Dialog text]. not logged in");
                return;
            }
            
            if (Account.LoggedIn) {
                ScreenManager.FadeToScreen(new CharacterListScreen(), Easing.SineInOut, 1000, 0x0);
            }
            else {
                OverlayManager.Enqueue(new LoginContainer());
            }
        }, true);
        playButton.SetAnchor(UiAnchor.Middle);
        playButton.X = Settings.DefaultScreenWidth / 2;
        playButton.Y = Settings.DefaultScreenHeight - 50;
        AddChild(playButton);

        var serversButton = new MenuBarButton("servers", FontSize, () => {
            ScreenManager.FadeToScreen(new ServersTitleScreen(), Easing.SineInOut, 1000, 0x0);
        });
        serversButton.X = playButton.X - playButton.Width / 2 - serversButton.Width - 50;
        serversButton.Y = Settings.DefaultScreenHeight - 50 - serversButton.Height / 2;
        AddChild(serversButton);

        var editorButton = new MenuBarButton("editor", FontSize, () => { ScreenManager.FadeToScreen(new MapEditorScreen(), Easing.SineInOut, 1000, 0x0); });
        editorButton.X = serversButton.X - editorButton.Width - 50;
        editorButton.Y = Settings.DefaultScreenHeight - 50 - editorButton.Height / 2;
        AddChild(editorButton);

        var legendsButton = new MenuBarButton("legends", FontSize, () => { ScreenManager.FadeToScreen(new LegendsTitleScreen(), Easing.SineInOut, 1000, 0x0); });
        legendsButton.X = playButton.X + playButton.Width / 2 + 50;
        legendsButton.Y = Settings.DefaultScreenHeight - 50 - legendsButton.Height / 2;
        AddChild(legendsButton);

        var exitButton = new MenuBarButton("exit", FontSize, () => { Main.GameInstance.Exit(); });
        exitButton.X = legendsButton.X + legendsButton.Width + 50;
        exitButton.Y = Settings.DefaultScreenHeight - 50 - exitButton.Height / 2;
        AddChild(exitButton);
    }
    
}