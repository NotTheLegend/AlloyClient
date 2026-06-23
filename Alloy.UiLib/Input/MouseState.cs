using Alloy.UiLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.Input;

public struct MouseState {
    
    private InternalMouseState _state;
    private Vector2 _scrollDelta;
    private Vector2 _mousePosition;

    public bool IsButtonDown(MouseButton button) => _state.IsSet(button.AsFlag());

    public bool IsButtonUp(MouseButton button) => !_state.IsSet(button.AsFlag());

    public Vector2 GetScrollDelta() => _scrollDelta;
    
    public float GetVerticalScrollDelta() => _scrollDelta.Y;

    public float GetHorizontalScrollDelta() => _scrollDelta.X;

    public Vector2i GetMousePosition() => new((int) _mousePosition.X, (int)_mousePosition.Y);

    internal bool SetButtonDown(MouseButton button) => _state.TrySet(button.AsFlag());

    internal bool SetButtonUp(MouseButton button) => _state.TryClear(button.AsFlag());

    internal void SetScrollDelta(Vector2 delta) => _scrollDelta = delta;
    
    internal void SetPosition(Vector2 position) => _mousePosition = position;
    
    private struct InternalMouseState {
        private MouseButtonFlags _flags;

        public bool IsSet(MouseButtonFlags buttonFlag) => (_flags & buttonFlag) != 0;

        public bool TrySet(MouseButtonFlags buttonFlag) {
            var wasSet = (_flags & buttonFlag) != 0;
            _flags |= buttonFlag;
            return !wasSet;
        }

        public bool TryClear(MouseButtonFlags buttonFlag) {
            var wasSet = (_flags & buttonFlag) != 0;
            _flags &= ~buttonFlag;
            return wasSet;
        }
    }
    
}