using System;
using System.Collections.Generic;
using System.Linq;
using MonoClient.Data;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Incoming;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud.Chat;

// TODO:
// Chat scrolling

public class ChatContainer : Container {
    private const int MaxChatHistory = 4;
    private const int MaxChatShown = 7;

    private readonly LinkedList<ChatLine> _chatLines = [];

    public ChatContainer(ContainerConfig config) : base(config) {
        
    }

    public void ScrollUp() {
        // TODO
    }

    public void ScrollDown() {
        // TODO
    }
    
    public void Clear() {
        RemoveAllChildren();
        _chatLines.Clear();
    }

    public void AddChatLine(int time, ChatLineData data) {
        _chatLines.AddFirst(new ChatLine(time, data));
        UpdateChatHistory();
        RefreshChatOrder();
    }

    private void RefreshChatOrder() {
        var yOffset = 0;
        
        foreach (var line in _chatLines) {
            var sprite = line.GetSprite();
            sprite.Y = Height - yOffset;
            AddChild(sprite);
            
            yOffset += sprite.Height;
        }
    }

    private void UpdateChatHistory() {
        if (_chatLines.Count <= MaxChatHistory) 
            return;
        
        var line = _chatLines.Last?.Value;
        if (line is null) 
            return;
        
        RemoveChild(line.GetSprite());
        _chatLines.RemoveLast();
    }

}