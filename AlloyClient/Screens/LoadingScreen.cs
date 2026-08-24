using System.Threading.Tasks;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.AppEngine;
using AlloyClient.Assets;
using AlloyClient.Display;
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
            Color = 0xFFFFFF,
            Anchor = UiAnchor.Middle
        });

        MenuBar.AddChild(_text);

        AddEventListener(Task.WhenAll(
            AppRequests.Startup(),
            isRetry ? Task.CompletedTask : AssetParser.LoadAssetsAsync(),
            Task.Delay(MinLoadingTime)
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }
}