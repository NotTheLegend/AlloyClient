using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Chat;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components;

public sealed class GameSprite : Sprite {

    private readonly HudView _hud;
    private readonly TextInput _chatBox;

    public GameSprite() {
        AddChild(new ChatLayer());
        
        
        _hud = new HudView();
        _hud.X = Settings.DefaultScreenWidth;
        _hud.SetAnchor(UiAnchor.RightTop);
        AddChild(_hud);

        _chatBox = new TextInput(new InputConfig { Y = Settings.DefaultScreenHeight, FontSize = 18, Bold = true, OutlineThickness = 2, Width = Settings.DefaultScreenWidth - 256, ClickToActivate = false, Anchor = UiAnchor.LeftBottom });
        _chatBox.Visible = false;
        AddChild(_chatBox);
        InputHandler.OnChatKey.Add(OnChatBox);
    }

    public void Update() {
        _hud.Update();
    }

    private void OnChatBox(bool active) {
        if (active) {
            _chatBox.SetActive();
            _chatBox.Visible = true;
        } else {
            var text = PlayerText.CreatePacket();
            text.Text = _chatBox.Text;
            Client.QueuePacket(text);
            
            ChatLayer.QueueSpeech(new SpeechData { Owner = Map.LocalPlayer, Text = _chatBox.Text});
            
            _chatBox.Clear();
            _chatBox.Visible = false;
        }
    }
    
}