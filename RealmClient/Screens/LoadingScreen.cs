using System;
using System.Threading.Tasks;
using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;
using RealmClient.Assets;
using RealmClient.Data;
using RealmClient.Display;
using RealmClient.Screens.Title;
using RealmClient.State;
using RealmClient.Ui.Components.Graphics;

namespace RealmClient.Screens;

public class LoadingScreen : Screen {

    private readonly SimpleText _text;
    
    public LoadingScreen() {
        var background = new ScreenGraphic();
        AddChild(background);

        _text = new SimpleText(new TextConfig {
            Text = "Loading...",
            FontSize = 40,
            FontType = FontType.Bold,
            OutlineThickness = 4,
            X = Settings.DefaultScreenWidth / 2,
            Y = Settings.DefaultScreenHeight - 90,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.Middle
        });
        AddChild(_text);
        
        SetAutoResize(OnResize);
        
        AddEventListener(Task.WhenAll(
            Account.LoadAsync(),
            //Task.Run(SoundManager.PreLoadSounds),
            //Task.Run(Music.PreLoadSongs),
            AssetParser.ParseAssetsAsync(),
            Task.Delay(2000) // Loading screen too fast lmao
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }
    
    private void OnResize(ResizeEvent args) {
        _text.Scale = UiRender.ScreenScale;
        _text.X = Stage.StageWidth / 2;
        _text.Y = Stage.StageHeight - (int)(90 * UiRender.ScreenScale.Y);
    }
}