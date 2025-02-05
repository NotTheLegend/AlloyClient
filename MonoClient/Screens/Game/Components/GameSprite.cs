using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.Screens.Game.Components.Hud.Chat;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Character;
using MonoClient.Ui.Chat;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components;

public sealed class GameSprite : Sprite {

    public readonly HudView Hud;
    private readonly ChatView _chat;

    public GameSprite() {
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
        Hud.Update();
    }
}