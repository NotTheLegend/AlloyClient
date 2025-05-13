using System;
using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.MapEditor;
using MonoClient.Screens.Title;
using MonoClient.Screens.Title.Components.Panels;
using MonoClient.Screens.Title.ServerListScreen;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.Ui.Components.Graphics;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens;

public class TestScreen : Screen {
    
    public const int PlayFontSize = 57;
    public const int FontSize = 35;

    private readonly Container _container = new(new ContainerConfig { Anchor = UiAnchor.MiddleTop });
    
    public TestScreen() {
        var background = new ScreenGraphic();
        AddChild(background);
        
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
        
        SetAutoResize(OnResize);
    }

    private void OnResize(ResizeEvent args) {
        _container.Scale = UiRender.ScreenScale;
        _container.X = Stage.StageWidth / 2;
        _container.Y = Stage.StageHeight - (int)(90 * UiRender.ScreenScale.Y);
    }
    
}