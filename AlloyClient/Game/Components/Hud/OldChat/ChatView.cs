using System.Collections.Generic;
using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Signals;

namespace AlloyClient.Game.Components.Hud.OldChat;

// TODO:
// Scroll up/down chat history

public class ChatView : Sprite {
    
    private static readonly Signal OnChatHistoryUp = new();
    private static readonly Signal OnChatHistoryDown = new();
    
    public static int MaxWidth { get; private set; } = Settings.DefaultScreenWidth / 2;

    private static readonly Queue<ChatLineData> ChatLineQueue = new();
    public static void QueueChatLine(ChatLineData data) => ChatLineQueue.Enqueue(data);

    private static bool ClearChatRequested;
    public static void ClearChat() => ClearChatRequested = true;

    private ChatContainer _chatContainer;

    public ChatView() {
        SetAnchor(UiAnchor.LeftBottom);
        //Width = MaxWidth;
        //Height = Settings.DefaultScreenHeight;

        

        _chatContainer = new ChatContainer(new ContainerConfig {
            EnableClip = true,
            Width = MaxWidth,
            Height = 400,
        });
        AddChild(_chatContainer);
        

        AddEventListener(Event.AddedToStage, AddHandlers);
        AddEventListener(Event.RemovedFromStage, RemoveHandlers);
    }

    private void AddHandlers() {
        OnChatHistoryUp.Add(_chatContainer.PageUp);
        OnChatHistoryDown.Add(_chatContainer.PageDown);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void RemoveHandlers() {
        OnChatHistoryUp.Remove(_chatContainer.PageUp);
        OnChatHistoryDown.Remove(_chatContainer.PageDown);
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnFrameEnter() {
        if (ClearChatRequested) {
            _chatContainer.Clear();
            ClearChatRequested = false;
        }

        while (ChatLineQueue.TryDequeue(out var chatLineData)) {
            _chatContainer.AddChatLine((int) Stage.GameTime.TotalMs, chatLineData);
        }
    }
}