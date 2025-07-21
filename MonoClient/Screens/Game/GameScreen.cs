using Common;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Rendering;
using MonoClient.Screens.Game.Components;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.UiLib;
using MonoClient.Utils;

namespace MonoClient.Screens.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    private double _lastLogTime;
    private int _frames;

    public GameScreen() {
        SetBaseDimensions(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight);
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
    }

    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalMs;
        var dt = gameTime.ElapsedMs;
        
        if (Main.GameInstance.IsActive) {
            InputHandler.Update(time, dt);
        }
        
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