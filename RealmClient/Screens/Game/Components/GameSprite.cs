using RealmClient.UiLib.Core;
using RealmClient.Screens.Game.Components.Hud;
using RealmClient.Screens.Game.Components.Hud.Chat;
using RealmClient.State;
using RealmClient.Ui.Character;
using RealmClient.Ui.Chat;

namespace RealmClient.Screens.Game.Components;

public sealed class GameSprite : Sprite {

    public readonly UserInput UserInput;
    public readonly HudView Hud;
    private readonly ChatView _chat;

    public GameSprite() {
        AddChild(UserInput = new UserInput());
        AddChild(new ChatLayer());
        AddChild(new NotificationLayer());
        
        Hud = new HudView();
        Hud.X = Settings.DefaultScreenWidth;
        AddChild(Hud);

        _chat = new ChatView();
        AddChild(_chat);

        Map.GameSprite = this;
    }

    public void Update() {
        Hud.Update();//TODO: move to OnUpdate
    }
}