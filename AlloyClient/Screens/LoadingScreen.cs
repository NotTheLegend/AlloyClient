using System.Threading.Tasks;
using AlloyClient.AppEngine;
using AlloyClient.Assets;
using AlloyClient.Display;
using AlloyClient.State;
using AlloyClient.Ui.Components.Graphics;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;

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
        
        AddEventListener(Event.AddedToStage, OnStageEnter);
        AddEventListener(Event.RemovedFromStage, OnStageExit);
        
        AddEventListener(Task.WhenAll(
            AppRequests.Startup(),
            //Task.Run(SoundManager.PreLoadSounds),
            //Task.Run(Music.PreLoadSongs),
            AssetParser.LoadAssetsAsync(),
            Task.Delay(2000) // Loading screen too fast lmao
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }

    private void OnStageEnter() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnResize(new ResizeEvent("", Stage.StageWidth, Stage.StageHeight));
    }

    private void OnStageExit() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
    }
    
    protected override void OnResize(ResizeEvent args) {
        _text.Scale = Stage.ScreenScale;
        _text.X = Stage.StageWidth / 2;
        _text.Y = Stage.StageHeight - (int)(90 * Stage.ScreenScale.Y);
    }
}