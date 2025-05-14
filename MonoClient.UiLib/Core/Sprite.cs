using System;
using System.Collections.Generic;
using Common;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Extra;
using MonoClient.UiLib.Rendering;
using MonoClient.UiLib.Utils;

namespace MonoClient.UiLib.Core;

public partial class Sprite : EventManager {
    
    public static readonly Vector4 NoScissor = new Vector4(0, 0, 10000, 10000);

    public Stage Stage {
        get;
        internal set {
            if (field == value)
                return;

            if (value == null) {
                DispatchEvent(new Event(Event.RemovedToStage));
                
                if (_autoResize != null) {
                    field.RemoveEventListener(ResizeEvent.Resize, _autoResize);
                }
                
                field = null;
            } else {
                field = value;
                
                if (_autoResize != null) {
                    field.AddEventListener(ResizeEvent.Resize, _autoResize);
                    _autoResize(new ResizeEvent(ResizeEvent.Resize, -1, -1, field.StageWidth, field.StageHeight));
                }
                
                DispatchEvent(new Event(Event.AddedToStage));
            }
        }
    }

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
        get => _width;
        set {
            if (_width == 0)
                _graphicalWidth = value;
            else 
                Scale = new Vector2(value / (float)_width, Scale.Y);
            UpdateBounds();
        }
    }
    
    public int Height {
        get => _height;
        set {
            if (_height == 0)
                _graphicalHeight = value;
            else
                Scale = new Vector2(Scale.X, value / (float)_height);
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

    public Sprite Parent => _parent;

    public UiAnchor Anchor { get; private set; } = UiAnchor.LeftTop;

    public HitboxType HitboxType { get; private set; } = HitboxType.Default;
    
    public bool Visible = true;
    
    public bool MouseEnabled = false;
    
    public bool FollowMouse = false;

    public bool TooltipMode = false;

    public bool EnableClipRect = false;
    public bool ClipChildren = false;
    
    internal bool TweenActive = false;
    
    protected short[] Indices = [];
    
    protected VertexUi[] VertexData = [];

    public int OverridePrimCount = -1;

    internal IntVector2 Radii;
    
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

    private int _width;

    private int _height;

    private int _graphicalWidth;

    private int _graphicalHeight;

    private int _anchorX;
    
    private int _anchorY;
    
    #endregion    

    #region Children
    
    private Sprite _parent;
    
    private readonly List<Sprite> _children = [];
    
    private readonly Queue<Sprite> _childQueue = new();
    
    private readonly Queue<Sprite> _childRemovalQueue = new();

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

    #region Other

    private bool _noRenderData = true;

    private int _childCount;

    private Bounds _childbounds = new();
    private Bounds _bounds = new();

    private Action<ResizeEvent> _autoResize;

    #endregion

    #region Rendering

    private const int IndexBuffer = 2000;
    private const int VertexBuffer = 1000;
    
    private static int _drawCount;

    private static short _indexCount;
    private static short[] _indices;
    private static DynamicIndexBuffer _indexBuffer;

    private static int _vertexCount;
    private static VertexDataUi[] _vertices;
    private static DynamicVertexBuffer _vertexBuffer;

    #endregion

    internal static void BuildBuffers(GraphicsDevice graphics) {
        _indices = new short[IndexBuffer];
        _indexBuffer = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, IndexBuffer, BufferUsage.WriteOnly);

        _vertices = new VertexDataUi[VertexBuffer];
        _vertexBuffer = new DynamicVertexBuffer(graphics, VertexDataUi.VertexDeclaration, VertexBuffer, BufferUsage.WriteOnly);
    }

    private void UpdateBounds() {

        var bounds = _childbounds;

        if (_childCount > 0) { 
            bounds.AddParent(this);
        }

        var update = false;

        var (anchorX, anchorY) = InternalUtils.GetAnchorOffset(Anchor, _graphicalWidth, _graphicalHeight);

        var x = X + (int)(anchorX * Scale.X);
        var w = (int)(_graphicalWidth * Scale.X);
        var y = Y + (int)(anchorY * Scale.Y);
        var h = (int)(_graphicalHeight * Scale.Y);
        
        if (x < bounds.MinX) {
            update = true;
            bounds.MinX = x;
        }

        if (x + w > bounds.MaxX) {
            update = true;
            bounds.MaxX = x + w;
        }
        
        if (y < bounds.MinY) {
            update = true;
            bounds.MinY = y;
        }

        if (y + h > bounds.MaxY) {
            update = true;
            bounds.MaxY = y + h;
        }
        
        _width = bounds.MaxX - bounds.MinX;
        _height = bounds.MaxY - bounds.MinY;
        _bounds = bounds;

        // if clip, override w/h to clip size
        if (EnableClipRect) {
            _width = (int)(_graphicalWidth * Scale.X);
            _height = (int)(_graphicalHeight * Scale.Y);
        }

        if (update && _parent != null) {
            _parent.UpdateChildBounds(bounds);
            _parent.UpdateBounds();
        }

        if (TooltipMode) // Lets not play with tooltip width, it should remain its width no matter the children
        {
            _width = _graphicalWidth;
        }
    }

    private void UpdateChildBounds(Bounds bounds) {
        if (bounds.MinX <_childbounds.MinX) {
            _childbounds.MinX = bounds.MinX;
        }
        
        if (bounds.MaxX > _childbounds.MaxX) {
            _childbounds.MaxX = bounds.MaxX;
        }
        
        if (bounds.MinY < _childbounds.MinY) {
            _childbounds.MinY = bounds.MinY;
        }
        
        if (bounds.MaxY > _childbounds.MaxY) {
            _childbounds.MaxY = bounds.MaxY;
        }
    }

    protected virtual void ResizeBackBuffer() {
        _noRenderData = VertexData == null || VertexData.Length < 1;
    }

    protected virtual void OnUpdate(GameTime gameTime) { }

    private void InternalUpdate() {
        (_anchorX, _anchorY) = InternalUtils.GetAnchorOffset(Anchor, _width, _height);
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
            (_anchorX, _anchorY) = InternalUtils.GetAnchorOffset(tx < UiRender.Screen.X / 2 ? UiAnchor.LeftBottom : UiAnchor.RightBottom, _width, _height);
            tx += (int) (_anchorX * _parent._trueScale.X);
            ty += (int) (_anchorY * _parent._trueScale.Y);
        } else if (_isDragging) {
            var pos = MouseInput.GetMousePosition();
            (tx, ty) = pos.ToPair();
            tx -= (int)(_dragOffset.X * _parent._trueScale.X);
            ty -= (int)(_dragOffset.Y * _parent._trueScale.Y);
        } else {
            tx += X + _anchorX;
            ty += Y + _anchorY;
            test = true;
        }
        
        var parentInteract = true;
        if (_parent != null) {
            if (test) {
                tx = (int) (tx * _parent._trueScale.X);
                ty = (int) (ty * _parent._trueScale.Y);
                tx += _parent._trueX;
                ty += _parent._trueY;
            }

            ta *= _parent._trueAlpha;
            ts *= _parent._trueScale;
            tt *= _parent._trueTransform;
            tr += _parent.Rotation;
            parentInteract = _parent._canInteract;
            
            if (_parent.EnableClipRect || _parent.ClipChildren) {
                _scissor = _parent._scissor;
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
        OnUpdate(gameTime);
        
        InternalUpdate();

        while (_childQueue.TryDequeue(out var child)) {
            _children.Add(child);
        }

        while (_childRemovalQueue.TryDequeue(out var child)) {
            _children.Remove(child);
        }
        
        UpdateNormalListeners();

        CheckHighestSprite();

        foreach (var child in _children) {
            child.Update(gameTime);
        }
    }

    internal void InternalUpdateLoop(GameTime gameTime) {
        if (_lockMouse && MouseInput.GetMousePosition().Clamp(new IntVector2(0), UiRender.Screen, out var pos)) {
            Mouse.SetPosition(pos.X, pos.Y);
        }
        
        Update(gameTime);

        HandleHover();
        
        HighestSprite?.DispatchMouseEvents();
        
        HighestSprite = null;

        if(MouseInput.CheckEvent(MouseEvent.LeftUp) && TextInput.UnFocusOnClick)
            TextInput.ActiveInput?.UnFocus();
    }

    private void DrawInternal() {
        if (_noRenderData || _trueAlpha == 0f || OverridePrimCount == 0) return;
        
        if (_vertexCount + VertexData.Length > VertexBuffer || _indexCount + Indices.Length > IndexBuffer)
            FlushRenderBuffer();

        var count = OverridePrimCount < 0 ? Indices.Length : OverridePrimCount * 3; // # of indices
        var numVertices = 0;
        for (var i = 0; i < count; i++) {
            var index = Indices[i];
            _indices[_indexCount + i] = (short)(_vertexCount + index);

            if (index > numVertices)
                numVertices = index;// Get highest vertex index
        }

        _indexCount += (short)count;

        numVertices++;
        for (var i = 0; i < numVertices; i++) {
            var color = VertexData[i].Color;
            _vertices[_vertexCount + i] = new VertexDataUi(VertexData[i].Position.Transform(_trueScale, _trueRotation, _trueX, _trueY, _anchorX, _anchorY), color.A == 0f ? Color : color, ColorSecondary, _info, VertexData[i].UV, _scissor, Extra1, Extra2, _trueTransform);
        }

        _vertexCount += numVertices;
        
        UiRender.LastRenderCount++;
    }

    internal void InternalDrawLoop() {
        UiRender.Graphics.DepthStencilState = DepthStencilState.None;
        UiRender.Graphics.Indices = _indexBuffer;
        UiRender.Graphics.SetVertexBuffer(_vertexBuffer);
        UiRender.UiShader.CurrentTechnique.Passes[0].Apply();
        
        Draw();
        
        FlushRenderBuffer();
    }

    private static void FlushRenderBuffer() {
        if (_indexCount == 0) return;
        
        _indexBuffer.SetData(_indices, 0, _indexCount);
        _vertexBuffer.SetData(_vertices, 0, _vertexCount);
        
        UiRender.Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indexCount / 3);

        _indexCount = 0;
        _vertexCount = 0;
    }
    
    private void Draw() {
        if (!Visible) return;
        
        DrawInternal();
        
        foreach (var child in _children) {
            child.Draw();
        }
    }

    private void SetStage(Stage stage) {
        foreach (var child in _children) {
            child.UpdateNormalListeners();
            child.Stage = stage;
            child.SetStage(stage);
        }

        foreach (var child in _childQueue) {
            child.UpdateNormalListeners();
            child.Stage = stage;
            child.SetStage(stage);
        }
    }

    public bool ContainsChild(Sprite child) {
        if (child == null) return false;
        return _children.Contains(child) || _childQueue.Contains(child);
    }
    
    public void AddChild(Sprite child) {
        if (_children.Contains(child) || _childQueue.Contains(child) || child == this) return;
        
        _childCount++;
        child._parent = this;
        _childQueue.Enqueue(child);

        child.UpdateNormalListeners();
        child.Stage = Stage;
        child.SetStage(Stage);

        _childbounds.MinX = Math.Min(_childbounds.MinX, child._bounds.MinX);
        _childbounds.MaxX = Math.Max(_childbounds.MaxX, child._bounds.MaxX);
        _childbounds.MinY = Math.Min(_childbounds.MinY, child._bounds.MinY);
        _childbounds.MaxY = Math.Max(_childbounds.MaxY, child._bounds.MaxY);
        
        UpdateBounds();
    }

    public void RemoveChild(Sprite child) {
        if (child == null || !_children.Contains(child)) return;
        _childCount--;
        _childRemovalQueue.Enqueue(child);

        child._parent = null;
        
        child.Stage = null;
        child.SetStage(null);

        var bounds = new Bounds();

        for (var i = 0; i < _children.Count; i++) {
            var c = _children[i];
            if (_childRemovalQueue.Contains(c)) continue;
            
            bounds.MinX = Math.Min(bounds.MinX, c._bounds.MinX);
            bounds.MaxX = Math.Max(bounds.MaxX, c._bounds.MaxX);
            bounds.MinY = Math.Min(bounds.MinY, c._bounds.MinY);
            bounds.MaxY = Math.Max(bounds.MaxY, c._bounds.MaxY);
        }

        _childbounds = bounds;
        UpdateBounds();
    }

    public void RemoveAllChildren() {
        _childbounds = new Bounds();
        foreach (var child in _children) {
            _childRemovalQueue.Enqueue(child);
        }
    }
    
    public void PrioritizeChild(Sprite child) {
        if (_children.Count > 0 && _children[^1] == child) {
            return;
        }

        if (_children == null || !_children.Contains(child)) {
            return;
        }
        
        _children.Remove(child); 
        _children.Add(child);
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

    public void SetBaseDimensions(int width, int height) {
        _graphicalWidth = width;
        _graphicalHeight = height;
        UpdateBounds();
    }

    public void SetAnchor(UiAnchor anchor) {
        Anchor = anchor;
        UpdateBounds();
    }

    public void SetAutoResize(Action<ResizeEvent> callback) => _autoResize = callback;
    
    public void SetHitboxType(HitboxType hitbox) => HitboxType = hitbox;

    public IntVector2 GetRelativeMousePosition() {
        var pos = MouseInput.GetMousePosition();
        return new IntVector2(pos.X - _trueX, pos.Y - _trueY);
    }
    
    private struct Bounds {
        
        public int MinX = int.MaxValue;
        public int MaxX = int.MinValue;
        public int MinY = int.MaxValue;
        public int MaxY = int.MinValue;
        
        public Bounds(){}

        public void AddParent(Sprite sprite) {
            var w = sprite._noRenderData ? sprite._childbounds.MaxX - sprite._childbounds.MinX : sprite._graphicalWidth;
            var h = sprite._noRenderData ? sprite._childbounds.MaxY - sprite._childbounds.MinY : sprite._graphicalHeight;

            var (anchorX, anchorY) = InternalUtils.GetAnchorOffset(sprite.Anchor, w, h);
            var offsetX = sprite.X + anchorX * sprite.Scale.X;
            var offsetY = sprite.Y + anchorY * sprite.Scale.Y;

            MinX = (int) (MinX * sprite.Scale.X + offsetX);
            MaxX = (int) (MaxX * sprite.Scale.X + offsetX);
            MinY = (int) (MinY * sprite.Scale.Y + offsetY);
            MaxY = (int) (MaxY * sprite.Scale.Y + offsetY);
        }
    }
}