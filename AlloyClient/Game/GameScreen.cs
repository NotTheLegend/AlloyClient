using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Networking;
using AlloyClient.State;
using Common;

namespace AlloyClient.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    public static int Frames;

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
        Frames++;
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}