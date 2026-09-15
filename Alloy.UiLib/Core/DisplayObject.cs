using System;
using System.Collections.Generic;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

internal struct ObjectState(Vector2i pos, Vector2 scale, float alpha, ScissorRect scissor) {
    // !! ONLY ADD DATA THAT ENDS UP IN SHADERS !!

    public static readonly ObjectState Default = new(Vector2i.Zero, Vector2.One, 1f, ScissorRect.Default);

    public Vector2i Position = pos;
    public Vector2 Scale = scale;
    public float Alpha = alpha;
    public ScissorRect Scissor = scissor;

    public static ObjectState operator +(ObjectState state, in ObjectState child) {
        state.Position = (child.Position * state.Scale).AsInt() + state.Position;
        state.Scale *= child.Scale;
        state.Alpha *= child.Alpha;

        if (child.Scissor != ScissorRect.Default) {
            state.Scissor += child.Scissor.ToGlobal(state.Position, state.Scale);
        }
        
        return state;
    }
}

internal struct DisplayState(bool visible, bool mouseChildren) {
    public static readonly DisplayState Default = new(true, true);

    public bool Visible = visible;
    public bool MouseChildren = mouseChildren;

    public static DisplayState operator +(DisplayState state, in DisplayState child) {
        state.Visible = state.Visible && child.Visible;
        state.MouseChildren = state.MouseChildren && child.MouseChildren;
        return state;
    }
}

public abstract class DisplayObject : EventManager {

    private protected static readonly ILogger Logger = UiRender.LogFactory.CreateLogger(nameof(DisplayObject));

    private static readonly AncestorPool AncestorPool = new();

    #region BackingMethods

    private void IsBoundsChange<T>(ref T field, T value) where T : struct, IEquatable<T> {
        if (field.Equals(value)) {
            return;
        }

        field = value;
        DirtyInstance = true;

        DoBoundsUpdate();
    }
    
    private void IsInstanceChange<T>(ref T field, T value) where T : struct, IEquatable<T> {
        if (field.Equals(value)) {
            return;
        }

        field = value;
        DirtyInstance = true;
    }

    #endregion

    public int X { get; set => IsBoundsChange(ref field, value); }

    public int Y { get; set => IsBoundsChange(ref field, value); }
    
    public int Width { get => GetDimension(ContentSizeWidth, ScaleX); set => ScaleX = SetScale(ScaleX, ContentSizeWidth, value); }
    
    public int Height { get => GetDimension(ContentSizeHeight, ScaleY); set => ScaleY = SetScale(ScaleY, ContentSizeHeight, value); }

    public float ScaleX { get; set => IsBoundsChange(ref field, value); } = 1.0f; // neg scale mirrors around origin
    
    public float ScaleY { get; set => IsBoundsChange(ref field, value); } = 1.0f;
    
    public Vector2 Scale { get => new(ScaleX, ScaleY); set { ScaleX = value.X; ScaleY = value.Y; } }
    
    public float Alpha { get; set => IsInstanceChange(ref field, Math.Clamp(value, 0f, 1f)); } = 1f;
    
    public float Rotation { get; set => IsInstanceChange(ref field, value); }

    public UiAnchor Anchor { get; set => IsBoundsChange(ref field, value); } = UiAnchor.Default;
    
    public bool Visible { get; set => IsInstanceChange(ref field, value); } = true;
    
    public ColorTransform ColorTransformation { get; set => IsInstanceChange(ref field, value); } = ColorTransform.Default;

    public bool MouseEnabled = true;
    
    public DisplayContainer Parent { get; internal set; }
    
    public Stage Stage { get; private set; }

    protected CollisionType HitboxType = CollisionType.Square;

    public ScissorRect Scissor = ScissorRect.Default;
    
    // ======================

    private protected Bounds ContentBounds = Bounds.Zero;

    private protected Vector2i AnchorOffset = Vector2i.Zero;

    private protected bool CanInteract = true;
    
    private int ContentSizeWidth => ContentBounds.Width;
    
    private int ContentSizeHeight => ContentBounds.Height;

    private protected bool DirtyInstance; // Unused, keep for possible future implementation
    private protected ObjectState State;
    private protected DisplayState DisplayState;

    private bool _isDragging;
    private Vector2i _dragOffset;
    
    internal bool TweenActive; // TODO: remove & rework tween functionality

    private protected virtual Bounds GetSelfBounds() => Bounds.Zero;

    private protected virtual void DoBoundsUpdate() => ContentBounds = Bounds.Zero;
    
    private static int GetDimension(in int size, in float scale) => (int)(size * scale);

    private static float SetScale(in float scale, in int size, in int newSize) {
        if (size == 0 || newSize == 0) { // Avoid div by zero
            return 0;
        }
        
        if (newSize < 0) { // Ignore negative width/height
            return scale;
        }

        return (float)newSize / size;
    }

    internal Bounds GetContentBounds() => Bounds.Scale(ContentBounds, Scale).Translate(GetPositionWithAnchor());

    private Vector2i GetSelfPosition() {
        var (width, height) = AnchorOffset = ContentBounds.Anchor(Anchor);
        return new Vector2i((int)(X - width * ScaleX), (int)(Y - height * ScaleY));
    }

    // move tooltip mode out into client rather than built in feature
    private Vector2i GetPositionWithAnchor() => _isDragging ? Stage.Mouse.GetMousePosition() - _dragOffset : GetSelfPosition();

    internal virtual void SetStageReference(Stage stage) => Stage = stage;
    
    private protected virtual DisplayState GetDisplayState() => new(Visible, false);

    internal virtual void Update(bool dirty, ObjectState state, DisplayState displayState) {
        DirtyInstance = dirty || DirtyInstance;
        State = state + new ObjectState(GetSelfPosition(), Scale, Alpha, Scissor);
        DisplayState = displayState + GetDisplayState();
        CanInteract = DisplayState.Visible && displayState.MouseChildren && MouseEnabled;

        if (_isDragging) { // override position if dragging
            State.Position = Stage.Mouse.GetMousePosition() - (_dragOffset * Scale).AsInt();
        }
        
        if (CanInteract && FullBoundsCheck(Stage.Mouse.GetMousePosition())) {
            Stage.CurrentHighestSprite = this;
        }
    }

    internal virtual void Draw() { }


    private Vector2i GetLocalPosition(Vector2i position) => ((position - State.Position) / State.Scale).AsInt();

    private bool FullBoundsCheck(Vector2i position) {
        if (!State.Scissor.Contains(position)) {
            return false;
        }
        
        var hasBounds = ContentBounds.Area == 0; // guard for the one pixel hole in empty bounds
        var firstCheck = IsInBounds(GetLocalPosition(position), CollisionType.SimpleSquare);

        if (!firstCheck) { // Break early if rough bounds fails
            return false;
        }

        if (hasBounds && HitboxType == CollisionType.SimpleSquare) { // SimpleSquare doesn't recursive check children
            return true;
        }
        
        return IsInBounds(position);
    }

    internal virtual bool IsInBounds(Vector2i position) {
        if (!State.Scissor.Contains(position)) {
            return false;
        }

        if (ContentBounds.Area == 0) { // guard for the one pixel hole in empty bounds
            return false;
        }

        return IsInBounds(GetLocalPosition(position), HitboxType);
    }

    private bool IsInBounds(Vector2i position, CollisionType type) => type switch {
        CollisionType.SimpleSquare => ContentBounds.Contains(position),
        CollisionType.Square => HitboxSquare(position),
        CollisionType.Ellipse => HitboxEllipse(position),
        CollisionType.Vertices => HitboxComplex(position),
        CollisionType.Custom => CustomHitbox(position),
        _ => throw new ArgumentOutOfRangeException($"{type} not handled in InternalBoundsCheck")
    };

    private protected virtual bool HitboxSquare(Vector2i position) => false;
    
    private protected virtual bool HitboxEllipse(Vector2i position) => false;
    
    private protected virtual bool HitboxComplex(Vector2i position) => false;

    /// <param name="pos">local mouse coords</param>
    protected virtual bool CustomHitbox(Vector2i pos) => throw new MissingMethodException("Sprite must define override for CustomHitbox");

    #region Events
    
    private static readonly HashSet<string> BroadcastEvents = [Event.EnterFrame];
    
    internal static readonly Dictionary<string, CachedList<DisplayObject>> BroadcastMap = new();

    public sealed override void AddEventListener<T>(EventType<T> type, Action callback, bool capture = false) {
        if (IsBroadcast(type)) {
            if (!BroadcastMap.TryGetValue(type, out var list)) {
                BroadcastMap[type] = list = [];
            }
            
            list.Add(this);
        }
        
        base.AddEventListener(type, callback, capture);
    }
    
    public sealed override void AddEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) {
        if (IsBroadcast(type)) {
            if (!BroadcastMap.TryGetValue(type, out var list)) {
                BroadcastMap[type] = list = [];
            }
            
            list.Add(this);
        }
        
        base.AddEventListener(type, callback, capture);
    }

    public sealed override void AddEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) {
        if (IsBroadcast(type)) {
            if (!BroadcastMap.TryGetValue(type, out var list)) {
                BroadcastMap[type] = list = [];
            }
            
            list.Add(this);
        }
        
        base.AddEventListener(type, callback, capture);
    }

    public sealed override void RemoveEventListener<T>(EventType<T> type, Action callback, bool capture = false) {
        if (IsBroadcast(type) && BroadcastMap.TryGetValue(type, out var list)) {
            list.Remove(this);
        }
        
        base.RemoveEventListener(type, callback, capture);
    }

    public sealed override void RemoveEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) {
        if (IsBroadcast(type) && BroadcastMap.TryGetValue(type, out var list)) {
            list.Remove(this);
        }
        
        base.RemoveEventListener(type, callback, capture);
    }

    public sealed override void RemoveEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) {
        if (IsBroadcast(type) && BroadcastMap.TryGetValue(type, out var list)) {
            list.Remove(this);
        }
        
        base.RemoveEventListener(type, callback, capture);
    }

    private static bool IsBroadcast(string type) => BroadcastEvents.Contains(type);
    
    public sealed override void DispatchEvent(Event @event) {
        if (@event is null || string.IsNullOrWhiteSpace(@event.Type)) {
            throw new Exception("Event or Event.Type must not be null, empty, or whitespace");
        }
        
        DispatchWithCapture(@event);
    }

    internal void DispatchWithCapture(Event @event) {
        if (@event.Target is null) {
            @event.SetTarget(this);
        }
        
        if (Parent != null) {
            var chain = AncestorPool.Pop();
        
            var obj = this;

            while ((obj = obj!.Parent) != null) {
                chain.Add(obj);
            }

            @event.Phase = EventPhase.Capture;
            for (var i = chain.Count - 1; i >= 0; i--) {
                if (@event.ImmediateStop) {
                    AncestorPool.Push(chain);
                    return;
                }
                base.DispatchEventInternal(@event);
            }
            
            AncestorPool.Push(chain);

            if (@event.Stop) {
                return;
            }
        }

        @event.Phase = EventPhase.Target;
        DispatchEventInternal(@event);
    }

    private protected sealed override void DispatchEventInternal(Event @event) {
        var parent = @event.Bubbles ? Parent : null;
        base.DispatchEventInternal(@event);
        
        if (@event.Stop || parent is null || parent == this) {
            return;
        }
        
        @event.Phase = EventPhase.Bubble;
        parent.DispatchEventInternal(@event);
    }
    
    internal virtual void DispatchChildren(Event @event) { }

    #endregion
}