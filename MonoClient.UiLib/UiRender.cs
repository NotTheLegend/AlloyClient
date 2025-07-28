using System;
using System.Drawing;
using Common;
using Common.ContentReaders;
using Common.Rendering;
using Common.Vector;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Data;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace MonoClient.UiLib;

public class UiSettings {

    public required Vector2i DefaultScreen;

    public required Vector2i Screen;
}

public static class UiRender {

    internal static bool IsFocused;

    internal static Stage Stage;
    
    internal static Vector2i DefaultScreen;
    
    internal static Vector2i Screen;
    
    public static Vector2 ScreenScale;//Todo: move to stage?
    
    public static int LastRenderCount = 0;

    public static BitmapFamily MyriadPro;
    
    public static Matrix4 ViewMatrix = new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.5f, 0f, -1f, 1f, 0.5f, 1f);

    internal static Shader UiShader;

    public static void ConfigureAndLoad(UiSettings settings, out Stage stage) {
        if (Stage != null) {
            stage = Stage;
            return;
        }
        
        DefaultScreen = settings.DefaultScreen;
        Stage = stage = new Stage();
        
        KeyboardInput.Register(Stage);
        
        EventQueue.EventRaised += HandleEvents;

        UiShader = ContentReader.LoadShader("shaders/ShaderUi");

        Sprite.BuildBuffers();
        
        OnResize(settings.Screen);
    }

    public static void RegisterTexture(TextureType textureId, Texture texture) {
        UiShader.Apply();
        switch (textureId) {
            case TextureType.GameAtlas:
                UiShader.SetValue("GameAtlasTexture", texture);
                break;
            case TextureType.UiAtlas:
                UiShader.SetValue("UiAtlasTexture", texture);
                break;
            case TextureType.TitleBackground:
                UiShader.SetValue("TitleBackgroundTexture", texture);
                break;
            case TextureType.TitleGraphic:
                UiShader.SetValue("TitleGraphicTexture", texture);
                break;
            case TextureType.Minimap:
                UiShader.SetValue("MinimapTexture", texture);
                break;
            case TextureType.Text:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Text is handled through 'RegisterFont'");
            default:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Type has no texture association");
        }
    }

    public static void RegisterFont(FontFamily font) {
        MyriadPro = new BitmapFamily(font);
        
        UiShader.Apply();
        UiShader.SetValue("PixelRange", MyriadPro.PixelRange);
        UiShader.SetValue("TextTextureSize", new Vector2(MyriadPro.Atlas.Width, MyriadPro.Atlas.Height));
        UiShader.SetValue("TextTexture", MyriadPro.Atlas);
    }

    private static void OnResize(Vector2i screen) {
        if (screen == Screen)
            return;

        Screen = screen;
        
        var ratio = MathF.Min((float)Screen.X / DefaultScreen.X, (float)Screen.Y / DefaultScreen.Y);
        
        ScreenScale = new Vector2(ratio, ratio);

        Stage.StageWidth = Screen.X;
        Stage.StageHeight = Screen.Y;

        ViewMatrix.M11 = 2.0f / Screen.X;
        ViewMatrix.M22 = 2.0f / -Screen.Y;

        UiShader.Apply();
        UiShader.SetValue("ViewMatrix", ViewMatrix);
        
        Stage.DispatchEvent(new ResizeEvent(ResizeEvent.Resize, Screen.X, Screen.Y));
    }

    public static BitmapFont GetFont(FontType type) {
        return MyriadPro.Fonts[type];
    }

    private static void SetFocus(bool focus) => IsFocused = focus;
    
    private static void HandleEvents(PalHandle handle, PlatformEventType type, EventArgs args) {
        if (args is FocusEventArgs fea) {
            SetFocus(fea.GotFocus);
        }
        if (!IsFocused) return;
        
        switch (args) {
            case KeyDownEventArgs e:
                if (e.IsRepeat) break;
                KeyboardInput.SetKeyDown(e);
                break;
            case KeyUpEventArgs e:
                KeyboardInput.SetKeyUp(e);
                break;
            case TextInputEventArgs e:
                KeyboardInput.OnTextInput(e);
                break;
            case WindowResizeEventArgs e:
                OnResize(e.NewClientSize);
                break;
            case MouseButtonDownEventArgs e:
                MouseInput.SetKeyDown(e.Button);
                break;
            case MouseButtonUpEventArgs e:
                MouseInput.SetKeyUp(e.Button);
                break;
            case MouseMoveEventArgs e:
                MouseInput.SetMousePosition(e.ClientPosition);
                break;
            case ScrollEventArgs e:
                MouseInput.SetScrollDelta(e.Delta);
                break;
        }
    }
}