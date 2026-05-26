using AlloyClient.Display;
using AlloyClient.State;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;

namespace AlloyClient.Screens.Components;

public enum ScreenType {
    Loading,
    Title,
    Other
}

public abstract class TitleScreenBase : Screen {
    
    private readonly ScreenDarkenOverlay _darken = new();
    
    private readonly MusicButton _music = new MusicButton(new MusicButtonConfig { X = 7, Y = 7, Width = 32, Height = 32 });

    public readonly AccountOverlay Overlay;
    
    protected TitleScreenBase(ScreenType type = ScreenType.Other) {
        var background = new ScreenGraphic(type == ScreenType.Title);
        AddChild(background);
        
        if (type == ScreenType.Other) {
            AddChild(_darken);
        }
        
        AddChild(_music);
        
        //Todo guild/stars

        Overlay = new AccountOverlay(type == ScreenType.Title);
        Overlay.X = Settings.DefaultScreenWidth - 10;
        Overlay.Y = 10;
        Overlay.SetAnchor(UiAnchor.RightTop);

        if (type != ScreenType.Loading) {
            AddChild(Overlay);
        }
        
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
        Overlay.Scale = Stage.ScreenScale;
        Overlay.X = args.Width - (int)(10 * Stage.ScreenScale.X);
        Overlay.Y = (int)(10 * Stage.ScreenScale.Y);
    }
}