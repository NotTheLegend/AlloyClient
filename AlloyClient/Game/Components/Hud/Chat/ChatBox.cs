using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Signals;
using Common;

namespace AlloyClient.Game.Components.Hud.Chat;

public class ChatBox : Sprite {

    private const int MaxWidth = Settings.DefaultScreenWidth / 2;
    private const int MaxHeight = Settings.DefaultScreenHeight / 2;
    
    public static readonly Signal OnChatKey = new(); // todo: merge chatkey & chatopen together
    public static readonly Signal<string> OnChatOpen = new();
    public static readonly Signal OnChatHistoryUp = new();
    public static readonly Signal OnChatHistoryDown = new();

    private readonly RollingList<ChatBoxLine> _lines = new(100);
    
    private readonly TextInput _chatInput;
    private bool _inFocus;
    
    private string _recentTeller = string.Empty;

    public ChatBox() {
        Y = Settings.DefaultScreenHeight;
        SetAnchor(UiAnchor.LeftBottom);

        var t = new ColorRect(new ColorRectConfig {
            Width = MaxWidth,
            Height = MaxHeight,
            Color = 0,
            Alpha = 0.5f
        });
        //AddChild(t); // todo: replace with chat history

        var container = new Container(new ContainerConfig {
            Width = MaxWidth,
            Height = MaxHeight,
            EnableClip = true
        });
        AddChild(container);
        
        _chatInput = new TextInput(new InputConfig {
            Y = t.Height,
            FontSize = 18,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            ClickToActivate = true,
            Width = MaxWidth,
            OnFocus = FocusTextInput,
            OnUnfocus = UnfocusTextInput
        });
        
        AddEventListener(Event.AddedToStage, AddHandlers);
        AddEventListener(Event.RemovedFromStage, RemoveHandlers);
    }
    
    private void AddHandlers() {
        OnChatKey.Add(HandleChatKey);
        OnChatOpen.Add(HandleChatOpen);
        //OnChatHistoryUp.Add(_chatContainer.PageUp);
        //OnChatHistoryDown.Add(_chatContainer.PageDown);
        //AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void RemoveHandlers() {
        OnChatKey.Remove(HandleChatKey);
        OnChatOpen.Remove(HandleChatOpen);
        //OnChatHistoryUp.Remove(_chatContainer.PageUp);
        //OnChatHistoryDown.Remove(_chatContainer.PageDown);
        //RemoveEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void HandleChatKey() {
        if (_inFocus) {
            var hasText = _chatInput.HasText(true);
            if (hasText) {
                var text = PlayerText.CreatePacket();
                text.Text = _chatInput.Text;
                Client.QueuePacket(text);
            }
            
            OnKeyUnfocus(hasText);
        } else {
            OnKeyFocus();
        }
    }

    private void HandleChatOpen(string text) {
        if (text == "/tell " && !string.IsNullOrWhiteSpace(_recentTeller))
            text = $"/tell {_recentTeller} ";
        
        _chatInput.SetText(text);
        OnKeyFocus();
    }

    private void OnKeyFocus() {
        if (!Contains(_chatInput)) {
            AddChild(_chatInput);
        }
        _chatInput.Focus();
    }
    
    private void OnKeyUnfocus(bool clear) {
        if (clear || !_chatInput.HasText(false)) {
            RemoveChild(_chatInput);
        }
        _chatInput.UnFocus(clear);
    }

    private void FocusTextInput() {
        UserInput.SetManualFocus(false);
        Stage.AddEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        _inFocus = true;
    }

    private void UnfocusTextInput() {
        UserInput.SetManualFocus(true);
        Stage.RemoveEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        _inFocus = false;
    }
    
    private void OnKeyDown(KeyboardEvent args) {
        if (args.Code == Settings.Chat.Key) {
            HandleChatKey();
        }
    }
    
}