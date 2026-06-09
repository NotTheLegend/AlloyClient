using AlloyClient.Game.Components.Hud;
using AlloyClient.Ui.Character;
using AlloyClient.Ui.Chat;
using Alloy.UiLib.Core;
using AlloyClient.Game.Components.Hud.Chat;
using AlloyClient.Ui.Components.Elements;

namespace AlloyClient.Game.Components;

public sealed class GameSprite : Sprite {

    public readonly UserInput UserInput;
    public readonly HudView Hud;
    private readonly ChatBox _chat;

    public GameSprite() {
        AddChild(UserInput = new UserInput());
        AddChild(new ChatLayer());
        AddChild(new NotificationLayer());
        AddChild(Hud = new HudView());
        AddChild(_chat= new ChatBox());
        AddChild(new DebugStats());

        Map.GameSprite = this;
        
        SetPosition(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight);
        
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
    
    
    
    private void OnResize(ResizeEvent args) {
        SetPosition(args.Width, args.Height);
    }

    private void SetPosition(int width, int height) {
        Hud.X = width;
        Hud.Y = height / 2;
        Hud.Scale = Stage.ScreenScale;

        _chat.X = 0;
        _chat.Y = height;
        _chat.Scale = Stage.ScreenScale;
    }
}