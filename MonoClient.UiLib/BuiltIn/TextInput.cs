using System;
using System.Text;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;

namespace MonoClient.UiLib.BuiltIn;

public struct InputConfig {
    public int X = 0;
    public int Y = 0;
    public float FontSize = 10;
    public bool Bold = false;
    public uint Color = 0xFFFFFF;
    public uint OutlineColor = 0x0;
    public uint OutlineThickness = 4;
    public int Width = 100;
    public string DefaultText = "";
    public byte MaxCharacters = byte.MaxValue;
    public bool Password = false;
    public bool ClickToActivate = true;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    
    public Action OnChange = null;
    
    public InputConfig() { }
}

public sealed class TextInput : Sprite {

    private const int CutX = 2;
    private const int CutY = 2;

    internal static TextInput ActiveInput;
    internal static bool UnFocusOnClick = false;
    
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

    private readonly NineSliceRect _textBox;
    private readonly SimpleText _caret;
    private int _caretIndex = -1;
    private bool _isCaretActive = false;
    private double _lastCaretUpdateTime;
    private int _startIndex;

    private IntVector2 _mousePosition;

    public TextInput(InputConfig config) {
        X = config.X;
        Y = config.Y;
        _fontScale = config.FontSize;
        _font = UiRender.GetFont(config.Bold);
        SetColor(config.Color);
        SetColorSecondary(config.OutlineColor);
        _outlineThickness = _font.ValidateOutlineSize(config.OutlineThickness);
        _width = config.Width;
        _defaultText = config.DefaultText;
        _maxCharacters = config.MaxCharacters;
        _password = config.Password;
        _clickActivate = config.ClickToActivate;
        SetAnchor(config.Anchor);

        MouseEnabled = true;
        TextureId = config.Bold ? TextureType.TextBold : TextureType.TextNormal;
        Extra1.X = _outlineThickness;

        _inputText.Append(config.DefaultText);
        
        var caretConfig = new TextConfig { Text = "|", FontSize = config.FontSize, Bold = config.Bold, Color = config.Color, OutlineColor = config.OutlineColor, OutlineThickness = (int)_outlineThickness };
        _caret = new SimpleText(caretConfig);
        _caret.Visible = false;
        AddChild(_caret);
        
        var rectConfig = new NineSliceConfig { Width = _width, Height = (int)(_font.LineHeight * _fontScale) + CutY * 3, SliceData = "textBox", CutX = CutX, CutY = CutY};
        _textBox = new NineSliceRect(rectConfig);
        AddChild(_textBox);
        
        SetBaseDimensions(_textBox.Width, _textBox.Height);
        SetHitboxType(HitboxType.Custom);
        
        AddEventListener(MouseEventId.LeftClick, OnMouseClick);
        
        ResizeBackBuffer();
        FillData();
    }
    
    protected override void ResizeBackBuffer() {
        var size = _maxCharacters + 1;
        VertexData = new VertexUi[size * 4];
        Indices = new short[size * 6];
        for (var i = 0; i < Indices.Length / 6; i++) {
            var idx6 = i * 6;
            var idx4 = i * 4;

            Indices[idx6] = (short)(0 + idx4);
            Indices[idx6 + 1] = (short)(1 + idx4);
            Indices[idx6 + 2] = (short)(2 + idx4);
            Indices[idx6 + 3] = (short)(0 + idx4);
            Indices[idx6 + 4] = (short)(2 + idx4);
            Indices[idx6 + 5] = (short)(3 + idx4);
        }
        base.ResizeBackBuffer();
    }
    
    protected override void OnUpdate(GameTime gameTime) {
        if (!_isCaretActive) return;
        if (gameTime.TotalGameTime.TotalMilliseconds - _lastCaretUpdateTime < 500) return;
            
        _lastCaretUpdateTime = gameTime.TotalGameTime.TotalMilliseconds;
        _caret.Visible = !_caret.Visible;
    }

    private void FillData() {
        var startX = CutX * 3;
        var startY = _font.Ascender * _fontScale + CutY * 3;
        var zero = new Vector2(startX, startY);

        var (start, end) = _font.GetStartIndex(_inputText, _caretIndex, _width - startX * 2 - _caret.Width, _outlineThickness, _fontScale);
        _startIndex = start;
        OverridePrimCount = 2;
        
        var idx = 4;
        var len = _inputText.Length;
        var caret = false;

        var password = _password && !_isDefaultText;
        
        for (var i = start; i < end; i++) {

            if (!caret && i == _caretIndex) {
                caret = true;
                i--;
                
                _caret.X = (int)zero.X;
                _caret.Y = CutY * 3;
                zero.X += _caret.Width;
                
                continue;
            }
            
            var c = password ? '*' : _inputText[i];
            switch (c) {
                case '\n':
                case '\r': 
                    continue;
                default:
                    if (!_font.Glyphs.TryGetValue(c, out var glyph)) continue;

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
                    zero.X += Extra1.X / _font.OutlineTexel * _fontScale * 2;
                    idx += 4;
                    OverridePrimCount += 2;
                    continue;
            }
        }

        if (!caret) {
            _caret.X = (int)zero.X;
            _caret.Y = CutY * 3;
        }
    }

    protected override bool CustomHitbox(IntVector2 pos) {
        var hit = pos.X > 0 && pos.X < _textBox.Width && pos.Y > 0 && pos.Y < _textBox.Height;

        UnFocusOnClick = !hit && ActiveInput == this;
        
        _mousePosition = pos;
        return hit;
    }

    private void OnMouseClick(MouseEventArgs args) {
        if (ActiveInput != this && _clickActivate) {
            ActiveInput?.UnFocus();
            Focus();
        }
        
        SetCaretIndex();
    }

    private void SetCaretIndex() {
        var i = 1;// Offset by 1 for rect
        for (var j = _startIndex; j < _inputText.Length; j++) {
            var p1 = VertexData[i * 4 + 1].Position.X;
            var p2 = VertexData[i * 4 + 3].Position.X;
            var half = (p2 - p1) / 2f;

            if (j == _startIndex && _mousePosition.X <= p1) {
                _caretIndex = 0;
            } else if (_mousePosition.X >= p1 && _mousePosition.X < p1 + half) {
                _caretIndex = _startIndex + i - 1;
            } else if (_mousePosition.X <= p2 && _mousePosition.X >= p2 - half) {
                _caretIndex = _startIndex + i;
            } else if (j + 1 == _inputText.Length && _mousePosition.X >= p2) {
                _caretIndex = -1;
            }

            i++;
        }
        
        FillData();
    }

    //Todo: maybe improve this logic, tis a mess
    internal void OnTextInput(TextInputEventArgs e) {
        if (e.Key == Keys.Back && _inputText.Length > 0 && _caretIndex == -1) {
            _inputText.Remove(_inputText.Length - 1, 1);
            FillData();
            return;
        }
        
        if (e.Key == Keys.Back && _inputText.Length > 0 && _caretIndex > 0) {
            _caretIndex--;
            _inputText.Remove(_caretIndex, 1);
            FillData();
            return;
        }
        
        if (e.Key == Keys.Delete && _caretIndex < _inputText.Length && _caretIndex >= 0) {
            _inputText.Remove(_caretIndex, 1);
            FillData();
            if (_caretIndex == _inputText.Length)
                _caretIndex = -1;
            return;
        }

        if (_inputText.Length == _maxCharacters) {
            return;
        }

        if (char.IsWhiteSpace(e.Character) && e.Character != ' ') {
            return;
        }
        
        // TODO: probably add a regex validator or something
        
        if (_caretIndex == -1) {
            _inputText.Append(e.Character);
        }
        else {
            _inputText.Insert(_caretIndex, e.Character);
            _caretIndex++;
        }
        
        FillData();
    }

    public void SetActive() {
        if (ActiveInput != this) {
            ActiveInput?.UnFocus();
            Focus();
        }
        
        SetCaretIndex();
    }
    
    public void Focus() {
        ActiveInput = this;
        _isCaretActive = true;
        _caret.Visible = true;
        _caretIndex = -1;

        if (_isDefaultText) {
            _inputText.Clear();
            _isDefaultText = false;
        }
        
        FillData();
    }

    public void UnFocus() {
        ActiveInput = null;
        _isCaretActive = false;
        _caretIndex = -1;
        _caret.Visible = false;
        
        if (_inputText.Length == 0) {
            _isDefaultText = true;
            _inputText.Append(_defaultText);
        }
        
        FillData();
    }
    
    public void AddText(string text) {
        foreach (char input in text) {
            if (_caretIndex == -1) {
                _inputText.Append(input);
            }
            else {
                _inputText.Insert(_caretIndex, input);
                _caretIndex++;
            }
        }
        FillData();
    }

    public void ClearText() {
        _inputText.Clear();
        _caretIndex = -1;
        FillData();
    }

    public void Clear() {
        ActiveInput = null;
        _isCaretActive = false;
        _caretIndex = -1;
        _caret.Visible = false;
        _inputText.Clear();
        FillData();
    }
}