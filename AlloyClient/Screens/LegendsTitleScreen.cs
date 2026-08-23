using AlloyClient.Screens.Components;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.Display;
using AlloyClient.Ui.Components.Buttons;

namespace AlloyClient.Screens;

public class LegendsTitleScreen : TitleScreenBase {

    public LegendsTitleScreen() {
        var backButton = new MenuBarButton("back", TitleScreen.FontSize,
            () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });

        backButton.SetAnchor(UiAnchor.Middle);
        MenuBar.AddChild(backButton);
    }
}