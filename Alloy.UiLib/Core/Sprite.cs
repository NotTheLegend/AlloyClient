using System;
using System.Runtime.CompilerServices;
using Alloy.Engine.Common;
using Alloy.Common;
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Input;
using Alloy.UiLib.Rendering;
using Alloy.UiLib.Utils;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

/* =TODO=
 * Focus
 * make resize broadcast event
 */

public partial class Sprite : DisplayContainer {
    
    public static readonly Vector4 NoScissor = new Vector4(0, 0, 10000, 10000);

    public int X { get; set { field = value; UpdateBounds(); } }

    public int Y { get; set { field = value; UpdateBounds(); } }

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
    
    public float Alpha { get; set => field = Math.Clamp(value, 0f, 1f); } = 1f;

    public float Rotation = 0;

    public UiAnchor Anchor { get; private set; } = UiAnchor.LeftTop;

    public CollisionType CollisionType { get; private set; } = CollisionType.Square;
    
    public bool Visible = true;
    
    public bool MouseEnabled = false;

    public bool TooltipMode = false;

    public bool EnableClipRect = false;
    public bool ClipChildren = false;
    
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

    private int _anchorX;
    
    private int _anchorY;
    
    #endregion    

    #region TrueValues

    private int _trueX;
    
    private int _trueY;

    private float _trueRotation;

    private float _trueAlpha;

    private Vector2 _trueScale;

    private ColorTransform _trueTransform;

    private bool _canInteract = true;

    #endregion

    private void InternalUpdate() {
        (_anchorX, _anchorY) = InternalUtils.GetAnchorOffset(Anchor, (int)ContentWidth, (int)ContentHeight);
        var (tx, ty) = (0, 0);
        var ta = Alpha;
        var ts = Scale;
        var tt = ColorTransformation;
        var tr = Rotation;
        var test = false;
        
        //TODO: maybe move these out into the client rather than being built in
        if (TooltipMode) {
            (tx, ty) = MouseInput.GetMousePosition().ToPair();
            (_anchorX, _anchorY) = InternalUtils.GetAnchorOffset(tx < UiRender.Screen.X / 2 ? UiAnchor.LeftBottom : UiAnchor.RightBottom, (int)ContentWidth, (int)ContentHeight);
            tx += (int) (_anchorX * Parent._trueScale.X);
            ty += (int) (_anchorY * Parent._trueScale.Y);
        } else if (_isDragging) {
            var pos = MouseInput.GetMousePosition();
            (tx, ty) = pos.ToPair();
            tx -= (int)(_dragOffset.X * Parent._trueScale.X);
            ty -= (int)(_dragOffset.Y * Parent._trueScale.Y);
        } else {
            tx += X + (int)(_anchorX * ScaleX);
            ty += Y + (int)(_anchorY * ScaleY);
            test = true;
        }
        
        var parentInteract = true;
        if (Parent != null) {
            if (test) {
                tx = (int) (tx * Parent._trueScale.X);
                ty = (int) (ty * Parent._trueScale.Y);
                tx += Parent._trueX;
                ty += Parent._trueY;
            }

            ta *= Parent._trueAlpha;
            ts *= Parent._trueScale;
            tt *= Parent._trueTransform;
            tr += Parent.Rotation;
            parentInteract = Parent._canInteract;
            
            if (Parent.EnableClipRect || Parent.ClipChildren) {
                _scissor = Parent._scissor;
                ClipChildren = true;
            }
        }
        
        
        _canInteract = parentInteract && Visible && !TweenActive;

        _trueX = tx;
        _trueY = ty;
        _trueAlpha = ta;
        _trueScale = ts;
        _trueTransform = tt;
        _trueRotation = tr;

        if (TooltipMode) {
            _trueX = Math.Clamp(_trueX, 0, UiRender.Screen.X - Width);
            _trueY = Math.Clamp(_trueY, 0, UiRender.Screen.Y - Height);
        }

        if (EnableClipRect) {
            var scissor = new Vector4(tx, ty, tx + SelfContentWidth * _trueScale.X, ty + SelfContentHeight * _trueScale.Y);
            if (ClipChildren) {
                _scissor.X = Math.Max(_scissor.X, scissor.X);
                _scissor.Y = Math.Max(_scissor.Y, scissor.Y);
                _scissor.Z = Math.Min(_scissor.Z, scissor.Z);
                _scissor.W = Math.Min(_scissor.W, scissor.W);
            }
            else {
                _scissor = scissor;
                ClipChildren = true;
            }
        }
        
        _info = new Vector2((float) TextureId, ta);
    }

    private void Update() {
        InternalUpdate();

        CheckHighestSprite();

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.Update();
        }
    }

    internal void InternalUpdateLoop() {
        BroadcastEvent(new Event(Event.EnterFrame));
        HandleFinishedTasks();
        
        Update();

        HandleHover();
        
        HighestSprite?.DispatchMouseEvents();
        
        HighestSprite = null;

        /*if (MouseInput.CheckEvent(MouseEvent.LeftUp) && TextInput.UnFocusOnClick) {
            TextInput.ActiveInput?.UnFocus();
            TextInput.UnFocusOnClick = false;
        }*/
            
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

    public Vector2i GetRelativeMousePosition() {
        var pos = MouseInput.GetMousePosition();
        return new Vector2i(pos.X - _trueX, pos.Y - _trueY);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetScale(int contentWidth, int newWidth) => contentWidth == 0 ? 0f : (float) newWidth / contentWidth;
}