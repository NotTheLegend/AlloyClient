using System;
using System.Collections.Generic;
using System.Text;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.BuiltIn;

public struct InputConfig {
    public int X = 0;
    public int Y = 0;
    public float FontSize = 10;
    public FontType FontType = FontType.Normal;
    public uint Color = 0xFFFFFF;
    public uint OutlineColor = 0x0;
    public uint OutlineThickness = 4;
    public int Width = 100;
    public string DefaultText = "";
    public byte MaxCharacters = byte.MaxValue;
    public bool Password = false;
    public bool ClickToActivate = true;
    public Action OnFocus = null;
    public Action OnUnfocus = null;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    
    public Action OnChange = null;
    
    public InputConfig() { }
}

public sealed class TextInput : Sprite {

    public const string BoxLookup = "textBox";

    private const int CutX = 2;
    private const int CutY = 2;

    internal static TextInput ActiveInput;
    
    public string Text => _inputText.ToString();
    private readonly StringBuilder _inputText = new();
    private bool _isDefaultText = true;
    
    private readonly float _fontScale;
    private readonly BitmapFont _font;
    private readonly float _outlineThickness;
    private readonly int _width;
    private readonly string _defaultText;
    private readonly byte _maxCharacters;
    private readonly bool _password;
    private readonly bool _clickActivate;
    private readonly Action _onFocus;
    private readonly Action _onUnfocus;
    private readonly Action _onChange;

    private readonly NineSliceRect _textBox;
    private readonly ColorRect _caret;
    private readonly ColorRect _selectionHighlight;
    private int _caretIndex = -1;
    private int _selectionAnchor = -1;
    private int _selectionEnd = -1;
    private bool _isCaretActive;
    private double _lastCaretUpdateTime;
    private int _startIndex;
    private int _endIndex;
    private bool _mouseSelecting;

    private readonly Stack<EditState> _undo = new();
    private readonly Stack<EditState> _redo = new();

    private struct EditState {
        public string Text;
        public int Caret;
    }

    public TextInput(InputConfig config) {
        X = config.X;
        Y = config.Y;
        _fontScale = config.FontSize;
        _font = UiRender.GetFont(config.FontType);
        SetColor(config.Color);
        SetColorSecondary(config.OutlineColor);
        _outlineThickness = _font.ValidateOutlineSize(config.OutlineThickness);
        _width = config.Width;
        _defaultText = config.DefaultText;
        _maxCharacters = config.MaxCharacters;
        _password = config.Password;
        _clickActivate = config.ClickToActivate;
        _onFocus = config.OnFocus;
        _onUnfocus = config.OnUnfocus;
        _onChange = config.OnChange;
        SetAnchor(config.Anchor);

        MouseEnabled = true;

        TextureId = TextureType.Text;

        Extra1.X = _outlineThickness;

        _inputText.Append(config.DefaultText);

        var selectionConfig = new ColorRectConfig {
            X = CutX * 3,
            Y = CutY * 3,
            Width = 1,
            Height = (int)(_font.LineHeight * _fontScale),
            Color = 0x4A90E2,
            Alpha = 0.45f,
        };
        _selectionHighlight = new ColorRect(selectionConfig);
        _selectionHighlight.Visible = false;
        AddChild(_selectionHighlight);
        
        var caretConfig = new ColorRectConfig {
            Width = 1,
            Height = (int)(_font.LineHeight * _fontScale),
            Color = config.Color,
        };
        _caret = new ColorRect(caretConfig);
        _caret.Visible = false;
        AddChild(_caret);
        
        var rectConfig = new NineSliceConfig { Width = _width, Height = (int)(_font.LineHeight * _fontScale) + CutY * 3, SliceData = BoxLookup, CutX = CutX, CutY = CutY};
        _textBox = new NineSliceRect(rectConfig);
        AddChild(_textBox);
        
        SetHitboxType(CollisionType.CustomNoScale);
        
        AddEventListener(MouseEvent.LeftDown, OnSelectionDown);
        
        ResizeBackBuffer();
        FillData();
    }
    
    private void ResizeBackBuffer() {
        var size = _maxCharacters + 1;
        VertexData = new VertexUi[size * 4];
        Indices = new ushort[size * 6];
        for (var i = 0; i < Indices.Length / 6; i++) {
            var idx6 = i * 6;
            var idx4 = i * 4;

            Indices[idx6] = (ushort)(0 + idx4);
            Indices[idx6 + 1] = (ushort)(1 + idx4);
            Indices[idx6 + 2] = (ushort)(2 + idx4);
            Indices[idx6 + 3] = (ushort)(0 + idx4);
            Indices[idx6 + 4] = (ushort)(2 + idx4);
            Indices[idx6 + 5] = (ushort)(3 + idx4);
        }
    }
    
    private void OnFrameEnter() {
        if (!_isCaretActive) {
            return;
        }

        if (HasSelection()) {
            _caret.Visible = false;
            return;
        }

        var gameTime = Stage.GameTime;
        if (gameTime.TotalMs - _lastCaretUpdateTime < 500) {
            return;
        }

        _lastCaretUpdateTime = gameTime.TotalMs;
        _caret.Visible = !_caret.Visible;
    }

    private void ShowCaretNow(bool visible) {
        _caret.Visible = visible;
        _lastCaretUpdateTime = Stage.GameTime.TotalMs;
    }

    private void FillData() {
        var startX = CutX * 3;
        var startY = _font.Ascender * _fontScale + CutY * 3;
        var zero = new Vector2(startX, startY);

        var (start, end) = _font.GetStartIndex(_inputText, _caretIndex, _width - startX * 2 - _caret.Width, _outlineThickness, _fontScale);
        _startIndex = start;
        _endIndex = end;
        OverridePrimCount = 2;
        
        var idx = 4;
        var len = _inputText.Length;
        var caret = false;

        var password = _password && !_isDefaultText;
        var hasSelection = HasSelection();
        
        for (var i = start; i < end; i++) {

            if (!hasSelection && !caret && i == _caretIndex) {
                caret = true;
                i--;
                
                _caret.X = (int)zero.X;
                _caret.Y = CutY * 3;
                
                continue;
            }
            
            var c = password ? '*' : _inputText[i];
            switch (c) {
                case '\n':
                case '\r': 
                    continue;
                default:
                    if (!_font.Glyphs.TryGetValue(c, out var glyph)) {
                        continue;
                    }

                    var uv = glyph.UV;
                    var pos = glyph.Position;
                    
                    VertexData[idx + 0] = new VertexUi(new Vector2(zero.X + pos.X0 * _fontScale, zero.Y - pos.Y1 * _fontScale), new Vector2(uv.X0, uv.Y1)); //bl
                    VertexData[idx + 1] = new VertexUi(new Vector2(zero.X + pos.X0 * _fontScale, zero.Y - pos.Y0 * _fontScale), new Vector2(uv.X0, uv.Y0)); //tl
                    VertexData[idx + 2] = new VertexUi(new Vector2(zero.X + pos.X1 * _fontScale, zero.Y - pos.Y0 * _fontScale), new Vector2(uv.X1, uv.Y0)); //tr
                    VertexData[idx + 3] = new VertexUi(new Vector2(zero.X + pos.X1 * _fontScale, zero.Y - pos.Y1 * _fontScale), new Vector2(uv.X1, uv.Y1)); //br

                    if (i < len - 1) {
                        var k = password ? '*' : _inputText[i + 1];
                        _font.Kernings.TryGetValue((c, k), out var kern);
                        zero.X += kern * _fontScale;
                    }

                    zero.X += glyph.Advance * _fontScale;
                    idx += 4;
                    OverridePrimCount += 2;
                    continue;
            }
        }

        if (!caret) {
            _caret.X = (int)zero.X;
            _caret.Y = CutY * 3;
        }

        UpdateSelectionHighlight(start, end, password);
        
        SetGraphicsBuffer();
    }

    private void UpdateSelectionHighlight(int visibleStart, int visibleEnd, bool password) {
        if (!HasSelection()) {
            _selectionHighlight.Visible = false;
            return;
        }

        var (selectionStart, selectionEnd) = GetSelectionRange();
        selectionStart = Math.Max(selectionStart, visibleStart);
        selectionEnd = Math.Min(selectionEnd, visibleEnd);
        if (selectionStart >= selectionEnd) {
            _selectionHighlight.Visible = false;
            return;
        }

        var x1 = GetTextX(visibleStart, selectionStart, password);
        var x2 = GetTextX(visibleStart, selectionEnd, password);
        _selectionHighlight.X = (int)x1;
        _selectionHighlight.Resize(Math.Max(1, (int)MathF.Ceiling(x2 - x1)), (int)(_font.LineHeight * _fontScale));
        _selectionHighlight.Visible = true;
    }

    private float GetTextX(int start, int end, bool password) {
        var x = CutX * 3f;
        for (var i = start; i < end && i < _inputText.Length; i++) {
            var c = password ? '*' : _inputText[i];
            if (!_font.Glyphs.TryGetValue(c, out var glyph)) {
                continue;
            }

            if (i < _inputText.Length - 1) {
                var next = password ? '*' : _inputText[i + 1];
                _font.Kernings.TryGetValue((c, next), out var kern);
                x += kern * _fontScale;
            }

            x += glyph.Advance * _fontScale;
        }

        return x;
    }

    protected override bool CustomHitbox(Vector2i pos) {
        return pos.X > 0 && pos.X < _textBox.Width && pos.Y > 0 && pos.Y < _textBox.Height;
    }

    private void OnSelectionDown(MouseEvent args) {
        if (ActiveInput != this && _clickActivate) {
            ActiveInput?.UnFocus();
            Focus();
        }

        if (ActiveInput != this) {
            return;
        }

        var position = GetPositionAtX(GetLocalMousePosition().X);
        if (args.ShiftKey) {
            if (_selectionAnchor < 0) {
                _selectionAnchor = GetCaretPosition();
            }
        } else {
            _selectionAnchor = position;
        }
        _selectionEnd = position;
        SetCaretPosition(position);
        ShowCaretNow(!HasSelection());
        FillData();

        _mouseSelecting = true;
        Stage.AddEventListener(MouseEvent.MouseMove, OnSelectionMove, true);
        Stage.AddEventListener(MouseEvent.LeftUp, OnSelectionUp, true);
    }

    private void OnSelectionMove(MouseEvent args) {
        if (!_mouseSelecting) {
            return;
        }

        var localX = GetLocalMousePosition().X;
        var position = localX <= 0
            ? 0
            : localX >= _textBox.Width
                ? _inputText.Length
                : GetPositionAtX(localX);

        _selectionEnd = position;
        SetCaretPosition(position);
        ShowCaretNow(!HasSelection());
        FillData();
    }

    private void OnSelectionUp(MouseEvent args) {
        if (!_mouseSelecting) {
            return;
        }

        StopMouseSelection();
        if (!HasSelection()) {
            ClearSelection();
        }
    }

    private void StopMouseSelection() {
        _mouseSelecting = false;
        if (Stage is null) {
            return;
        }

        Stage.RemoveEventListener(MouseEvent.MouseMove, OnSelectionMove, true);
        Stage.RemoveEventListener(MouseEvent.LeftUp, OnSelectionUp, true);
    }

    private int GetPositionAtX(int x) {
        if (_inputText.Length == 0 || x <= CutX * 3) {
            return _startIndex;
        }

        var password = _password && !_isDefaultText;
        for (var i = _startIndex; i < _endIndex; i++) {
            var left = GetTextX(_startIndex, i, password);
            var right = GetTextX(_startIndex, i + 1, password);
            if (x < (left + right) * 0.5f) {
                return i;
            }
        }

        return _endIndex;
    }

    private int GetCaretPosition() {
        return _caretIndex < 0 ? _inputText.Length : _caretIndex;
    }

    private void SetCaretPosition(int position) {
        position = Math.Clamp(position, 0, _inputText.Length);
        _caretIndex = position == _inputText.Length ? -1 : position;
    }

    private bool HasSelection() {
        return _selectionAnchor >= 0 && _selectionEnd >= 0 && _selectionAnchor != _selectionEnd;
    }

    private (int, int) GetSelectionRange() {
        return (Math.Min(_selectionAnchor, _selectionEnd), Math.Max(_selectionAnchor, _selectionEnd));
    }

    private void ClearSelection() {
        _selectionAnchor = -1;
        _selectionEnd = -1;
        _selectionHighlight.Visible = false;
    }

    private void SelectAll() {
        if (_inputText.Length == 0) {
            return;
        }

        _selectionAnchor = 0;
        _selectionEnd = _inputText.Length;
        SetCaretPosition(_selectionEnd);
        ShowCaretNow(false);
        FillData();
    }

    private void MoveCaret(int position, bool extendSelection) {
        var previous = GetCaretPosition();
        position = Math.Clamp(position, 0, _inputText.Length);
        if (extendSelection) {
            if (_selectionAnchor < 0) {
                _selectionAnchor = previous;
            }
            _selectionEnd = position;
            if (_selectionAnchor == _selectionEnd) {
                ClearSelection();
            }
        } else {
            ClearSelection();
        }

        SetCaretPosition(position);
        ShowCaretNow(!HasSelection());
        FillData();
    }

    private int FindWordLeft(int position) {
        while (position > 0 && char.IsWhiteSpace(_inputText[position - 1])) {
            position--;
        }

        while (position > 0 && !char.IsWhiteSpace(_inputText[position - 1])) {
            position--;
        }
        return position;
    }

    private int FindWordRight(int position) {
        while (position < _inputText.Length && char.IsWhiteSpace(_inputText[position])) {
            position++;
        }

        while (position < _inputText.Length && !char.IsWhiteSpace(_inputText[position])) {
            position++;
        }
        return position;
    }
    
    internal void OnManualTextInput(Key key) {
        var ctrl = Stage.Keyboard.IsCtrlDown();
        var shift = Stage.Keyboard.IsShiftDown();

        switch (key) {
            case Key.A when ctrl:
                SelectAll();
                break;
            case Key.C when ctrl:
                CopySelection();
                break;
            case Key.X when ctrl:
                CutSelection();
                break;
            case Key.V when ctrl:
                PasteClipboard();
                break;
            case Key.Z when ctrl && !shift:
                Undo();
                break;
            case Key.Y when ctrl:
            case Key.Z when ctrl && shift:
                Redo();
                break;
            case Key.Backspace when _inputText.Length > 0:
                DeleteBackward(ctrl);
                break;
            case Key.Delete when _inputText.Length > 0:
                DeleteForward(ctrl);
                break;
            case Key.LeftArrow:
                if (HasSelection() && !shift) {
                    var (start, _) = GetSelectionRange();
                    MoveCaret(start, false);
                    break;
                }

                var left = GetCaretPosition();
                left = ctrl ? FindWordLeft(left) : left - 1;
                MoveCaret(left, shift);
                break;
            case Key.RightArrow:
                if (HasSelection() && !shift) {
                    var (_, end) = GetSelectionRange();
                    MoveCaret(end, false);
                    break;
                }

                var right = GetCaretPosition();
                right = ctrl ? FindWordRight(right) : right + 1;
                MoveCaret(right, shift);
                break;
            case Key.Home:
                MoveCaret(0, shift);
                break;
            case Key.End:
                MoveCaret(_inputText.Length, shift);
                break;
        }
    }

    internal void OnTextInput(ReadOnlySpan<char> text) {
        if (text.Length != 1) {
            return;
        }

        if (!CanAddChar(text[0])) {
            return;
        }
        var selectedLength = 0;
        if (HasSelection()) {
            var (start, end) = GetSelectionRange();
            selectedLength = end - start;
        }

        if (_inputText.Length - selectedLength >= _maxCharacters) {
            return;
        }

        RecordUndo();
        DeleteSelection(false);
        AddChar(text[0]);
        FinishEdit();
    }

    private bool CanAddChar(char input) {
        if (char.IsControl(input)) {
            return false;
        }

        if (char.IsWhiteSpace(input) && input != ' ') {
            return false;
        }
        return _font.Glyphs.ContainsKey(input);
    }

    private void AddChar(char input) {
        if (_inputText.Length == _maxCharacters) {
            return;
        }

        var position = GetCaretPosition();
        _inputText.Insert(position, input);
        SetCaretPosition(position + 1);
    }

    private void CopySelection() {
        if (_password || !HasSelection()) {
            return;
        }

        var (start, end) = GetSelectionRange();
        Toolkit.Clipboard.SetClipboardText(_inputText.ToString(start, end - start));
    }

    private void CutSelection() {
        if (_password || !HasSelection()) {
            return;
        }

        CopySelection();
        RecordUndo();
        DeleteSelection(false);
        FinishEdit();
    }

    private void PasteClipboard() {
        if (Toolkit.Clipboard.GetClipboardFormat() != ClipboardFormat.Text) {
            return;
        }

        var clipboardText = Toolkit.Clipboard.GetClipboardText();
        if (string.IsNullOrEmpty(clipboardText)) {
            return;
        }

        var replacementLength = HasSelection() ? GetSelectionRange().Item2 - GetSelectionRange().Item1 : 0;
        var available = _maxCharacters - (_inputText.Length - replacementLength);
        if (available <= 0) {
            return;
        }

        var filtered = new StringBuilder(Math.Min(clipboardText.Length, available));
        foreach (var input in clipboardText) {
            if (!CanAddChar(input)) {
                continue;
            }
            filtered.Append(input);
            if (filtered.Length == available) {
                break;
            }
        }

        if (filtered.Length == 0) {
            return;
        }

        RecordUndo();
        DeleteSelection(false);
        var position = GetCaretPosition();
        _inputText.Insert(position, filtered);
        SetCaretPosition(position + filtered.Length);
        FinishEdit();
    }

    private void DeleteBackward(bool byWord) {
        if (HasSelection()) {
            RecordUndo();
            DeleteSelection(false);
            FinishEdit();
            return;
        }

        var end = GetCaretPosition();
        if (end == 0) {
            return;
        }

        var start = byWord ? FindWordLeft(end) : end - 1;
        RecordUndo();
        _inputText.Remove(start, end - start);
        SetCaretPosition(start);
        FinishEdit();
    }

    private void DeleteForward(bool byWord) {
        if (HasSelection()) {
            RecordUndo();
            DeleteSelection(false);
            FinishEdit();
            return;
        }

        var start = GetCaretPosition();
        if (start == _inputText.Length) {
            return;
        }

        var end = byWord ? FindWordRight(start) : start + 1;
        RecordUndo();
        _inputText.Remove(start, end - start);
        SetCaretPosition(start);
        FinishEdit();
    }

    private bool DeleteSelection(bool recordUndo) {
        if (!HasSelection()) {
            return false;
        }

        if (recordUndo) {
            RecordUndo();
        }
        var (start, end) = GetSelectionRange();
        _inputText.Remove(start, end - start);
        ClearSelection();
        SetCaretPosition(start);
        return true;
    }

    private void RecordUndo() {
        _undo.Push(new EditState { Text = _inputText.ToString(), Caret = GetCaretPosition() });
        while (_undo.Count > 128) {
            var states = _undo.ToArray();
            _undo.Clear();
            for (var i = states.Length - 2; i >= 0; i--) {
                _undo.Push(states[i]);
            }
        }
        _redo.Clear();
    }

    private void Undo() {
        if (_undo.Count == 0) {
            return;
        }

        _redo.Push(new EditState { Text = _inputText.ToString(), Caret = GetCaretPosition() });
        ApplyEditState(_undo.Pop());
    }

    private void Redo() {
        if (_redo.Count == 0) {
            return;
        }

        _undo.Push(new EditState { Text = _inputText.ToString(), Caret = GetCaretPosition() });
        ApplyEditState(_redo.Pop());
    }

    private void ApplyEditState(EditState state) {
        _inputText.Clear();
        _inputText.Append(state.Text);
        _isDefaultText = false;
        ClearSelection();
        SetCaretPosition(state.Caret);
        FinishEdit();
    }

    private void FinishEdit() {
        ClearSelection();
        ShowCaretNow(true);
        FillData();
        _onChange?.Invoke();
    }

    public bool HasText(bool ignoreWhitespace) {
        if (ignoreWhitespace) {
            return !string.IsNullOrWhiteSpace(_inputText.ToString());
        }
        
        return _inputText.Length > 0;
    }

    public void Focus() {
        if (ActiveInput != this) {
            ActiveInput?.UnFocus();
            ActiveInput = this;
        }
        
        _isCaretActive = true;
        ClearSelection();
        ShowCaretNow(true);
        _caretIndex = -1;
        _onFocus?.Invoke();

        ClearIfDefault();
        
        AddEventListener(Event.EnterFrame, OnFrameEnter);
        
        FillData();
    }

    public void UnFocus(bool clearText = false) {
        StopMouseSelection();
        ActiveInput = null;
        _isCaretActive = false;
        _caretIndex = -1;
        ClearSelection();
        _caret.Visible = false;
        _onUnfocus?.Invoke();

        if (clearText) {
            _inputText.Clear();
        }
        
        if (_inputText.Length == 0) {
            SetDefault();
        }
        
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
        
        FillData();
    }
    
    public void InsertText(string text) {
        ClearIfDefault();
        
        if (_caretIndex == -1) {
            _inputText.Append(text);
        } else {
            _inputText.Insert(_caretIndex, text);
            _caretIndex += text.Length;
        }
        
        FillData();
    }

    public void SetText(string text) {
        if (text == string.Empty) {
            SetDefault();
            return;
        }

        ClearIfDefault();
        _inputText.Append(text);
        FillData();
    }

    private void ClearIfDefault() {
        if (!_isDefaultText) {
            return;
        }
        
        _inputText.Clear();
        _isDefaultText = false;
    }

    private void SetDefault() {
        _inputText.Clear();
        _inputText.Append(_defaultText);
        _isDefaultText = true;
    }
}
