using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.Screens.Game.Components.Hud.Chat;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Chat;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components;

public sealed class GameSprite : Sprite {

    private readonly HudView _hud;
    private readonly ChatView _chat;

    public GameSprite() {
        AddChild(new ChatLayer());
        
        _hud = new HudView();
        _hud.X = Settings.DefaultScreenWidth;
        AddChild(_hud);

        _chat = new ChatView();
        AddChild(_chat);
    }

    public void Update() {
        _hud.Update();
    }
}