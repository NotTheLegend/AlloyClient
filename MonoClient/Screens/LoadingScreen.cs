using System;
using System.Threading.Tasks;
using MonoClient.Assets;
using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.Title;
using MonoClient.State;
using MonoClient.Ui.Components.Graphics;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Extra;

namespace MonoClient.Screens;

public class LoadingScreen : Screen {

    private readonly SimpleText _text;
    
    public LoadingScreen() {
        var background = new ScreenGraphic();
        AddChild(background);

        _text = new SimpleText(new TextConfig {
            Text = "Loading...",
            FontSize = 40,
            FontType = FontType.Bold,
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