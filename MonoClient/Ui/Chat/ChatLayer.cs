using System.Collections.Generic;
using Common;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Chat;

public class ChatLayer : Sprite {

    private static readonly Queue<SpeechData> Queue = new();

    private static readonly Dictionary<int, SpeechBubble> Bubbles = [];

    public static void QueueSpeech(SpeechData data) => Queue.Enqueue(data);

    protected override void OnUpdate(GameTime gameTime) {
        while (Queue.TryDequeue(out var data)) {
            if (Bubbles.TryGetValue(data.Owner.ObjectId, out var bubble))
                RemoveChild(bubble);

            var sprite = new SpeechBubble(data);
            Bubbles[data.Owner.ObjectId] = sprite;
            AddChild(sprite);
        }
    }
}