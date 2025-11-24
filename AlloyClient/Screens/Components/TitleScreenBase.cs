using AlloyClient.Display;
using AlloyClient.State;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;

namespace AlloyClient.Screens.Components;

public abstract class TitleScreenBase : Screen {
    
    private readonly ColorRect _darken = new ColorRect(new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });
    
    private readonly MusicButton _music = new MusicButton(new MusicButtonConfig { X = 7, Y = 7, Width = 32, Height = 32 });

    public readonly AccountOverlay Overlay;
    
    protected TitleScreenBase(bool title = false) {
        var background = new ScreenGraphic(title);
        AddChild(background);
        
        if (!title) {
            AddChild(_darken);
        }
        
        AddChild(_music);
        
        //Todo guild/stars

        Overlay = new AccountOverlay(title);
        Overlay.X = Settings.DefaultScreenWidth - 10;
        Overlay.Y = 10;
        Overlay.SetAnchor(UiAnchor.RightTop);
        AddChild(Overlay);
        
        AddEventListener(Event.AddedToStage, OnStageEnter);
        AddEventListener(Event.RemovedFromStage, OnStageExit);
    }

    private void OnStageEnter() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnResize(new ResizeEvent("", Stage.StageWidth, Stage.StageHeight));
    }

    private void OnStageExit() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        _darken.Resize(args.Width, args.Height);
        _music.Scale = Stage.ScreenScale;
        Overlay.Scale = Stage.ScreenScale;
        Overlay.X = args.Width - (int)(10 * Stage.ScreenScale.X);
        Overlay.Y = (int)(10 * Stage.ScreenScale.Y);
    }
}