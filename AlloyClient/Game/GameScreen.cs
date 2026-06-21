using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Networking;
using Alloy.Engine;
using Alloy.UiLib.Core;
using AlloyClient.Game.Components.Hud;
using AlloyClient.Game.Components.Hud.Chat;
using AlloyClient.Rendering;
using AlloyClient.Ui.Character;
using AlloyClient.Ui.Chat;
using AlloyClient.Ui.Components.Elements;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public sealed class GameScreen : Screen {

    public static GameScreen GameSprite;
    
    private readonly UserInput _userInput;
    private readonly ChatLayer _chatLayer;
    private readonly NotificationLayer _notificationLayer;
    private readonly HudView _hud;
    private readonly ChatBox _chat;
    private readonly DebugStats _debugStats;
    

    private Camera _camera;

    public GameScreen() {
        Client.Connect(Settings.GameServerAddress, Settings.SelectedGameServerPort);
        
        AddChild(_userInput = new UserInput()); // add map as param
        AddChild(_chatLayer = new ChatLayer());
        AddChild(_notificationLayer = new NotificationLayer());
        AddChild(_hud = new HudView());
        AddChild(_chat= new ChatBox());
        AddChild(_debugStats = new DebugStats());
        
        GameSprite = this; // TODO: remove this ;-;
    }

    public void CreatePlayerDependentAssets() => _hud.CreatePlayerDependentAssets(); // TODO: remove this ;-;

    public override void Update(GameTime gameTime) {
        Client.Tick();
        
        if (Map.LocalPlayer is null) {
            return;
        }
        
        _camera = Camera.Update(Map.LocalPlayer.Position, new Vector3i(Stage.StageWidth, Stage.StageHeight, 240), Settings.CameraAngle, Settings.CameraZoom);
        _userInput.Update(gameTime, _camera);
        _chatLayer.Update(gameTime, _camera);
        _notificationLayer.Update(gameTime, _camera);
        _debugStats.Update(gameTime);
        
        
        
        Map.Update(gameTime, _camera);
        PartyData.Update(gameTime.TotalMs);
        
    }

    public override void Draw(GameTime gameTime) {
        Render.SetShaderParams(gameTime, _camera);
        Map.Draw(gameTime);
        MinimapTexture.PreDrawUpdate();
    }

    protected override void OnResize(ResizeEvent args) {
        var width = args.Width;
        var height = args.Height;
        
        _hud.X = width;
        _hud.Y = height / 2;
        _hud.Scale = Stage.ScreenScale;

        _chat.X = 0;
        _chat.Y = height;
        _chat.Scale = Stage.ScreenScale;

        _debugStats.Scale = Stage.ScreenScale;
    }
}