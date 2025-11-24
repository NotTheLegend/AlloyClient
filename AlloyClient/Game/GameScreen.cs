using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Networking;
using AlloyClient.State;
using AlloyClient.Ui.Components.Elements;
using AlloyClient.UiLib.Core;
using Common;
using AlloyClient.Rendering;
using AlloyClient.UiLib;
using AlloyClient.Utils;

namespace AlloyClient.Game;

public sealed class GameScreen : Screen {
    public readonly GameSprite GameSprite;
    
    public static int Frames;

    public GameScreen() {
        //todo:SetBaseDimensions(Settings.ScreenWidth, Settings.ScreenHeight);
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        GameSprite = new GameSprite();
        AddChild(GameSprite);
        
        AddChild(new DebugStats());
        
        this.SetAutoResize(OnResize);
    }

    protected override void OnResize(ResizeEvent args) {
        //todo:SetBaseDimensions(args.Width, args.Height);
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