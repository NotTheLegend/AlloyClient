using System.Collections.Generic;
using System.Linq;
using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Networking;
using AlloyClient.State;
using Common;

namespace AlloyClient.Game;

public sealed class GameScreen : Screen {
    private const int WINDOW_DURATION = 30000;
    
    public readonly GameSprite GameSprite;

    public static double FramesSeconds;
    public static double AvgFrameTime;
    public static double P90FrameTime;
    public static double P99FrameTime;
    public static double MaxFrameTime;
    public static double FPS;

    private static readonly Queue<double> _frameTimes = new ();
    private static double _statisticsTimer;
    private static int _frameCount;

    public GameScreen() {
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
    }

    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalMs;
        var dt = gameTime.ElapsedMs;
        
        Map.Update(time, dt);
        PartyData.Update(time);
        Client.Tick();
    }

    public override void Draw(GameTime gameTime) {
        var elapsed = gameTime.ElapsedMs;
        _frameTimes.Enqueue(elapsed);
        FramesSeconds += elapsed;
        _statisticsTimer += elapsed;

        // Drop frames that fall outside the 30s window
        while (FramesSeconds > WINDOW_DURATION) {
            FramesSeconds -= _frameTimes.Dequeue();
        }

        _frameCount++;
        if (_statisticsTimer >= 1000) { // Calculate statistics every second
            FPS = _frameCount / 1.0;
            var count = _frameTimes.Count;
            var sorted = _frameTimes.OrderBy(f => f).ToArray();
            AvgFrameTime = _frameTimes.Sum() / count;
            P90FrameTime = sorted[(int)(count * 0.90f)];
            P99FrameTime = sorted[(int)(count * 0.99f)];
            MaxFrameTime = sorted[count - 1];
            _statisticsTimer = 0;
            _frameCount = 0;
        }

        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}