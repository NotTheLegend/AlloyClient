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
    public static bool IsTyping = false;
    
    private static readonly Queue<ChatLineData> ChatLineQueue = new();
    public static void QueueChatLine(ChatLineData data) => ChatLineQueue.Enqueue(data);

    private static bool ClearChatRequested;
    public static void ClearChat() => ClearChatRequested = true;

    private ChatContainer _chatContainer;
    private TextInput _chatBox;

    private string _recentTeller = string.Empty;
    
    public ChatView() {
        Width = Settings.DefaultScreenWidth - 256;
        Height = Settings.DefaultScreenHeight;
            
        _chatBox = new TextInput(new InputConfig {
            FontSize = 18, 
            Bold = true, 
            OutlineThickness = 3, 
            ClickToActivate = false,
            Width = Width,
        });
        AddChild(_chatBox);
        
        _chatContainer = new ChatContainer(new ContainerConfig() {
            EnableClip = true,
            Width = Width,
            Height = Height - _chatBox.Height,
        });
        AddChild(_chatContainer);
        
        _chatBox.Y = _chatContainer.Height;
        _chatBox.Visible = false;
        
        InputHandler.OnChatKey.Add(OnChatKey);
        InputHandler.OnTellKey.Add(OnTellKey);
        InputHandler.OnGuildChatKey.Add(OnGuildKey);
        InputHandler.OnPartyChatKey.Add(OnPartyKey);
        InputHandler.OnChatHistoryUp.Add(_chatContainer.PageUp);
        InputHandler.OnChatHistoryDown.Add(_chatContainer.PageDown);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (ClearChatRequested) {
            _chatContainer.Clear();
            ClearChatRequested = false;
        }
        
        while (ChatLineQueue.TryDequeue(out var chatLineData)) {
            _chatContainer.AddChatLine((int)gameTime.TotalGameTime.TotalMilliseconds, chatLineData);
        }
    }

    private void OnChatKey(bool active) {
        if (active) {
            OpenTextInput(string.Empty);
        } else {
            var text = PlayerText.CreatePacket();
            text.Text = _chatBox.Text;
            Client.QueuePacket(text);
            
            ChatLayer.QueueSpeech(new SpeechData { Owner = Map.LocalPlayer, Text = _chatBox.Text});
            
            _chatBox.Clear();
            _chatBox.Visible = false;
            IsTyping = false;
        }
    }

    private void OnTellKey() {
        OpenTextInput(!string.IsNullOrEmpty(_recentTeller) 
            ? $"/tell {_recentTeller} " 
            : $"/tell "
        );
    }

    private void OnGuildKey() {
        OpenTextInput("/g ");
    }
    
    private void OnPartyKey() {
        OpenTextInput("/p ");
    }

    private void OpenTextInput(string defaultText) {
        _chatBox.SetActive();
        _chatBox.Visible = true;
        IsTyping = true;
        
        if (!string.IsNullOrEmpty(defaultText)) {
            _chatBox.ClearText();
            _chatBox.AddText(defaultText);
        }
    }
    
}