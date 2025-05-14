using System;
using System.Runtime.InteropServices;
using Common.Pipeline;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.Assets;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Extra;
using MonoClient.UiLib.Input;
using MonoClient.UiLib.Utils;

namespace MonoClient.UiLib;

public static class UiRender {

    internal static Stage Stage;

    internal static IntVector2 DefaultScreen;
    internal static IntVector2 Screen;
    public static Vector2 ScreenScale;//Todo: move to stage?

    public static readonly IntVector2 MinimumScreen = new (800, 600);
    
    public static int LastRenderCount = 0;

    public static Game Game;

    public static ContentManager Content;

    public static GraphicsDevice Graphics;

    public static MainAtlas GameAtlas;

    public static UiAtlas UiAtlas;

    public static Texture2D Minimap;

    public static BitmapFamily MyriadPro;
    
    public static Matrix ViewMatrix = new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.5f, 0f, -1f, 1f, 0.5f, 1f);

    internal static Effect UiShader;

    public static void ConfigureAndLoad(Game game, MainAtlas gameAtlas, UiAtlas uiAtlas, Texture2D minimap, IntVector2 defaultScreen, out Stage stage) {
        if (Game != null) {
            stage = Stage;
            return;
        }
        
        Game = game;
        Content = game.Content;
        Graphics = game.GraphicsDevice;
        GameAtlas = gameAtlas;
        UiAtlas = uiAtlas;
        Minimap = minimap;
        DefaultScreen = defaultScreen;
        Stage = stage = new Stage();
        
        KeyboardInput.Register(game, Stage);
        MouseInput.Register(game);

        Game.Window.ClientSizeChanged += OnResize;

        MyriadPro = new BitmapFamily("Fonts/MyriadPro/MyriadPro");

        UiShader = Content.Load<Effect>("shaders/ShaderUi");

        UiShader.Parameters["GameAtlasTexture"].SetValue(gameAtlas.Texture);
        UiShader.Parameters["UiAtlasTexture"].SetValue(uiAtlas.Texture);
        UiShader.Parameters["MinimapTexture"].SetValue(Minimap);

        UiShader.Parameters["PixelRange"].SetValue(MyriadPro.PixelRange);
        UiShader.Parameters["TextTextureSize"].SetValue(new Vector2(MyriadPro.Atlas.Width, MyriadPro.Atlas.Height));
        UiShader.Parameters["TextTexture"].SetValue(MyriadPro.Atlas);

        UiShader.Parameters["TitleBackgroundTexture"].SetValue(Content.Load<Texture2D>("Ui/titleView/TitleScreenBackground"));
        UiShader.Parameters["TitleGraphicTexture"].SetValue(Content.Load<Texture2D>("Ui/titleView/TitleScreenGraphic"));

        Sprite.BuildBuffers(Graphics);
        
        UpdateViewMatrix(defaultScreen.X, DefaultScreen.Y);
    }

    private static void OnResize(object _, EventArgs __) {
        var rect = Game.Window.ClientBounds;
        var screen = new IntVector2(rect.Width, rect.Height);

        if (screen == Screen)
            return;

        screen = IntVector2.Max(screen, MinimumScreen);
        Screen = screen;

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

    public static void SetStartingResolution(int width, int height) {
        Stage.StageWidth = width;
        Stage.StageHeight = height;
        UpdateViewMatrix(width, height);
    }

    public static void UpdateViewMatrix(int width, int height) {
        Screen = new IntVector2(width, height);
        ScreenScale = new Vector2((float)Screen.X / DefaultScreen.X, (float)Screen.Y / DefaultScreen.Y);
        ViewMatrix = Matrix.CreateOrthographicOffCenter(0, Screen.X, Screen.Y, 0, -1, 1);
        UiShader.Parameters["ViewMatrix"].SetValue(ViewMatrix);

    }

    public static BitmapFont GetFont(FontType type) {
        return MyriadPro.Fonts[type];
    }
}

public struct VertexUi {
    
    public Vector2 Position;
    public Vector2 UV;
    public Color Color;

    public VertexUi(Vector2 pos, Vector2 uv, Color color) {
        Position = pos;
        UV = uv;
        Color = color;
    }
    
    public VertexUi(Vector2 pos, Vector2 uv) {
        Position = pos;
        UV = uv;
        Color = new Color(0);
    }
    
    public VertexUi(Vector2 pos, Color color) {
        Position = pos;
        UV = new Vector2(0f);
        Color = color;
    }

    public VertexUi(Vector2 pos) {
        Position = pos;
        UV = new Vector2(0f);
        Color = new Color(0);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VertexDataUi : IVertexType {
    public Vector2 Position;
    public Color Color;
    public Color ColorOverride;
    public Vector2 Info;
    public Vector2 UVCoords;
    public Vector4 Scissor;
    public Vector4 Extra1;
    public Vector4 Extra2;
    public Vector4 ColorTransform;

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
        new VertexElement(80, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5));

    public VertexDataUi(Vector2 position, Color color, Color colorOverride, Vector2 info, Vector2 uvCoords, Vector4 scissor, Vector4 extra1, Vector4 extra2, ColorTransform colorTransform) {
        Position = position;
        Color = color;
        ColorOverride = colorOverride;
        Info = info;
        UVCoords = uvCoords;
        Scissor = scissor;
        Extra1 = extra1;
        Extra2 = extra2;
        ColorTransform = colorTransform.GetTransformData();
    }

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public override int GetHashCode() {
        return (((((((Position.GetHashCode() 
                                        * 397 ^ Color.GetHashCode())
                                    * 397 ^ ColorOverride.GetHashCode())
                                * 397 ^ Info.GetHashCode())
                            * 397 ^ UVCoords.GetHashCode())
                        * 397 ^ Scissor.GetHashCode())
                    * 397 ^ Extra1.GetHashCode())
                * 397 ^ Extra2.GetHashCode())
            * 397 ^ ColorTransform.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Position
                             + " Color: " + Color
                             + " Override: " + ColorOverride
                             + " Info: " + Info
                             + " UVCoords:" + UVCoords
                             + " Scissor:" + Scissor
                             + " E1:" + Extra1
                             + " E2:" + Extra2
                             + " CT:" + ColorTransform + "}}";
    }

    public static bool operator ==(VertexDataUi left, VertexDataUi right) {
        return left.Position == right.Position
               && left.Color == right.Color
               && left.ColorOverride == right.ColorOverride
               && left.Info == right.Info
               && left.UVCoords == right.UVCoords
               && left.Scissor == right.Scissor
               && left.Extra1 == right.Extra1
               && left.Extra2 == right.Extra2
               && left.ColorTransform == right.ColorTransform;
    }

    public static bool operator !=(VertexDataUi left, VertexDataUi right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexDataUi)obj;
    }
}