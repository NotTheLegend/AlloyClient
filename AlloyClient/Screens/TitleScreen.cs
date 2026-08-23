using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Screens.Components;
using AlloyClient.Screens.Components.Containers;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Dialogs;
using AlloyClient.Ui.Components.Graphics;

namespace AlloyClient.Screens;

public class TitleScreen : TitleScreenBase {

    public const int PlayFontSize = 57;
    public const int FontSize = 35;

    public TitleScreen() : base(Components.ScreenType.Title) {
        var editor = new MenuBarButton("editor", FontSize, () => { });
        editor.SetAnchor(UiAnchor.MiddleRight);
        MenuBar.AddChild(editor);

        var servers = new MenuBarButton("servers", FontSize, () => ScreenManager.FadeTo(new ServersTitleScreen()));
        servers.SetAnchor(UiAnchor.MiddleRight);
        MenuBar.AddChild(servers);

        var play = new MenuBarButton("play", PlayFontSize, OnPlay, true);
        play.SetAnchor(UiAnchor.Middle);
        MenuBar.AddChild(play);

        servers.X = -play.Width / 2 - MenuGap;
        editor.X = servers.X - servers.Width - MenuGap;

        var legends = new MenuBarButton("legends", FontSize, () => ScreenManager.FadeTo(new LegendsTitleScreen()));
        legends.SetAnchor(UiAnchor.MiddleLeft);
        legends.X = play.Width / 2 + MenuGap;
        MenuBar.AddChild(legends);

        var exit = new MenuBarButton("exit", FontSize, () => Main.OnQuit.Dispatch());
        exit.SetAnchor(UiAnchor.MiddleLeft);
        exit.X = legends.X + legends.Width + MenuGap;
        MenuBar.AddChild(exit);

        CheckForAppFailure();
    }

    private void OnPlay() {
        if (GlobalData.Contains<LoginData>()) {
            ScreenManager.FadeTo(new CharacterListScreen());
        } else {
            var login = new LoginContainer();
            login.AddEventListener(LoginContainer.LoginEvent, Overlay.OnLogin);
            OverlayManager.Set(login);
        }
    }

    private void CheckForAppFailure() {
        if (!GlobalData.TryRemove<AppRequestFailedFlag>(out var data)) {
            return;
        }

        AddChild(new ScreenDarkenOverlay());

        DialogManager.Enqueue(new RetryLoadDialog(data.Message));
    }
}