using System.Collections.Generic;
using Alloy.UiLib.Core;
using Alloy.Common;

namespace AlloyClient.Ui.Chat;

public class ChatLayer : Sprite {

    private static readonly Queue<SpeechData> Queue = new();

    private static readonly Dictionary<int, SpeechBubble> Bubbles = [];

    public ChatLayer() {
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    public static void QueueSpeech(SpeechData data) => Queue.Enqueue(data);

    private void OnFrameEnter() {
        while (Queue.TryDequeue(out var data)) {
            if (Bubbles.TryGetValue(data.Owner.ObjectId, out var bubble))
                RemoveChild(bubble);

            var sprite = new SpeechBubble(data);
            Bubbles[data.Owner.ObjectId] = sprite;
            AddChild(sprite);
        }
    }
}