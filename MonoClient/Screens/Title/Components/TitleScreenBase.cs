using MonoClient.Display;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.Ui.Components.Graphics;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components;

public abstract class TitleScreenBase : Screen {
    
    private readonly ColorRect _darken = new ColorRect(new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });
    
    private readonly MusicButton _music = new MusicButton(new MusicButtonConfig { Width = 36, Height = 36 });

    private readonly AccountOverlay _overlay;
    
    protected TitleScreenBase(bool title = false) {
        var background = new ScreenGraphic(title);
        AddChild(background);
        
        if (!title) {
            AddChild(_darken);
        }
        
        AddChild(_music);
        
        //Todo guild/stars

        _overlay = new AccountOverlay(title);
        _overlay.X = Settings.DefaultScreenWidth - 10;
        _overlay.Y = 10;
        _overlay.SetAnchor(UiAnchor.RightTop);
        AddChild(_overlay);
        
        SetAutoResize(OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        _darken.Resize(args.Width, args.Height);
        _music.Scale = UiRender.ScreenScale;
        _overlay.Scale = UiRender.ScreenScale;
        _overlay.X = args.Width - (int)(10 * UiRender.ScreenScale.X);
        _overlay.Y = (int)(10 * UiRender.ScreenScale.Y);
    }
}