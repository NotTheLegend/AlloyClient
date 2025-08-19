using Common;
using RealmClient.Display;
using RealmClient.Game.Components;
using RealmClient.Networking;
using RealmClient.Rendering;
using RealmClient.State;
using RealmClient.UiLib;
using RealmClient.UiLib.Core;
using RealmClient.Utils;

namespace RealmClient.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    private double _lastLogTime;
    private int _frames;

    public GameScreen() {
        SetBaseDimensions(Settings.ScreenWidth, Settings.ScreenHeight);
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
        
        SetAutoResize(OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        SetBaseDimensions(args.Width, args.Height);
    }

    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalMs;
        var dt = gameTime.ElapsedMs;
        
        Map.Update(time, dt);
        PartyData.Update(time);
        Client.Tick();
        GameSprite.Update();
        
        if (time - _lastLogTime > 1000) {
            _lastLogTime = time;
            Logger.Info($"\n[Frame Stats]\n" +
                        $"FPS: {_frames}\n" +
                        $"Tiles: {Render.LastDrawCountTiles}\n" +
                        $"Shadows: {Render.LastDrawCountShadows}\n" +
                        $"DrawnEntities: {Render.LastDrawCountEntities}\n" +
                        $"DrawnUiElements: {UiRender.LastRenderCount}\n" +
                        $"DrawnParticles: {Render.LastDrawParticleCount}");
            _frames = 0;
        }
    }

    public override void Draw(GameTime gameTime) {
        _frames++;
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}