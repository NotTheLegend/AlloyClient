using System;
using System.Collections.Generic;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

internal struct ObjectState(Vector2i pos, Vector2 scale, float alpha) {

    public static readonly ObjectState Default = new ObjectState(Vector2i.Zero, Vector2.One, 1f);

    public int X = pos.X;
    public int Y = pos.Y;
    public Vector2 Scale = scale;
    public float Alpha = alpha;

    public static ObjectState operator +(in ObjectState parent, in ObjectState child) => 
        new() {
            X = (int)(child.X * parent.Scale.X) + parent.X,
            Y = (int)(child.Y * parent.Scale.Y) + parent.Y,
            Scale = parent.Scale * child.Scale,
            Alpha = parent.Alpha * child.Alpha
        };
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

    public UiAnchor Anchor { get; set => IsBoundsChange(ref field, value); } = UiAnchor.LeftTop;
    
    public bool Visible { get; set => IsInstanceChange(ref field, value); } = true;
    
    public ColorTransform ColorTransformation { get; set => IsInstanceChange(ref field, value); } = ColorTransform.Default;

    public bool MouseEnabled = false;
    
    public DisplayContainer Parent { get; internal set; }
    
    public Stage Stage { get; private set; }
    
    private protected int ContentSizeWidth;
    private protected int ContentSizeHeight;

    private protected bool DirtyInstance;
    private protected ObjectState State;

    private protected bool _isDragging;

    private protected virtual void DoBoundsUpdate() => ContentSizeWidth = ContentSizeHeight = 0;
    
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

    private protected virtual Vector2i GetSelfContentDimensions() => Vector2i.Zero;
    
    internal Vector2i GetPositionWithAnchor() { // move tooltip mode out into client rather than built in feature
        if (_isDragging) {
            return Stage.Mouse.GetMousePosition();
        }
        
        var (width, height) = Anchor.GetOffset(ContentSizeWidth, ContentSizeHeight);
        return new Vector2i((int)(X + width * ScaleX), (int)(Y + height * ScaleY));
    }

    internal virtual void SetStageReference(Stage stage) => Stage = stage;

    internal virtual void Update(bool dirty, ObjectState state) {
        DirtyInstance = dirty || DirtyInstance;
        var currentState = new ObjectState(GetPositionWithAnchor(), Scale, Alpha);
        State = state + currentState;
    }

    internal virtual void Draw() { }
    
    
    
    internal bool TweenActive;
    
    
    
    
    
    
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