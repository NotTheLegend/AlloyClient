using RealmClient.Display;
using RealmClient.Game.Components.Hud;
using RealmClient.Game.Components.Hud.Chat;
using RealmClient.State;
using RealmClient.Ui.Character;
using RealmClient.Ui.Chat;
using RealmClient.UiLib;
using RealmClient.UiLib.Core;
using RealmClient.Utils;

namespace RealmClient.Game.Components;

public sealed class GameSprite : Sprite {

    public readonly UserInput UserInput;
    public readonly HudView Hud;
    private readonly ChatView _chat;

    public GameSprite() {
        AddChild(UserInput = new UserInput());
        AddChild(new ChatLayer());
        AddChild(new NotificationLayer());
        AddChild(Hud = new HudView());
        AddChild(_chat= new ChatView());

        Map.GameSprite = this;
        
        this.SetAutoResize(OnResize);
        SetPosition(Settings.ScreenWidth, Settings.ScreenHeight);
    }
    
    private void OnResize(ResizeEvent args) {
        SetPosition(args.Width, args.Height);
    }

    private void SetPosition(int width, int height) {
        /* FIXME 
         * this fixes bug: first item move drops regardless of target
         * bounds are just wrong until 2nd end drag for some reason
         * would like to fix the root cause
         */
        //todo:SetBaseDimensions(width, height);
        
        Hud.X = width;
        Hud.Y = height / 2;
        Hud.Scale = Stage.ScreenScale;

        _chat.X = 0;
        _chat.Y = height;
        _chat.Scale = Stage.ScreenScale;
    }
}