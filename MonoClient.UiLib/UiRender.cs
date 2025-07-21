using System;
using System.Drawing;
using Common;
using Common.ContentReaders;
using Common.Vector;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Data;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using OpenTK.Mathematics;

namespace MonoClient.UiLib;

public class UiSettings {

    public required Game Game;

    public required IntVector2 MinimumScreen;

    public required IntVector2 DefaultScreen;
}

public static class UiRender {

    internal static Stage Stage;

    internal static IntVector2 MinimumScreen;
    
    internal static IntVector2 DefaultScreen;
    
    internal static IntVector2 Screen;
    
    public static Vector2 ScreenScale;//Todo: move to stage?
    
    public static int LastRenderCount = 0;

    internal static Game Game;

    //internal static ContentManager Content;

    internal static GraphicsDevice Graphics;

    public static BitmapFamily MyriadPro;
    
    public static Matrix4 ViewMatrix = new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.5f, 0f, -1f, 1f, 0.5f, 1f);

    internal static Effect UiShader;

    public static void ConfigureAndLoad(UiSettings settings, out Stage stage) {
        if (Stage != null) {
            stage = Stage;
            return;
        }
        
        Game = settings.Game;
        Graphics = settings.Game.GraphicsDevice;
        MinimumScreen = settings.MinimumScreen;
        DefaultScreen = settings.DefaultScreen;
        Stage = stage = new Stage();
        
        KeyboardInput.Register(Game, Stage);
        MouseInput.Register(Game);

        Game.Window.ClientSizeChanged += OnResize;

        UiShader = settings.Game.Content.Load<Effect>("shaders/ShaderUi");

        Sprite.BuildBuffers(Graphics);
        
        OnResize(null, null);
    }

    public static void RegisterTexture(TextureType textureId, Texture texture) {
        switch (textureId) {
            case TextureType.GameAtlas:
                UiShader.Parameters["GameAtlasTexture"].SetValue(texture);
                break;
            case TextureType.UiAtlas:
                UiShader.Parameters["UiAtlasTexture"].SetValue(texture);
                break;
            case TextureType.TitleBackground:
                UiShader.Parameters["TitleBackgroundTexture"].SetValue(texture);
                break;
            case TextureType.TitleGraphic:
                UiShader.Parameters["TitleGraphicTexture"].SetValue(texture);
                break;
            case TextureType.Minimap:
                UiShader.Parameters["MinimapTexture"].SetValue(texture);
                break;
            case TextureType.Text:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Text is handled through 'RegisterFont'");
            default:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Type has no texture association");
        }
    }

    public static void RegisterFont(FontFamily font) {
        MyriadPro = new BitmapFamily(font);
        
        UiShader.Parameters["PixelRange"].SetValue(MyriadPro.PixelRange);
        UiShader.Parameters["TextTextureSize"].SetValue(new Vector2(MyriadPro.Atlas.Width, MyriadPro.Atlas.Height));
        UiShader.Parameters["TextTexture"].SetValue(MyriadPro.Atlas);
    }

    private static void OnResize(object _, EventArgs __) {
        var rect = Game.Window.ClientBounds;
        var screen = new IntVector2(rect.Width, rect.Height);

        if (screen == Screen)
            return;

        Screen = IntVector2.Max(screen, MinimumScreen);
        
        if (screen.X < MinimumScreen.X || screen.Y < MinimumScreen.Y)
            Game.Window.Position = new Point(rect.X, rect.Y);
        
        var ratio = MathF.Min((float)Screen.X / DefaultScreen.X, (float)Screen.Y / DefaultScreen.Y);
        
        //ScreenScale = new Vector2((float) Screen.X / DefaultScreen.X, (float) Screen.Y / DefaultScreen.Y);
        ScreenScale = new Vector2(ratio, ratio);

        Stage.StageWidth = Screen.X;
        Stage.StageHeight = Screen.Y;

        ViewMatrix.M11 = 2.0f / Screen.X;
        ViewMatrix.M22 = 2.0f / -Screen.Y;

        UiShader.Parameters["ViewMatrix"].SetValue(ViewMatrix);
        
        Stage.DispatchEvent(new ResizeEvent(ResizeEvent.Resize, rect.X, rect.Y, Screen.X, Screen.Y));
    }

    public static BitmapFont GetFont(FontType type) {
        return MyriadPro.Fonts[type];
    }
}