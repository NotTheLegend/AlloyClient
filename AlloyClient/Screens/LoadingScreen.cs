using System.Threading.Tasks;
using AlloyClient.AppEngine;
using AlloyClient.Assets;
using AlloyClient.Display;
using AlloyClient.State;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Extra;
using AlloyClient.Models;
using AlloyClient.Utils;

namespace AlloyClient.Screens;

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
        
        this.SetAutoResize(OnResize);
        
        AddEventListener(Task.WhenAll(
            AppRequests.Startup(),
            //Task.Run(SoundManager.PreLoadSounds),
            //Task.Run(Music.PreLoadSongs),
            AssetParser.ParseAssetsAsync(),
            Task.Delay(2000) // Loading screen too fast lmao
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }
    
    private new void OnResize(ResizeEvent args) {
        _text.Scale = Stage.ScreenScale;
        _text.X = Stage.StageWidth / 2;
        _text.Y = Stage.StageHeight - (int)(90 * Stage.ScreenScale.Y);
    }
}