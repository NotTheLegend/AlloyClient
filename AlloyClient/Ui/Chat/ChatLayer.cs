using System.Collections.Generic;
using Alloy.UiLib.Core;
using Alloy.Engine;
using AlloyClient.Game;

namespace AlloyClient.Ui.Chat;

public class ChatLayer : Sprite {

    private readonly static Queue<SpeechData> Queue = new();

    private readonly static Dictionary<int, SpeechBubble> Bubbles = [];

    public static void QueueSpeech(SpeechData data) => Queue.Enqueue(data);

    public void Update(in GameTime gameTime, in Camera camera) {
        while (Queue.TryDequeue(out var data)) {
            if (Bubbles.Remove(data.Owner.ObjectId, out var bubble)) {
                RemoveChild(bubble);
            }

            var sprite = new SpeechBubble(data, gameTime.TotalMs);
            Bubbles[data.Owner.ObjectId] = sprite;
            AddChild(sprite);
        }

        foreach (var (key, bubble) in Bubbles) {
            if (!bubble.Update(in gameTime, in camera)) {
                Bubbles.Remove(key);
                RemoveChild(bubble);
            }
        }
    }
}