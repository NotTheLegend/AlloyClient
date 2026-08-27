using System;
using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using Alloy.UiLib.Rendering;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib;

public class UiSettings {

    public required Vector2i DefaultScreen;

    public required Vector2i Screen;
}

public static partial class UiRender {

    internal static ILoggerFactory LogFactory;

    internal static bool IsFocused = true;

    internal static Stage Stage;
    
    internal static Vector2i DefaultScreen;
    
    internal static Vector2i Screen;
    
    public static int LastRenderCount = 0;

    public static BitmapFamily MyriadPro;
    
    public static Matrix4 ViewMatrix = new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.5f, 0f, -1f, 1f, 0.5f, 1f);
    
    [Shader("Ui")] private static partial ShaderSource UiShaderSource { get; }

    internal static Shader UiShader;

    public static void ConfigureAndLoad(ILoggerFactory logFactory, UiSettings settings, out Stage stage) {
        if (Stage != null) {
            stage = Stage;
            return;
        }

        LogFactory = logFactory;
        DefaultScreen = settings.DefaultScreen;
        Stage = stage = new Stage();
        
        Toolkit.Event.EventRaised += HandleEvents;

        UiShader = Shader.FromSource(UiShaderSource);
        
        SpriteRender.Init();
        
        OnResize(settings.Screen);
    }

    public static void RegisterTexture(TextureType textureId, Sampler sampler) {
        switch (textureId) {
            case TextureType.GameAtlas:
                UiShader.SetValue("GameAtlasTexture", sampler);
                break;
            case TextureType.UiAtlas:
                UiShader.SetValue("UiAtlasTexture", sampler);
                break;
            case TextureType.UiAtlasLinear:
                UiShader.SetValue("UiAtlasTextureLinear", sampler);
                break;
            case TextureType.TitleBackground:
                UiShader.SetValue("TitleBackgroundTexture", sampler);
                break;
            case TextureType.TitleGraphic:
                UiShader.SetValue("TitleGraphicTexture", sampler);
                break;
            case TextureType.Minimap:
                UiShader.SetValue("MinimapTexture", sampler);
                break;
            case TextureType.Text:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Text is handled through 'RegisterFont'");
            default:
                throw new ArgumentOutOfRangeException(nameof(textureId), textureId, "Type has no texture association");
        }
    }

    public static void RegisterFont(BitmapFamily font) {
        MyriadPro = font;
        
        UiShader.SetValue("PixelRange", MyriadPro.PixelRange);
        UiShader.SetValue("TextTextureSize", new Vector2(MyriadPro.Atlas.Width, MyriadPro.Atlas.Height));
        UiShader.SetValue("TextTexture", MyriadPro.Sampler);
    }

    private static void OnResize(Vector2i screen) {
        if (screen == Screen)
            return;

        Screen = screen;
        
        var ratio = MathF.Min((float)Screen.X / DefaultScreen.X, (float)Screen.Y / DefaultScreen.Y);
        
        Stage.SetSize(screen, new Vector2(ratio, ratio));

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
    
    private static void HandleEvents(EventArgs args) {
        if (args is FocusEventArgs fea) {
            SetFocus(fea.GotFocus);
            if (!fea.GotFocus)
                Stage.ResetMouseState();
        }
        if (!IsFocused) return;
        
        switch (args) {
            case KeyDownEventArgs e:
                Stage.SetKeyDown(e.Key, e.Scancode);
                break;
            case KeyUpEventArgs e:
                Stage.SetKeyUp(e.Key, e.Scancode);
                break;
            case TextInputEventArgs e:
                TextInput.ActiveInput?.OnTextInput(e.Text.AsSpan());
                break;
            case WindowResizeEventArgs e:
                OnResize(e.NewClientSize);
                break;
            case MouseButtonDownEventArgs e:
                Stage.SetMouseButtonDown(e.Button);
                break;
            case MouseButtonUpEventArgs e:
                Stage.SetMouseButtonUp(e.Button);
                break;
            case MouseMoveEventArgs e:
                Stage.SetMousePosition(e.ClientPosition);
                break;
            case ScrollEventArgs e:
                Stage.SetMouseScroll(e.Delta);
                break;
        }
    }
}