using System;
using System.Runtime.CompilerServices;
using Alloy.Common;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Rendering;
using Alloy.UiLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.Core;

public partial class Sprite : DisplayContainer {

    public readonly static Vector4 NoScissor = new(0, 0, 10000, 10000);

    public int X {
        get;
        set {
            field = value;
            UpdateBounds();
        }
    }

    public int Y {
        get;
        set {
            field = value;
            UpdateBounds();
        }
    }

    public int Width {
        get => (int)(ContentWidth * Scale.X);
        set => ScaleX = GetScale(ContentWidth, value);
    }

    public int Height {
        get => (int)(ContentHeight * Scale.Y);
        set => ScaleY = GetScale(ContentHeight, value);
    }

    public float ScaleX {
        get;
        set {
            field = value;
            UpdateBounds();
        }
    } = 1f;

    public float ScaleY {
        get;
        set {
            field = value;
            UpdateBounds();
        }
    } = 1f;

    public Vector2 Scale {
        get => new(ScaleX, ScaleY);
        set {
            ScaleX = value.X;
            ScaleY = value.Y;
        }
    }

    public float Alpha {
        get;
        set => field = Math.Clamp(value, 0f, 1f);
    } = 1f;

    public float Rotation = 0;

    public UiAnchor Anchor = UiAnchor.LeftTop;

    public CollisionType CollisionType = CollisionType.Square;

    public bool Visible = true;

    public bool MouseEnabled = false;

    public bool FocusEnabled = false;

    public bool PointerFocusEnabled = true;

    public bool TabEnabled = false;

    public int TabIndex = -1;

    public bool TooltipMode = false;

    public bool EnableClipRect = false;
    public bool ClipChildren;

    internal bool TweenActive = false;

    protected ushort[] Indices = [];

    protected VertexUi[] VertexData = [];

    public int OverridePrimCount = -1;

    internal Vector2i Radii;

    #region VertexData

    public TextureType TextureId = TextureType.None;

    public Color Color = Color.Transparent;

    public Color ColorSecondary = Color.Transparent;

    public ColorTransform ColorTransformation = new(1f, 1f, 1f, 1f);

    protected Vector4 Extra1 = Vector4.Zero;

    protected Vector4 Extra2 = Vector4.Zero;

    private Vector2 _info = Vector2.Zero;

    private Vector4 _scissor = NoScissor;

    #endregion VertexData

    #region Backers

    private float _anchorX;

    private float _anchorY;

    #endregion

    #region TrueValues

    private float _trueAlpha;

    private ColorTransform _trueTransform;

    private Transform2D _worldTransform = Transform2D.Identity();

    private Transform2D _inverseWorldTransform = Transform2D.Identity();

    private bool _hasInverseWorldTransform = true;

    private bool _canInteract = true;

    private float _boundsRotation = float.NaN;

    #endregion

    private void InternalUpdate() {
        ClipChildren = false;
        _scissor = NoScissor;

        (_anchorX, _anchorY) = Anchor.GetOffset(ContentWidth, ContentHeight);
        var ta = Alpha;
        var tt = ColorTransformation;
        var localX = (float)X;
        var localY = (float)Y;

        if (TooltipMode) {
            var mousePosition = Stage.Mouse.GetMousePosition();
            (_anchorX, _anchorY) =
                (mousePosition.X < UiRender.Screen.X / 2 ? UiAnchor.LeftBottom : UiAnchor.RightBottom).GetOffset(ContentWidth,
                    ContentHeight);

            var localMouse = Parent?.GlobalToLocal(mousePosition) ?? mousePosition;
            localX = localMouse.X;
            localY = localMouse.Y;
        } else if (_isDragging) {
            var mousePosition = Stage.Mouse.GetMousePosition();
            var localMouse = Parent?.GlobalToLocal(mousePosition) ?? mousePosition;
            var dragTransform = Transform2D.Create(0f, 0f, ScaleX, ScaleY, Rotation, _anchorX, _anchorY);
            var transformedOffset = dragTransform.TransformPoint(_dragOffset);
            localX = localMouse.X - transformedOffset.X;
            localY = localMouse.Y - transformedOffset.Y;
        }

        var localTransform = Transform2D.Create(localX, localY, ScaleX, ScaleY, Rotation, _anchorX, _anchorY);
        var parentInteract = true;
        if (Parent != null) {
            _worldTransform = Transform2D.Multiply(Parent._worldTransform, localTransform);
            ta *= Parent._trueAlpha;
            tt *= Parent._trueTransform;
            parentInteract = Parent._canInteract;

            if (Parent.EnableClipRect || Parent.ClipChildren) {
                _scissor = Parent._scissor;
                ClipChildren = true;
            }
        } else {
            _worldTransform = localTransform;
        }

        _hasInverseWorldTransform = _worldTransform.TryInvert(out _inverseWorldTransform);
        _canInteract = parentInteract && Visible && !TweenActive;
        _trueAlpha = ta;
        _trueTransform = tt;

        if (TooltipMode) {
            GetWorldBounds(out var minX, out var minY, out var maxX, out var maxY);
            var offsetX = minX < 0f ? -minX : maxX > UiRender.Screen.X ? UiRender.Screen.X - maxX : 0f;
            var offsetY = minY < 0f ? -minY : maxY > UiRender.Screen.Y ? UiRender.Screen.Y - maxY : 0f;
            _worldTransform.TX += offsetX;
            _worldTransform.TY += offsetY;
            _hasInverseWorldTransform = _worldTransform.TryInvert(out _inverseWorldTransform);
        }

        if (EnableClipRect) {
            GetTransformedBounds(_worldTransform, SelfContentWidth, SelfContentHeight, out var minX, out var minY, out var maxX,
                out var maxY);

            var scissor = new Vector4(minX, minY, maxX, maxY);
            if (ClipChildren) {
                _scissor.X = Math.Max(_scissor.X, scissor.X);
                _scissor.Y = Math.Max(_scissor.Y, scissor.Y);
                _scissor.Z = Math.Min(_scissor.Z, scissor.Z);
                _scissor.W = Math.Min(_scissor.W, scissor.W);
            } else {
                _scissor = scissor;
                ClipChildren = true;
            }
        }

        _info = new Vector2((float)TextureId, ta);
    }

    private void Update() {
        InternalUpdate();

        ResolvePointerTargetSelf();

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.Update();
        }
    }

    private void ResolvePointerTargetSelf() {
        if (MouseEnabled && _canInteract && Stage.PointerPositionValid && IsInBounds(Stage.Mouse.GetMousePosition())) {
            Stage.CurrentHighestSprite = this;
        }
    }

    internal void ResolvePointerTarget() {
        ResolvePointerTargetSelf();

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.ResolvePointerTarget();
        }
    }

    private readonly Event _cachedEnterFrame = new(Event.EnterFrame);

    internal void InternalUpdateLoop() {
        BroadcastEvent(_cachedEnterFrame);
        HandleFinishedTasks();

        RefreshRotationBounds();
        Update();
    }

    private void RefreshRotationBounds() {
        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.RefreshRotationBounds();
        }

        if (Math.Abs(_boundsRotation - Rotation) < 0.0001) {
            return;
        }

        _boundsRotation = Rotation;
        Parent?.UpdateBounds();
    }

    /// <summary>
    /// Sets the primary color channel. If sprite is using a texture, it multiplies the color by the sampled pixel 
    /// </summary>
    public void SetColor(uint rgb, float alpha = 1f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        Color.PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }

    /// <summary>
    /// Sets the secondary color channel, this will control outlines if supported by the set <see cref="TextureType"/>
    /// </summary>
    public void SetColorSecondary(uint rgb, float alpha = 1f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        ColorSecondary.PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }

    public void SetAnchor(UiAnchor anchor) {
        Anchor = anchor;
        UpdateBounds();
    }

    public void SetHitboxType(CollisionType collision) => CollisionType = collision;

    /// <summary>
    /// Returns the pointer position in this sprite's local coordinate space.
    /// </summary>
    public Vector2i GetRelativeMousePosition() {
        if (Stage is null) {
            return Vector2i.Zero;
        }

        var position = GlobalToLocal(Stage.Mouse.GetMousePosition());
        return new Vector2i((int)position.X, (int)position.Y);
    }

    protected Vector2i GetLocalMousePosition() {
        return GetRelativeMousePosition();
    }

    public Vector2 LocalToGlobal(Vector2 point) {
        return _worldTransform.TransformPoint(point);
    }

    public Vector2 GlobalToLocal(Vector2 point) {
        if (!_hasInverseWorldTransform) {
            return new Vector2(float.PositiveInfinity);
        }

        return _inverseWorldTransform.TransformPoint(point);
    }

    public void CapturePointer(MouseButton button = MouseButton.Button1) {
        Stage?.CapturePointer(this, button);
    }

    public void ReleasePointer(MouseButton button = MouseButton.Button1) {
        Stage?.ReleasePointer(this, button);
    }

    internal bool CanReceiveFocus() {
        return FocusEnabled && Visible && _canInteract && Stage is not null;
    }

    internal static void GetTransformedBounds(in Transform2D transform, float width, float height, out float minX, out float minY,
        out float maxX, out float maxY) {
        var topLeft = transform.TransformPoint(Vector2.Zero);
        var topRight = transform.TransformPoint(new Vector2(width, 0f));
        var bottomRight = transform.TransformPoint(new Vector2(width, height));
        var bottomLeft = transform.TransformPoint(new Vector2(0f, height));
        minX = MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomRight.X, bottomLeft.X));
        minY = MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomRight.Y, bottomLeft.Y));
        maxX = MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomRight.X, bottomLeft.X));
        maxY = MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomRight.Y, bottomLeft.Y));
    }

    private void GetWorldBounds(out float minX, out float minY, out float maxX, out float maxY) {
        GetTransformedBounds(_worldTransform, ContentWidth, ContentHeight, out minX, out minY, out maxX, out maxY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetScale(int contentWidth, int newWidth) => contentWidth == 0 ? 0f : (float)newWidth / contentWidth;
}