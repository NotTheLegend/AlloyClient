using AlloyClient.State;
using AlloyClient.Ui.Components.Panels;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.Utils;

namespace AlloyClient.Display;

public sealed class OverlayManager : Sprite {
    
    private static readonly ColorRect Overlay = new (new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });

    private static OverlayManager Instance;

    private Sprite _current;

    public OverlayManager() {
        Instance = this;
        AddEventListener(Event.AddedToStage, OnStageEnter);
        AddEventListener(Event.RemovedFromStage, OnStageExit);
    }

    public static void Set(Overlay sprite) => Instance.PrivateSet(sprite);
    
    public static void Clear() => Instance.PrivateClear();

    private void PrivateSet(Overlay sprite) {
        if (_current is null) {
            AddChild(Overlay);
            Overlay.AddAlphaTween(0f, 0.8f, 250);
            
            AddChild(_current = sprite);
            _current.AddAlphaTween(0f, 1f, 250);
        } else {
            _current.AddAlphaTween(1f, 0f, 150, onFinish: () => {
                RemoveChild(_current);
                AddChild(_current = sprite);
                _current.AddAlphaTween(0f, 1f, 150);
            });
        }
    }

    private void PrivateClear() {
        Overlay.AddAlphaTween(0.8f, 0f, 250, onFinish: () => { RemoveChild(Overlay);});
        _current.AddAlphaTween(1f, 0f, 175, onFinish: () => { RemoveChild(_current); _current = null; });
    }

    private void OnStageEnter() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnResize(new ResizeEvent("", Stage.StageWidth, Stage.StageHeight));
    }

    private void OnStageExit() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
    }

    private void OnResize(ResizeEvent args) {
        if (_current is not null) {
            _current.X = Stage.StageWidth / 2;
            _current.Y = Stage.StageHeight / 2;
            _current.Scale = Stage.ScreenScale;
        }

        Overlay.Resize(args.Width, args.Height);
    }
    
}