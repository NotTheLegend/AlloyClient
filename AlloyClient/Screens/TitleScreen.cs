using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Screens.Components;
using AlloyClient.Screens.Components.Containers;
using AlloyClient.Ui.Components.Buttons;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;
using AlloyClient.Ui.Components.Dialogs;
using AlloyClient.Ui.Components.Graphics;

namespace AlloyClient.Screens;

public class TitleScreen : TitleScreenBase {
    
    public const int PlayFontSize = 57;
    public const int FontSize = 35;
    
    private readonly Container _container = new(new ContainerConfig { Anchor = UiAnchor.MiddleTop });

    private readonly int _center;
    
    public TitleScreen() : base(ScreenType.Title) {
        var editor = new MenuBarButton("editor", FontSize, () => { });
        editor.SetAnchor(UiAnchor.MiddleLeft);
        _container.AddChild(editor);
        
        var servers = new MenuBarButton("servers", FontSize, () => ScreenManager.FadeTo(new ServersTitleScreen()));
        servers.SetAnchor(UiAnchor.MiddleLeft);
        servers.X = editor.Width + 50;
        _container.AddChild(servers);
        
        var play = new MenuBarButton("play", PlayFontSize, OnPlay, true);
        play.SetAnchor(UiAnchor.Middle);
        play.X = servers.X + servers.Width + play.Width / 2 + 50;
        _container.AddChild(play);
        
        var legends = new MenuBarButton("legends", FontSize, () => ScreenManager.FadeTo(new LegendsTitleScreen()));
        legends.SetAnchor(UiAnchor.MiddleLeft);
        legends.X = play.X + play.Width / 2 + 50;
        _container.AddChild(legends);
        
        var exit = new MenuBarButton("exit", FontSize, () => { Main.GameInstance.Exit(); });
        exit.SetAnchor(UiAnchor.MiddleLeft);
        exit.X = legends.X + legends.Width + 50;
        _container.AddChild(exit);
        
        _center = play.X - _container.Width / 2;
        _container.X = Settings.DefaultScreenWidth / 2 - _center;
        _container.Y = Settings.DefaultScreenHeight - 90;
        AddChild(_container);
        
        CheckForAppFailure();
    }

    protected override void OnResize(ResizeEvent args) {
        _container.Scale = Stage.ScreenScale;
        _container.X = Stage.StageWidth / 2 - _center;
        _container.Y = Stage.StageHeight - (int)(90 * Stage.ScreenScale.Y);
        base.OnResize(args);
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
        if (!GlobalData.TryGet<AppRequestFailedFlag>(out var data)) {
            return;
        }
        
        GlobalData.Remove<AppRequestFailedFlag>();

        AddChild(new ScreenDarkenOverlay());
        
        DialogManager.Enqueue(new Dialog(data.Message, "", new DialogOption("Retry", () => { ScreenManager.FadeToScreen(new LoadingScreen(true), Easing.SineInOut, 500, 0); }), new DialogOption("Quit", () => { Main.GameInstance.Exit(); })));
    }
}