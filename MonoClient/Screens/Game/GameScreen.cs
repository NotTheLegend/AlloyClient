using System;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Rendering;
using MonoClient.Screens.Game.Components;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Input;
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


        MouseEnabled = true;
        //AddEventListener(MouseEvent.LeftClick, test);
    }

    private void test() {
        //Console.WriteLine(Matrix.Invert(Camera.WorldMatrix * Camera.ViewMatrix * Camera.ProjectionMatrix));
        //Console.WriteLine(Matrix.Invert(Camera.WorldMatrix) * Matrix.Invert(Camera.ViewMatrix) * Matrix.Invert(Camera.ProjectionMatrix));

        const int HudOffset = 240;

        var pos = new IntVector2();//MouseInput.GetMousePosition();
        
        var visTiles = new Vector2((Settings.ScreenWidth - HudOffset) / Settings.CameraZoom, Settings.ScreenHeight / Settings.CameraZoom);
        
        var s = MathF.Sin(-Settings.CameraAngle);
        var c = MathF.Cos(-Settings.CameraAngle);
        
        var visX = ((Settings.ScreenWidth - HudOffset) / Settings.CameraZoom) * c - (Settings.ScreenHeight / Settings.CameraZoom) * s;
        var visY = ((Settings.ScreenWidth - HudOffset) / Settings.CameraZoom) * s + (Settings.ScreenHeight / Settings.CameraZoom) * c;

        var x = MathUtils.Map(pos.X, 0f, Settings.ScreenWidth - HudOffset, -visX, visX);
        var y = MathUtils.Map(pos.Y, 0f, Settings.ScreenHeight, -visY, visY);
        
        var x1 = x * c - y * s + Camera.Position.X;

        
        var pos1 = new Vector2(pos.X - 240, pos.Y);

        //var t1 = Vector2.Transform(pos1, UiRender.ViewMatrix);
        var t = Vector2.Transform(pos1, UiRender.ViewMatrix * Matrix.Invert(Camera.WorldMatrix * Camera.ViewMatrix * Camera.ProjectionMatrix));

        Console.WriteLine($"{x1}, {Map.LocalPlayer.Position}");
    }

    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalGameTime.TotalMilliseconds;
        var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
        
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
        
        //base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
        _frames++;
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}