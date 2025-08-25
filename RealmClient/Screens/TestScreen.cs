using RealmClient.State;
using RealmClient.Ui.Components.Buttons;
using RealmClient.Ui.Components.Graphics;
using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.Display;

namespace RealmClient.Screens;

public class TestScreen : Screen {
    
    public const int PlayFontSize = 57;
    public const int FontSize = 35;

    private readonly Container _container = new(new ContainerConfig { Anchor = UiAnchor.MiddleTop });
    
    public TestScreen() {
        //var background = new ScreenGraphic();
        var background = new ColorRect(new ColorRectConfig {
            Width = 1280,
            Height = 720,
            Anchor = UiAnchor.LeftTop,
            MouseEnabled = false
        });
        background.Color.PackedValue = 0xFFFFFFFF;
        //background.SetColor(0xFF0000);
        //background.SetColor(0x00FF00, 1);
        //background.SetColor(0x0000FF);
        AddChild(background);
        /*
        var editor = new MenuBarButton("editor", FontSize, () => { });
        editor.SetAnchor(UiAnchor.MiddleLeft);
        _container.AddChild(editor);
        
        var servers = new MenuBarButton("servers", FontSize, () => { });
        servers.SetAnchor(UiAnchor.MiddleLeft);
        servers.X = editor.Width + 50;
        _container.AddChild(servers);
        
        var play = new MenuBarButton("play", PlayFontSize, () => { }, true);
        play.SetAnchor(UiAnchor.Middle);
        play.X = servers.X + servers.Width + play.Width / 2 + 50;
        _container.AddChild(play);
        
        var legends = new MenuBarButton("legends", FontSize, () => { });
        legends.SetAnchor(UiAnchor.MiddleLeft);
        legends.X = play.X + play.Width / 2 + 50;
        _container.AddChild(legends);
        
        var exit = new MenuBarButton("exit", FontSize, () => { Main.GameInstance.Exit(); });
        exit.SetAnchor(UiAnchor.MiddleLeft);
        exit.X = legends.X + legends.Width + 50;
        _container.AddChild(exit);

        _container.X = Settings.DefaultScreenWidth/ 2;
        _container.Y = Settings.DefaultScreenHeight - 90;
        AddChild(_container);
        */
        
        //SetAutoResize(OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        //_container.Scale = UiRender.ScreenScale;
        //_container.X = Stage.StageWidth / 2;
        //_container.Y = Stage.StageHeight - (int)(90 * UiRender.ScreenScale.Y);
    }
    
}