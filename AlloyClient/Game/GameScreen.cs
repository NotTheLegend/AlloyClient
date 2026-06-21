using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Networking;
using Alloy.Engine;
using AlloyClient.Rendering;
using AlloyClient.Ui.Components.Elements;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public sealed class GameScreen : Screen {
    
    private readonly UserInput _userInput;
    
    public readonly GameSprite GameSprite;



    private readonly DebugStats _debugStats;
    

    private Camera _camera;

    public GameScreen() {
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        
        
        
        
        AddChild(_userInput = new UserInput()); // add map as param
        
        
        AddChild(GameSprite = new GameSprite());
        
        
        
        AddChild(_debugStats = new DebugStats());
    }

    public override void Update(GameTime gameTime) {
        Client.Tick();
        
        if (Map.LocalPlayer is null) {
            return;
        }
        
        _camera = Camera.Update(Map.LocalPlayer.Position, new Vector3i(Stage.StageWidth, Stage.StageHeight, 240), Settings.CameraAngle, Settings.CameraZoom);
        _userInput.Update(gameTime, _camera);
        
        _debugStats.Update(gameTime);
        
        
        
        Map.Update(gameTime, _camera);
        PartyData.Update(gameTime.TotalMs);
        
    }

    public override void Draw(GameTime gameTime) {
        Render.SetShaderParams(gameTime, _camera);
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }
}