using Common;
using RealmClient.Display;
using RealmClient.Game.Components;
using RealmClient.Networking;
using RealmClient.Rendering;
using RealmClient.State;
using RealmClient.Ui.Components.Elements;
using RealmClient.UiLib;
using RealmClient.UiLib.Core;
using RealmClient.Utils;

namespace RealmClient.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    public static int Frames;

    public GameScreen() {
        SetBaseDimensions(Settings.ScreenWidth, Settings.ScreenHeight);
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
        
        AddChild(new DebugStats());
        
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
    }

    public override void Draw(GameTime gameTime) {
        Frames++;
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}