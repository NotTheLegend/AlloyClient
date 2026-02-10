using System;
using System.Collections.Generic;
using AlloyClient.UiLib.BuiltIn;

namespace AlloyClient.Game.Components.Hud.OldChat;

// TODO:
// Chat scrolling

public class ChatContainer : Container {
    private const int MaxChatHistory = 40;
    private const int MaxChatShown = 8;
    
    private readonly LinkedList<ChatLine> _chatLines = [];
    
    private int _chatLineIndex = 0;
    private int MaxChatLineIndex => Math.Min(MaxChatHistory, _chatLines.Count) - MaxChatShown;

    public ChatContainer(ContainerConfig config) : base(config) { }

    public void PageUp() {
        if (_chatLineIndex >= MaxChatLineIndex)
            return;
        
        _chatLineIndex = Math.Min(_chatLineIndex + MaxChatShown, MaxChatLineIndex);
        RefreshChatOrder();
    }

    public void PageDown() {
        if (_chatLineIndex == 0)
            return;
        
        _chatLineIndex = Math.Max(_chatLineIndex - MaxChatShown, 0);
        RefreshChatOrder();
    }
    
    public void ResetScroll() {
        _chatLineIndex = 0;
        RefreshChatOrder();
    }
    
    public void Clear() {
        RemoveChildren();
        _chatLines.Clear();
        _chatLineIndex = 0;
    }

    public void AddChatLine(int time, ChatLineData data) {
        var line = new ChatLine(time, data);
        _chatLines.AddFirst(line);
        AddChild(line.Sprite);
        
        // Prevent chat moving if we're not at the bottom of the chat
        if (_chatLineIndex > 0)
            _chatLineIndex++; 
        
        UpdateChatHistory();
        RefreshChatOrder();
    }

    private void RefreshChatOrder() {
        var yOffset = 10;

        var index = 0;
        foreach (var line in _chatLines) {
            var sprite = line.Sprite;
            
            // Ensure only lines on the "page" get shown
            if (index < _chatLineIndex || index >= _chatLineIndex + MaxChatShown) {
                sprite.Visible = false;
            } else {
                sprite.Y = Height - yOffset;
                yOffset += sprite.Height + 10;
                sprite.Visible = true;
            }
            index++;
        }
    }

    private void UpdateChatHistory() {
        if (_chatLines.Count <= MaxChatHistory) 
            return;
        
        var line = _chatLines.Last?.Value;
        if (line is null) 
            return;
        
        RemoveChild(line.Sprite);
        _chatLines.RemoveLast();
    }

}