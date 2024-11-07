using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Chat;

public class ChatLayer : Sprite {

    private static readonly Queue<SpeechData> Queue = new();

    public static void QueueSpeech(SpeechData data) => Queue.Enqueue(data);


    protected override void OnUpdate(GameTime gameTime) {
        while (Queue.TryDequeue(out var data)) {
            AddChild(new SpeechBubble(data, this));
        }
    }
}