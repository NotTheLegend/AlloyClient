using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Chat;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud.Chat;

// TODO:
// Scroll up/down chat history

public class ChatView : Sprite {
    public static int MaxWidth { get; private set; } = Settings.DefaultScreenWidth / 2;

    private static readonly Queue<ChatLineData> ChatLineQueue = new();
    public static void QueueChatLine(ChatLineData data) => ChatLineQueue.Enqueue(data);

    private static bool ClearChatRequested;
    public static void ClearChat() => ClearChatRequested = true;

    private ChatContainer _chatContainer;
    private TextInput _chatBox;
    private bool _inFocus;

    private string _recentTeller = "";// string.Empty;

    public ChatView() {
        Width = MaxWidth;
        Height = Settings.DefaultScreenHeight;

        _chatBox = new TextInput(new InputConfig {
            FontSize = 18,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            ClickToActivate = true,
            Width = Width,
            OnFocus = OnChatFocus,
            OnUnfocus = OnChatUnFocus
        });
        AddChild(_chatBox);

        _chatContainer = new ChatContainer(new ContainerConfig {
            EnableClip = true,
            Width = Width,
            Height = Height - _chatBox.Height,
        });
        AddChild(_chatContainer);

        _chatBox.Y = _chatContainer.Height;
        _chatBox.Visible = false;

        InputHandler.OnChatKey.Set(OnChatKey);
        InputHandler.OnChatOpen.Set(OnChatOpen);
        InputHandler.OnChatHistoryUp.Set(_chatContainer.PageUp);
        InputHandler.OnChatHistoryDown.Set(_chatContainer.PageDown);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (ClearChatRequested) {
            _chatContainer.Clear();
            ClearChatRequested = false;
        }

        while (ChatLineQueue.TryDequeue(out var chatLineData)) {
            _chatContainer.AddChatLine((int) gameTime.TotalGameTime.TotalMilliseconds, chatLineData);
        }
    }

    private void OnChatFocus() {
        InputHandler.AddInputBlocker(InputBlockers.Chat);
        _inFocus = true;
    }

    private void OnChatUnFocus() {
        InputHandler.RemoveInputBlocker(InputBlockers.Chat);
        _inFocus = false;
    }

    private void OnChatKey() {
        if (_inFocus) {
            var msg = _chatBox.Text;
            if (!string.IsNullOrEmpty(msg)) {
                var text = PlayerText.CreatePacket();
                text.Text = msg;
                Client.QueuePacket(text);
                
                if(msg[0] != '/')
                    ChatLayer.QueueSpeech(new SpeechData {Owner = Map.LocalPlayer, Text = _chatBox.Text});
            }

            _chatBox.UnFocus(true);
            _chatBox.Visible = false;
        } else {
            OpenTextInput(_chatBox.Text);
        }
    }

    private void OnChatOpen(string text) {
        if (_chatBox.Text != "") return;
        
        if (text == "/tell " && !string.IsNullOrEmpty(_recentTeller))
            text = $"/tell {_recentTeller} ";
        
        OpenTextInput(text);
    }

    private void OpenTextInput(string defaultText) {
        if (!_inFocus) {
            _chatBox.Focus();
            _chatBox.Visible = true;
        }

        if (!string.IsNullOrEmpty(defaultText)) {
            _chatBox.InsertText(defaultText);
        }
    }
}