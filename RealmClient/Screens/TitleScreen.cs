using RealmClient.Data;
using RealmClient.Display;
using RealmClient.Screens.Components;
using RealmClient.Screens.Components.Panels;
using RealmClient.State;
using RealmClient.Ui.Components.Buttons;
using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;

namespace RealmClient.Screens;

public class TitleScreen : TitleScreenBase {
    
    public const int PlayFontSize = 57;
    public const int FontSize = 35;
    
    private readonly Container _container = new(new ContainerConfig { Anchor = UiAnchor.MiddleTop });
    
    public TitleScreen() : base(true) {
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

        _container.X = Settings.DefaultScreenWidth / 2;
        _container.Y = Settings.DefaultScreenHeight - 90;
        AddChild(_container);
        
        SetAutoResize(OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        _container.Scale = UiRender.ScreenScale;
        _container.X = Stage.StageWidth / 2;
        _container.Y = Stage.StageHeight - (int)(90 * UiRender.ScreenScale.Y);
        base.OnResize(args);
    }

    private void OnPlay() {
        if (Account.LoggedIn) {
            ScreenManager.FadeTo(new CharacterListScreen());
        }
        else {
            OverlayManager.Enqueue(new LoginContainer());
        }
    }
}