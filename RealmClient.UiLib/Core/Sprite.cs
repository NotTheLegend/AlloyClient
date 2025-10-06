using System;
using System.Collections.Generic;
using Common;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;
using RealmClient.UiLib.Input;
using RealmClient.UiLib.Rendering;
using RealmClient.UiLib.Utils;

namespace RealmClient.UiLib.Core;

/* =TODO=
 * Focus
 * event phase enum
 * enter frame event instead of override thing
 * make enter frame & resize broadcast events
 */

public partial class Sprite : DisplayContainer {
    
    public static readonly Vector4 NoScissor = new Vector4(0, 0, 10000, 10000);

    public int X {
        get => _x;
        set {
            _x = value;
            UpdateBounds();
        }
    }

    public int Y {
        get => _y;
        set {
            _y = value;
            UpdateBounds();
        }
    }
    
    public float Alpha {
        get => _alpha;
        set => _alpha = Math.Max(Math.Min(value, 1f), 0f);
    }

    public int Width {
        get => (int)(ContentWidth * Scale.X);
        set {
            Scale = new Vector2(ContentWidth == 0 ? 0 : value / ContentWidth, Scale.Y);
            UpdateBounds();
        }
    }
    
    public int Height {
        get => (int)(ContentHeight * Scale.Y);
        set {
                Scale = new Vector2(Scale.X, ContentHeight == 0 ? 0 : value / ContentHeight);
            UpdateBounds();
        }
    }

    public Vector2 Scale {
        get => _scale;
        set {
            _scale = value;
            UpdateBounds();
        }
    }

    public float Rotation = 0;

    public UiAnchor Anchor { get; private set; } = UiAnchor.LeftTop;

    public HitboxType HitboxType { get; private set; } = HitboxType.Default;
    
    public bool Visible = true;
    
    public bool MouseEnabled = false;
    
    public bool FollowMouse = false;

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

    private int _x;
    
    private int _y;

    private float _alpha = 1f;
    
    private Vector2 _scale = Vector2.One;

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

    internal Vector2 GetFullRelativePosition() {
        var (x, y) = InternalUtils.GetAnchorOffset(Anchor, (int)ContentWidth, (int)ContentHeight);
        return new Vector2(_x + x * Scale.X, _y + y * Scale.Y);
    }

    

    //protected virtual void OnUpdate(GameTime gameTime) { }

    private void InternalUpdate() {
        (_anchorX, _anchorY) = InternalUtils.GetAnchorOffset(Anchor, (int)ContentWidth, (int)ContentHeight);
        var (tx, ty) = (0, 0);
        var ta = _alpha;
        var ts = Scale;
        var tt = ColorTransformation;
        var tr = Rotation;
        var test = false;
        

        if (FollowMouse) {
            var pos = MouseInput.GetMousePosition();
            (tx, ty) = pos.ToPair();
        } else if (TooltipMode) {
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
            tx += X + (int)(_anchorX * _scale.X);
            ty += Y + (int)(_anchorY * _scale.Y);
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
            var scissor = new Vector4(tx, ty, tx + Width * _trueScale.X, ty + Height * _trueScale.Y);
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

    private void Update(GameTime gameTime) {
        //OnUpdate(gameTime);// replace with enter frame event
        
        InternalUpdate();

        CheckHighestSprite();

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.Update(gameTime);
        }
    }

    internal void InternalUpdateLoop(GameTime gameTime) {
        HandleFinishedTasks();
        BroadcastEvent(new Event(Event.EnterFrame));
        
        Update(gameTime);

        HandleHover();
        
        HighestSprite?.DispatchMouseEvents();
        
        HighestSprite = null;

        if(MouseInput.CheckEvent(MouseEvent.LeftUp) && TextInput.UnFocusOnClick)
            TextInput.ActiveInput?.UnFocus();
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
    
    public void SetHitboxType(HitboxType hitbox) => HitboxType = hitbox;

    public Vector2i GetRelativeMousePosition() {
        var pos = MouseInput.GetMousePosition();
        return new Vector2i(pos.X - _trueX, pos.Y - _trueY);
    }
}