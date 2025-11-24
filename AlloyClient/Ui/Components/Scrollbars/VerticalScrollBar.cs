using System;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;

namespace RealmClient.Ui.Components.Scrollbars;

public struct VerticalScrollBarConfig {
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public int TotalContentHeight = 0;
    public int VisibleContentHeight = 0;
    public Action<int> OnValueChanged = null;
    
    // When set, scroll value will be incremented directly from this value.
    // (e.g. when set to 32, value changes will be 32, 64, 96, etc.)
    public int ScrollStep = -1;
    
    public VerticalScrollBarConfig() { }
}

public class VerticalScrollBar : Sprite {
    private readonly NineSliceRect _scrollBarHandleTexture;

    private readonly Action<int> _onValueChanged;
    
    private int _dragOffset;

    private readonly int _scrollStep;
    private readonly int _scrollHeight;
    private readonly int _heightDifference;
    private int _lastScrollY;

    public VerticalScrollBar(Container clipRect, VerticalScrollBarConfig config) {
        X = config.X;
        Y = config.Y;
        _onValueChanged = config.OnValueChanged;

        _scrollStep = config.ScrollStep == -1 ? config.Height / 20 : config.ScrollStep;

        var scrollBarTexture = new NineSliceRect(new NineSliceConfig {
            SliceData = SliceLibrary.ScrollBarBg, Width = config.Width, Height = config.Height, Anchor = UiAnchor.MiddleTop, MouseEnabled = true
        });
        AddChild(scrollBarTexture);

        var handleHeight = CalculateHandleHeight(config.Height, config.TotalContentHeight, config.VisibleContentHeight);
        _scrollBarHandleTexture = new NineSliceRect(new NineSliceConfig {
            SliceData = SliceLibrary.ScrollBar, CutX = 4, CutY = 4, Width = config.Width, Height = handleHeight, Anchor = UiAnchor.MiddleTop, MouseEnabled = true
        });
        AddChild(_scrollBarHandleTexture);

        _scrollHeight = config.Height - handleHeight;
        _heightDifference = config.TotalContentHeight - config.VisibleContentHeight;

        _scrollBarHandleTexture.AddEventListener(MouseEvent.LeftDown, OnHandleDown);
        //todo mousemove?
        //_scrollBarHandleTexture.AddEventListener(MouseEvent.MouseMove, OnMouseMove);
        clipRect.AddEventListener(MouseEvent.ScrollVertical, Scroll);

        MouseEnabled = true;
    }

    private static int CalculateHandleHeight(int scrollBarHeight, int totalContentHeight, int visibleAreaHeight) {
        var handleHeight = (int) (scrollBarHeight * (visibleAreaHeight / (float) totalContentHeight));
        handleHeight = Math.Clamp(handleHeight, 20, scrollBarHeight);
        return handleHeight;
    }

    private void OnHandleDown(MouseEvent args) {
        Stage.AddEventListener(MouseEvent.LeftUp, OnHandleUp, true);
        Stage.AddEventListener(MouseEvent.MouseMove, OnMouseMove, true);
        _dragOffset = args.Coords.Y - _scrollBarHandleTexture.Y;
    }

    private void OnHandleUp(MouseEvent args) {
        args.StopImmediatePropagation();
        Stage.RemoveEventListener(MouseEvent.LeftUp, OnHandleUp, true);
        Stage.RemoveEventListener(MouseEvent.MouseMove, OnMouseMove, true);
    }

    private void UpdateScrollHandlePosition(float newY) {
        newY = Math.Clamp(newY, 0, _scrollHeight);
        _scrollBarHandleTexture.Y = (int) newY;
        var scrollY = (int) (newY / _scrollHeight * _heightDifference);
        if (scrollY != _lastScrollY) {
            _onValueChanged(scrollY);
            _lastScrollY = scrollY;
        }
    }

    private void OnMouseMove(MouseEvent args) {
        args.StopImmediatePropagation();
        var newY = args.Coords.Y - _dragOffset;
        UpdateScrollHandlePosition(newY);
    }

    private void Scroll(MouseEvent args) {
        if (_heightDifference < 0) return;
        
        var newScrollY = _lastScrollY - args.Delta * _scrollStep;
        newScrollY = Math.Clamp(newScrollY, 0, _heightDifference);
    
        _onValueChanged((int) newScrollY);
    
        var handleY = newScrollY / _heightDifference * _scrollHeight;
        UpdateScrollHandlePosition(handleY);
    
        _lastScrollY = (int) newScrollY;
    }
}