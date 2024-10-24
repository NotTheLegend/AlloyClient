using MonoClient.Sound;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.Ui.Components.Graphics;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;

namespace MonoClient.Screens.Title.Components;

public abstract class TitleScreenBase : Screen {
    protected TitleScreenBase(bool title = false) {
        Main.GameInstance.SetTitleGraphics();
        
        var background = new ScreenGraphic(new ScreenGraphicConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, TitleScreen = title });
        AddChild(background);
        
        if (!title) {
            var darken = new ColorRect(new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });
            AddChild(darken);
        }

        var musicButton = new MusicButton(new MusicButtonConfig { Width = 36, Height = 36 });
        AddChild(musicButton);
        
        // Todo account login/out/guild/stars

        var accOverlay = new AccountOverlay(title);
        AddChild(accOverlay);
    }
}