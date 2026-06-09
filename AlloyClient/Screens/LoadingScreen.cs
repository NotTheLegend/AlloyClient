using System.Threading.Tasks;
using AlloyClient.AppEngine;
using AlloyClient.Assets;
using AlloyClient.Display;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;
using AlloyClient.Screens.Components;

namespace AlloyClient.Screens;

public class LoadingScreen : TitleScreenBase {

    private const int MinLoadingTime = 2000;

    private readonly SimpleText _text;
    
    public LoadingScreen(bool isRetry = false) : base(Components.ScreenType.Loading) {
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
        
        AddEventListener(Task.WhenAll(
            AppRequests.Startup(),
            isRetry ? Task.CompletedTask : AssetParser.LoadAssetsAsync(),
            Task.Delay(MinLoadingTime)
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }

    protected override void OnResize(ResizeEvent args) {
        _text.Scale = Stage.ScreenScale;
        _text.X = Stage.StageWidth / 2;
        _text.Y = Stage.StageHeight - (int)(90 * Stage.ScreenScale.Y);
    }
}