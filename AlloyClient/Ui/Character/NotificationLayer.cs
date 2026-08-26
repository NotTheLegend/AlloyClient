using System.Collections.Generic;
using AlloyClient.Game.Objects;
using Alloy.UiLib.Core;
using Alloy.Engine;
using AlloyClient.Game;

namespace AlloyClient.Ui.Character;

public record struct StatusData(Entity Owner, string Text, uint Color, int Lifetime, int OffsetTime);

public class NotificationLayer : Sprite {
    private readonly static Queue<StatusData> TextQueue = new();
    private readonly List<CharacterStatusText> _list = [];
    
    public static void AddStatusText(Entity en, string text, uint color, int lifetime, int offsetTime) {
        var data = new StatusData(en, text, color, lifetime, offsetTime);
        TextQueue.Enqueue(data);
    }

    public void Update(in GameTime gameTime, in Camera camera) {
        while (TextQueue.TryDequeue(out var data)) {
            var child = new CharacterStatusText(data.Owner, data.Text, data.Color, data.Lifetime, data.OffsetTime + gameTime.TotalMs);
            AddChild(child);
            _list.Add(child);
        }

        for (var i = _list.Count - 1; i >= 0; i--) {
            var status = _list[i];
            if (!status.Update(in gameTime, in camera)) {
                _list.RemoveAt(i);
                RemoveChild(status);
            }
        }
    }
}