using System;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Signals;
using Alloy.Common;
using Alloy.Common.Collections;

namespace AlloyClient.Game.Components.Hud.Chat;

public class ChatBox : Sprite {
    public const int MaxWidth = Settings.DefaultScreenWidth / 2;
    private const int MaxHeight = Settings.DefaultScreenHeight / 2 - 2;
    private const int MaxLines = 7;
    private const int LinePadding = 4;

    public static readonly SingleSignal<ChatBoxLineData> AddChatLine = new();
    
    public static readonly Signal<string> OnChatOpen = new();
    public static readonly Signal OnChatHistoryUp = new();
    public static readonly Signal OnChatHistoryDown = new();

    private readonly RollingList<ChatBoxLineData> _lines = new(100);

    private readonly Container _chatContainer;
    private readonly TextInput _chatInput;
    private bool _inFocus;
    private string _recentTeller = string.Empty;
    
    
    private readonly Timer _timer = new(1000);

    private int _lineOffset;
    private bool _showMax;

    public ChatBox() {
        Y = Settings.DefaultScreenHeight;
        Anchor = UiAnchor.LeftBottom;

        _chatContainer = new Container(new ContainerConfig {
            Width = MaxWidth,
            Height = MaxHeight,
            EnableClip = false
        });
        AddChild(_chatContainer);
        
        _chatInput = new TextInput(new InputConfig {
            Y = MaxHeight,
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
        AddChatLine.Set(AddChatBoxLine);
        OnChatOpen.Add(HandleChatOpen);
        OnChatHistoryUp.Add(OnPageUp);
        OnChatHistoryDown.Add(OnPageDown);
        _timer.AddEventListener(TimerEvent.Timer, Refresh);
        _timer.Start();
    }

    private void RemoveHandlers() {
        AddChatLine.Remove();
        OnChatOpen.Remove(HandleChatOpen);
        OnChatHistoryUp.Remove(OnPageUp);
        OnChatHistoryDown.Remove(OnPageDown);
        _timer.RemoveEventListener(TimerEvent.Timer, Refresh);
        _timer.Stop();
    }

    private void OnPageUp() {
        if (_showMax) {
            _lineOffset = Math.Max(0,Math.Min(_lines.Count - MaxLines ,_lineOffset + MaxLines));
        } else {
            _showMax = true;
        }
        
        Refresh();
    }

    private void OnPageDown() {
        if (_lineOffset == 0) {
            _showMax = false;
        } else {
            _lineOffset = Math.Max(0, _lineOffset - MaxLines);
        }
        
        Refresh();
    }

    private void AddChatBoxLine(ChatBoxLineData data) {
        _lines.Add(data);
        Refresh();
    }

    private void Refresh() {
        _chatContainer.RemoveChildren();

        var now = Main.GetTime();
        var yPos = MaxHeight;

        var startLine = Math.Max(0, _lines.Count - _lineOffset) - 1;
        var endLine = Math.Max(0, _lines.Count - _lineOffset - MaxLines - 1);

        for (var i = startLine; i >= endLine; i--) {
            var line = _lines[i];
            if (!_showMax && now > line.Time + 20000) {
                continue;
            }
                
            var sprite = line.Sprite;
            sprite.X = 3;
            sprite.Y = yPos -= sprite.Height + LinePadding;

            _chatContainer.AddChild(line.Sprite);
        }
    }

    private void HandleChatKey() {
        if (_inFocus) {
            var hasText = _chatInput.HasText(true);
            if (hasText) {
                var textPacket = PlayerText.CreatePacket();
                textPacket.Text = _chatInput.Text;
                Client.QueuePacket(textPacket);
            }
            
            OnKeyUnfocus(hasText);
        } else {
            OnKeyFocus();
        }
    }

    private void HandleChatOpen(string text) {
        if (text == string.Empty) {
            HandleChatKey();
            return;
        }

        if (text == "/tell " && !string.IsNullOrWhiteSpace(_recentTeller)) {
            text = $"/tell {_recentTeller} ";
        }
        
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