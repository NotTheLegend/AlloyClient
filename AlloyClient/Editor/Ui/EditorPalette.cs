using System;
using System.Collections.Generic;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Assets.Libraries;
using AlloyClient.Display;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.Utils;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorPalette : Container {
    public const int PanelWidth = 180;
    private const int ElementSize = 42;
    private const int ElementGap = 1;
    private const int ElementInset = 4;
    private const int Columns = 4;
    private const int ListTop = 40;
    private readonly CutEdgeRect _background;
    private readonly Container _elements;
    private readonly Action<int> _selected;
    private readonly TextInput _search;
    private EditorDrawType _drawType;
    private List<EditorCatalogEntry> _entries = [];
    private int _height;
    private int _scrollRows;
    private int _selectedType;
    private EditorPaletteTooltip _tooltip;

    public EditorPalette(int height, Action<int> selected, Action<bool> editing)
        : base(new ContainerConfig { Width = PanelWidth, Height = height, EnableClip = true }) {
        _height = height;
        _selected = selected;

        _background = new CutEdgeRect(new CutEdgeConfig {
            Width = PanelWidth, Height = height, CutX = 6, CutY = 6,
            Color = 0x565656, Alpha = 0.8f, MouseEnabled = true,
        });

        AddChild(_background);

        _search = new TextInput(new InputConfig {
            X = 5, Y = 8, Width = PanelWidth - 10, FontSize = 17, FontType = FontType.Bold, DefaultText = string.Empty,
            OnFocus = () => editing(true), OnUnfocus = () => editing(false), OnChange = ApplySearch,
        });

        AddChild(_search);

        _elements = new Container(new ContainerConfig {
            X = 0, Y = ListTop, Width = PanelWidth, Height = height - ListTop, EnableClip = true,
        });

        AddChild(_elements);

        MouseEnabled = true;
        AddEventListener(MouseEvent.ScrollVertical, OnScroll);
        SetDrawType(EditorDrawType.Ground, -1);
    }

    public void ResizePanel(int height) {
        _height = height;
        Resize(PanelWidth, height);
        _background.Resize(PanelWidth, height);
        _elements.Resize(PanelWidth, Math.Max(1, height - ListTop));
        ClampScroll();
        Rebuild();
    }

    public void SetDrawType(EditorDrawType drawType, int selectedType) {
        ClearTooltip();
        _drawType = drawType;
        _selectedType = selectedType;
        _search.SetText(string.Empty);
        _entries = EditorCatalog.GetEntries(drawType);
        _scrollRows = 0;
        Rebuild();
    }

    public void SetSelectedType(int selectedType) {
        _selectedType = selectedType;
        Rebuild();
    }

    private void OnScroll(MouseEvent args) {
        _scrollRows += args.VerticalDelta > 0 ? -1 : 1;
        ClampScroll();
        Rebuild();
    }

    private void ClampScroll() {
        var visibleRows = Math.Max(1, (_height - ListTop) / (ElementSize + ElementGap));
        var totalRows = (_entries.Count + Columns - 1) / Columns;
        _scrollRows = Math.Clamp(_scrollRows, 0, Math.Max(0, totalRows - visibleRows));
    }

    private void ApplySearch() {
        var source = EditorCatalog.GetEntries(_drawType);
        var query = _search.Text.Trim();
        _entries = string.IsNullOrEmpty(query)
            ? source
            : source.FindAll(entry => entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                                      || entry.Id.Contains(query, StringComparison.OrdinalIgnoreCase));

        _scrollRows = 0;
        Rebuild();
    }

    private void Rebuild() {
        ClearTooltip();
        _elements.RemoveChildren();
        
        var first = _scrollRows * Columns;
        var visibleRows = Math.Max(1, (_height - ListTop) / (ElementSize + ElementGap) + 1);
        var end = Math.Min(_entries.Count, first + visibleRows * Columns);
        for (var i = first; i < end; i++) {
            var entry = _entries[i];
            var local = i - first;
            var x = ElementInset + local % Columns * (ElementSize + ElementGap);
            var y = ElementInset + local / Columns * (ElementSize + ElementGap);
            var cell = new Container(new ContainerConfig { X = x, Y = y, Width = ElementSize, Height = ElementSize });
            cell.MouseEnabled = true;
            var selected = entry.Type == _selectedType;
            var hit = new CutEdgeRect(new CutEdgeConfig {
                Width = ElementSize, Height = ElementSize, CutX = 3, CutY = 3,
                Color = selected ? 0x777777u : 0x333333u,
                Alpha = selected ? 0.75f : 0.18f,
            });

            cell.AddChild(hit);
            AddIcon(cell, entry);
            
            if (selected) {
                AddSelectionOutline(cell);
            }
            
            EditorPaletteTooltip cellTooltip = null;
            cell.AddEventListener(MouseEvent.MouseOver, () => {
                hit.Alpha = selected ? 0.9f : 0.5f;
                if (_drawType is not (EditorDrawType.Ground or EditorDrawType.Objects)) {
                    return;
                }

                ClearTooltip();
                cellTooltip = new EditorPaletteTooltip(_drawType, entry);
                _tooltip = cellTooltip;
                TooltipManager.AddTooltip(cellTooltip);
            });

            cell.AddEventListener(MouseEvent.MouseOut, () => {
                hit.Alpha = selected ? 0.75f : 0.18f;
                ClearTooltip(cellTooltip);
                cellTooltip = null;
            });

            cell.AddEventListener(MouseEvent.LeftClick, () => {
                _selectedType = entry.Type;
                _selected(entry.Type);
                Rebuild();
            });

            _elements.AddChild(cell);
        }
    }

    private void ClearTooltip(EditorPaletteTooltip expected = null) {
        if (_tooltip is null) {
            return;
        }
        
        if (expected is not null && _tooltip != expected) {
            return;
        }

        TooltipManager.RemoveTooltip(_tooltip);
        _tooltip = null;
    }

    private static void AddSelectionOutline(Container cell) {
        cell.AddChild(new CutCornerOutline(ElementSize, ElementSize, 0xFFFFFF, 0.9f));
    }

    private void AddIcon(Container cell, EditorCatalogEntry entry) {
        if (_drawType == EditorDrawType.Regions) {
            var color = unchecked((uint)(entry.Type * 2654435761));
            cell.AddChild(new CutEdgeRect(new CutEdgeConfig {
                Width = ElementSize, Height = ElementSize, CutX = 3, CutY = 3,
                Color = 0x505050u | color & 0xAFAFAFu,
            }));

            return;
        }

        var found = _drawType == EditorDrawType.Ground
            ? GroundLibrary.TypeToTextureData.TryGetValue((ushort)entry.Type, out var texture)
            : ObjectLibrary.TypeToTextureData.TryGetValue((ushort)entry.Type, out texture);

        if (!found || texture is null) {
            return;
        }

        var atlas = _drawType == EditorDrawType.Objects && texture.EditorTexture.HasValue
            ? texture.EditorTexture.Value
            : texture.GetTexture();

        cell.AddChild(new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.Create(atlas, TextureType.GameAtlas), Width = ElementSize, Height = ElementSize,
            OutlineEnabled = false, GlowEnabled = false,
        }));
    }
}