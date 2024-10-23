using Microsoft.Xna.Framework;
using MonoClient.Networking;
using MonoClient.Rendering;
using MonoClient.Screens.Game.Components;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.Utils;

namespace MonoClient.Screens.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    private double _lastLogTime;
    private int _frames;

    public GameScreen() {
        SetBaseDimensions(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight);
        Main.GameInstance.SetInGameGraphics();
        Client.Connect(Settings.GameServerAddress, Settings.GameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
    }

    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalGameTime.TotalMilliseconds;
        var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
        
        if (Main.GameInstance.IsActive) {
            InputHandler.Update(time, dt);
        }
        
        Map.Update(time, dt);
        Client.Tick();
        GameSprite.Update();
        
        if (time - _lastLogTime > 1000) {
            _lastLogTime = time;
            Logger.Info($"FPS: {_frames} | Tiles: {Render.LastDrawCountTiles} | Shadows: {Render.LastDrawCountShadows} | DrawnEntities: {Render.LastDrawCountEntities} | DrawnUiElements: {UiRender.LastRenderCount} | DrawnParticles: {Render.LastDrawParticleCount}");
            _frames = 0;
        }
        
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
        _frames++;
        Map.Draw(gameTime);
        Minimap.Instance.PreDrawUpdate();
        base.Draw(gameTime);
    }
}