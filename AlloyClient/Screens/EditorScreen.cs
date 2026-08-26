using System;
using System.Collections.Generic;
using System.IO;
using Alloy.Engine;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using AlloyClient.Display;
using AlloyClient.Editor;
using AlloyClient.Editor.Ui;
using AlloyClient.Game;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Dialogs;
using AlloyClient.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient.Screens;

public sealed class EditorScreen : Screen {
    private const int BaseWidth = Settings.DefaultScreenWidth;
    private const int BaseHeight = Settings.DefaultScreenHeight;
    private const int PaletteWidth = EditorPalette.PanelWidth;
    private const int PaletteMargin = 15;
    private const int ToolbarWidth = 30;
    private const int ToolbarHeight = 220;
    private const int MapSelectorWidth = 150;
    private const int MapSelectorHeight = 150;
    private const int MapTabHeight = 28;
    private const int MapTabGap = 2;
    private const float BaseTileSize = 8f;
    private const float MinZoom = 1f;
    private const float MaxZoom = 1000f;

    private readonly Container _root = new(new ContainerConfig { Width = BaseWidth, Height = BaseHeight });
    private readonly Container _canvas = new(new ContainerConfig { Width = BaseWidth, Height = BaseHeight, EnableClip = true });
    private readonly Container _tileLayer = new(new ContainerConfig { Width = BaseWidth, Height = BaseHeight, EnableClip = true });
    private readonly Container _toolButtons = new(new ContainerConfig { Width = ToolbarWidth, Height = ToolbarHeight });
    private readonly List<EditorSmallButton> _topButtons = [];
    private readonly List<EditorToolButton> _editorToolButtons = [];
    private readonly List<EditorToolboxAction> _drawTypeButtons = [];
    private readonly SimpleText _mapInfo;
    private readonly SimpleText _tileInfo;
    private readonly SimpleText _status;
    private readonly EditorPalette _palette;
    private EditorController _editor;
    private readonly EditorBackdropRenderer _backdropRenderer = new();
    private readonly EditorMapRenderer _mapRenderer = new();
    private ColorRect _canvasHitArea;
    private readonly Container _mapSelector;
    private Container _mapTabs;
    private readonly Container _leftToolbox;
    private EditorToolboxCheckbox _gridCheckbox;
    private EditorToolboxCheckbox _autoSaveCheckbox;
    private EditorSmallButton _backButton;

    private readonly List<EditorMapDocument> _documents = [];
    private readonly List<EditorMapTab> _mapTabRows = [];
    private EditorMapDocument _activeDocument;
    private int _nextMapId;
    private int _mapTabScroll;
    private float _zoom = 100f;
    private float _panX;
    private float _panY;
    private int _screenWidth = BaseWidth;
    private int _screenHeight = BaseHeight;
    private int _mouseTileX = -1;
    private int _mouseTileY = -1;
    private bool _leftDrawing;
    private bool _selectionMoving;
    private bool _middlePanning;
    private bool _controlPanning;
    private Vector2i _lastPanMouse;
    private EditorToolType? _toolBeforeShiftDrag;
    private bool _grid = true;
    private bool _autoSave = true;
    private bool _textEditing;
    private bool _mapOpen;
    private bool _mapRenderDirty = true;
    private readonly int[,] _tileHotkeys = new int[3, 10];
    private double _lastAutoSave;
    private EditorPrompt _prompt;

    public EditorScreen() {
        EditorCatalog.Load();
        _editor = new EditorController(new EditorMapData("untitled", 64, 64));
        AttachEditorCallbacks(_editor);

        AddChild(_root);
        BuildCanvas();
        BuildTopBar();
        _mapSelector = BuildMapSelector();
        _leftToolbox = BuildLeftToolbox();
        BuildToolBar();

        for (var draw = 0; draw < 3; draw++)
        for (var slot = 0; slot < 10; slot++)
            _tileHotkeys[draw, slot] = draw == 0 ? -1 : 0;

        _palette = new EditorPalette(BaseHeight - 80, type => {
            _editor.Brush.SetSelectedType(type);
            RefreshStatus();
        }, editing => _textEditing = editing) { X = BaseWidth - PaletteWidth - PaletteMargin, Y = 50 };
        _root.AddChild(_palette);
        _mapInfo = AddLabel(15, BaseHeight - 15, UiAnchor.LeftBottom);
        _tileInfo = AddLabel(BaseWidth / 2, BaseHeight - 15, UiAnchor.MiddleBottom);
        _status = AddLabel(BaseWidth - 15, BaseHeight - 15, UiAnchor.RightBottom);
        _status.Visible = false;

        AddEventListener(Event.AddedToStage, OnAdded);
        AddEventListener(Event.RemovedFromStage, OnRemoved);
        BuildVisibleTiles();
        _tileLayer.Visible = false;
        SelectDefaultGround();
        RefreshAll();
    }

    private void BuildTopBar() {
        var x = 15;
        x = AddTopButton("Load", x, LoadMap);
        x = AddTopButton("New", x, NewMap);
        x = AddTopButton("Save JSON", x, () => SaveMap(false));
        x = AddTopButton("Save Wmap", x, () => SaveMap(true));
        AddTopButton("Test", x, () => ShowStatus("Map test mode is not available yet."));
        _backButton = new EditorSmallButton("Back", BackToTitle) { Y = 15 };
        _root.AddChild(_backButton);
    }

    private void BuildCanvas() {
        _canvasHitArea = new ColorRect(new ColorRectConfig {
            Width = BaseWidth, Height = BaseHeight, Color = 0x111111, Alpha = 0f, MouseEnabled = true
        });
        _canvas.AddChild(_canvasHitArea);
        _canvas.AddChild(_tileLayer);
        _root.AddChild(_canvas);
        _canvas.AddEventListener(MouseEvent.LeftDown, OnCanvasLeftDown);
        _canvas.AddEventListener(MouseEvent.MiddleDown, OnCanvasMiddleDown);
        _canvas.AddEventListener(MouseEvent.ScrollVertical, OnCanvasScroll);
    }

    private void BuildToolBar() {
        _toolButtons.AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = ToolbarWidth, Height = ToolbarHeight, CutX = 5, CutY = 5,
            Color = 0x565656, Alpha = 0.8f
        }));
        _root.AddChild(_toolButtons);
        var tools = new[] {
            (EditorToolType.Select, 0), (EditorToolType.Pencil, 1), (EditorToolType.Line, 6),
            (EditorToolType.Shape, 7), (EditorToolType.Bucket, 5), (EditorToolType.Picker, 3),
            (EditorToolType.Eraser, 2), (EditorToolType.Edit, 9)
        };
        for (var i = 0; i < tools.Length; i++) {
            var tool = tools[i];
            var button = new EditorToolButton(tool.Item1, tool.Item2, 6 + i * 26, SelectTool);
            button.SetSelected(tool.Item1 == _editor.Tool);
            _editorToolButtons.Add(button);
            _toolButtons.AddChild(button);
        }

    }

    private Container BuildMapSelector() {
        var selector = new Container(new ContainerConfig {
            X = 15, Y = 50, Width = MapSelectorWidth, Height = MapSelectorHeight, EnableClip = true
        });
        selector.AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = MapSelectorWidth, Height = MapSelectorHeight, CutX = 6, CutY = 6,
            Color = 0x565656, Alpha = 0.8f, MouseEnabled = true
        }));
        _mapTabs = new Container(new ContainerConfig { Width = MapSelectorWidth, Height = 1 });
        _mapTabs.MouseEnabled = true;
        selector.AddChild(_mapTabs);
        selector.MouseEnabled = true;
        selector.AddEventListener(MouseEvent.ScrollVertical, OnMapSelectorScroll);
        _root.AddChild(selector);
        return selector;
    }

    private void AttachEditorCallbacks(EditorController editor) {
        editor.Changed = OnEditorChanged;
        editor.Status = ShowStatus;
    }

    private EditorMapDocument AddDocument(EditorMapData map) {
        var document = new EditorMapDocument(_nextMapId++, map);
        AttachEditorCallbacks(document.Controller);
        _documents.Add(document);
        ActivateDocument(document);
        return document;
    }

    private void ActivateDocument(EditorMapDocument document) {
        if (ReferenceEquals(_activeDocument, document)) return;

        SaveActiveViewState();
        _activeDocument = document;
        _editor = document.Controller;
        _panX = document.PanX;
        _panY = document.PanY;
        _zoom = Math.Clamp(document.Zoom, MinZoom, MaxZoom);
        _grid = document.Grid;
        _mapOpen = true;
        _tileLayer.Visible = true;
        _mapRenderDirty = true;
        _mouseTileX = -1;
        _mouseTileY = -1;

        foreach (var button in _drawTypeButtons)
            button.SetSelected(button.DrawType == _editor.Brush.DrawType);
        _gridCheckbox.SetChecked(_grid);
        _palette.SetDrawType(_editor.Brush.DrawType, _editor.Brush.GetSelectedType());
        ClampOffset();
        UpdateMapSelector();
        RefreshAll();
    }

    private void SaveActiveViewState() {
        if (_activeDocument is null) return;
        _activeDocument.PanX = _panX;
        _activeDocument.PanY = _panY;
        _activeDocument.Zoom = _zoom;
        _activeDocument.Grid = _grid;
    }

    private void OnMapSelectorScroll(MouseEvent args) {
        var contentHeight = _documents.Count * (MapTabHeight + MapTabGap);
        var minimum = Math.Min(0, MapSelectorHeight - contentHeight);
        _mapTabScroll = Math.Clamp(
            _mapTabScroll + (args.VerticalDelta > 0 ? 12 : -12),
            minimum,
            0
        );
        _mapTabs.Y = _mapTabScroll;
        args.StopPropagation();
    }

    private void RebuildMapTabs() {
        _mapTabs.RemoveChildren();
        _mapTabRows.Clear();
        for (var i = 0; i < _documents.Count; i++) {
            var document = _documents[i];
            var tab = new EditorMapTab(
                document.Id,
                $"{document.Id}. {document.Map.Name}{(document.Map.SavedChanges ? string.Empty : " *")}",
                () => ActivateDocument(document),
                () => RequestCloseDocument(document)
            ) {
                Y = i * (MapTabHeight + MapTabGap)
            };
            tab.SetSelected(ReferenceEquals(document, _activeDocument));
            _mapTabs.AddChild(tab);
            _mapTabRows.Add(tab);
        }

        _mapTabs.Resize(MapSelectorWidth, Math.Max(1, _documents.Count * (MapTabHeight + MapTabGap)));
        var minimum = Math.Min(0, MapSelectorHeight - _mapTabs.Height);
        _mapTabScroll = Math.Clamp(_mapTabScroll, minimum, 0);
        _mapTabs.Y = _mapTabScroll;
    }

    private Container BuildLeftToolbox() {
        var box = new Container(new ContainerConfig { X = 15, Width = 120, Height = 194 });
        box.AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = 120, Height = 194, CutX = 6, CutY = 6,
            Color = 0x565656, Alpha = 0.8f, MouseEnabled = true
        }));
        box.AddChild(new SimpleText(new TextConfig {
            Text = "Settings", FontSize = 18, FontType = FontType.Bold,
            X = 60, Y = 14, Anchor = UiAnchor.Middle, Color = 0xFFFFFF
        }));
        box.AddChild(new ColorRect(new ColorRectConfig {
            X = 7, Y = 28, Width = 106, Height = 1, Color = 0xFFFFFF, Alpha = 0.45f
        }));
        _gridCheckbox = new EditorToolboxCheckbox("Grid", 36, _grid, ToggleGrid) { X = 7 };
        _autoSaveCheckbox = new EditorToolboxCheckbox("Autosave", 68, _autoSave, ToggleAutoSave) { X = 7 };
        box.AddChild(_gridCheckbox);
        box.AddChild(_autoSaveCheckbox);
        AddDrawTypeButton(box, "Ground", EditorDrawType.Ground, 104);
        AddDrawTypeButton(box, "Objects", EditorDrawType.Objects, 132);
        AddDrawTypeButton(box, "Regions", EditorDrawType.Regions, 160);
        _root.AddChild(box);
        return box;
    }

    private void AddDrawTypeButton(Container box, string text, EditorDrawType drawType, int y) {
        var button = new EditorToolboxAction(text, drawType, y, SelectDrawType) { X = 7 };
        button.SetSelected(_editor.Brush.DrawType == drawType);
        _drawTypeButtons.Add(button);
        box.AddChild(button);
    }

    private void OnAdded() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        Stage.AddEventListener(MouseEvent.MouseMove, OnMouseMove, true);
        Stage.AddEventListener(MouseEvent.LeftUp, OnLeftUp, true);
        Stage.AddEventListener(MouseEvent.MiddleUp, OnMiddleUp, true);
        Stage.AddEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        OnResize(new ResizeEvent(ResizeEvent.Resize, Stage.StageWidth, Stage.StageHeight));
    }

    private void OnRemoved() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
        Stage.RemoveEventListener(MouseEvent.MouseMove, OnMouseMove, true);
        Stage.RemoveEventListener(MouseEvent.LeftUp, OnLeftUp, true);
        Stage.RemoveEventListener(MouseEvent.MiddleUp, OnMiddleUp, true);
        Stage.RemoveEventListener(KeyboardEvent.KeyDown, OnKeyDown);
    }

    protected override void OnResize(ResizeEvent args) {
        _screenWidth = Math.Max(1, args.Width);
        _screenHeight = Math.Max(1, args.Height);
        _backdropRenderer.Resize(_screenWidth, _screenHeight);
        _root.Scale = Vector2.One;
        _root.X = 0;
        _root.Y = 0;
        _root.Resize(_screenWidth, _screenHeight);
        _canvas.Resize(_screenWidth, _screenHeight);
        _canvasHitArea.Resize(_screenWidth, _screenHeight);
        _tileLayer.Resize(_screenWidth, _screenHeight);
        _backButton.X = _screenWidth - _backButton.Width - 10;
        _palette.X = _screenWidth - PaletteWidth - PaletteMargin;
        _palette.Y = _backButton.Y + _backButton.Height + 15;
        _palette.ResizePanel(Math.Max(80, _screenHeight - 80));
        _toolButtons.X = _palette.X - ToolbarWidth - 8;
        _toolButtons.Y = (_screenHeight - ToolbarHeight) / 2;
        _mapSelector.X = 15;
        _mapSelector.Y = 15 + (_topButtons.Count == 0 ? 25 : _topButtons[0].Height) + 10;
        _leftToolbox.X = 15;
        _leftToolbox.Y = (_screenHeight - _leftToolbox.Height) / 2;
        _mapInfo.X = 15;
        _mapInfo.Y = _screenHeight - 15;
        _tileInfo.X = _screenWidth / 2;
        _tileInfo.Y = _screenHeight - 15;
        _status.X = _screenWidth - 15;
        _status.Y = _screenHeight - 15;
        BuildVisibleTiles();
        RefreshAll();
    }

    private void OnCanvasLeftDown(MouseEvent args) {
        if (args.ShiftKey && _editor.Tool != EditorToolType.Select) {
            _toolBeforeShiftDrag = _editor.Tool;
            SelectTool(EditorToolType.Select);
        } else if (args.CtrlKey) {
            _controlPanning = true;
            _lastPanMouse = ToLogical(args.Coords);
            return;
        }

        if (!TryGetMapTile(args.Coords, out var x, out var y)) return;
        if (_editor.Tool == EditorToolType.Edit) {
            ShowObjectNamePrompt(x, y);
            return;
        }
        if (_editor.Tool == EditorToolType.Select && _editor.BeginSelectionMove(x, y)) {
            _selectionMoving = true;
            return;
        }
        _leftDrawing = true;
        _editor.Begin(x, y);
        _mapRenderDirty = true;
        RefreshAll();
    }

    private void OnCanvasMiddleDown(MouseEvent args) {
        _middlePanning = true;
        _lastPanMouse = ToLogical(args.Coords);
    }

    private void OnMouseMove(MouseEvent args) {
        var previousTileX = _mouseTileX;
        var previousTileY = _mouseTileY;
        if (TryGetMapTile(args.Coords, out var x, out var y)) {
            _mouseTileX = x;
            _mouseTileY = y;
            if (_selectionMoving) {
                _editor.DragSelectionMove(x, y);
                _mapRenderDirty = true;
                RefreshCanvas();
            } else if (_leftDrawing) {
                _editor.Drag(x, y);
                _mapRenderDirty = true;
                RefreshCanvas();
            }
        } else {
            _mouseTileX = -1;
            _mouseTileY = -1;
        }

        if (_middlePanning || _controlPanning) {
            var logical = ToLogical(args.Coords);
            var dx = logical.X - _lastPanMouse.X;
            var dy = logical.Y - _lastPanMouse.Y;
            if (dx != 0 || dy != 0) {
                _panX += dx;
                _panY += dy;
                ClampOffset();
                SaveActiveViewState();
                _lastPanMouse = logical;
                RefreshCanvas();
            }
        }
        if (!_leftDrawing && !_middlePanning && !_controlPanning
            && (previousTileX != _mouseTileX || previousTileY != _mouseTileY))
            RefreshMapOverlays();
        RefreshStatus();
    }

    private void OnLeftUp(MouseEvent args) {
        if (_controlPanning) {
            _controlPanning = false;
            RestoreToolAfterShiftDrag();
            return;
        }
        if (_selectionMoving) {
            _selectionMoving = false;
            _editor.EndSelectionMove();
            RestoreToolAfterShiftDrag();
            RefreshAll();
            return;
        }
        if (!_leftDrawing) {
            RestoreToolAfterShiftDrag();
            return;
        }
        _leftDrawing = false;
        if (TryGetMapTile(args.Coords, out var x, out var y)) _editor.End(x, y);
        else _editor.End(_mouseTileX, _mouseTileY);
        RestoreToolAfterShiftDrag();
        RefreshAll();
    }

    private void OnMiddleUp(MouseEvent args) => _middlePanning = false;

    private void RestoreToolAfterShiftDrag() {
        if (!_toolBeforeShiftDrag.HasValue) return;
        var previousTool = _toolBeforeShiftDrag.Value;
        _toolBeforeShiftDrag = null;
        SelectTool(previousTool);
    }

    private void OnCanvasScroll(MouseEvent args) {
        if (args.CtrlKey) {
            _editor.Brush.Size = Math.Clamp(_editor.Brush.Size + (args.VerticalDelta > 0 ? 1 : -1), 0, 12);
            RefreshMapOverlays();
            RefreshStatus();
            return;
        }
        if (!_mapOpen) return;

        var logical = ToLogical(args.Coords);
        GetMapOrigin(out var oldOriginX, out var oldOriginY);
        var oldTileSize = BaseTileSize * _zoom / 100f;
        var mapPixelX = (logical.X - oldOriginX) / oldTileSize;
        var mapPixelY = (logical.Y - oldOriginY) / oldTileSize;
        var step = Math.Max(1f, _zoom * 0.1f);
        var newZoom = Math.Clamp(_zoom + (args.VerticalDelta > 0 ? step : -step), MinZoom, MaxZoom);
        if (Math.Abs(newZoom - _zoom) < 0.001f) return;

        _zoom = newZoom;
        var newScale = _zoom / 100f;
        var baseX = (_screenWidth - _editor.Map.Width * BaseTileSize * newScale) / 2f;
        var baseY = (_screenHeight - _editor.Map.Height * BaseTileSize * newScale) / 2f;
        _panX = logical.X - mapPixelX * BaseTileSize * newScale - baseX;
        _panY = logical.Y - mapPixelY * BaseTileSize * newScale - baseY;
        ClampOffset();
        SaveActiveViewState();
        if (TryGetMapTile(args.Coords, out var tileX, out var tileY)) {
            _mouseTileX = tileX;
            _mouseTileY = tileY;
        }
        RefreshAll();
    }

    private void OnKeyDown(KeyboardEvent args) {
        if (_prompt is not null || _textEditing) return;
        var hotkey = GetNumberKey(args.Key);
        if (hotkey >= 0) {
            if (args.Ctrl) {
                _tileHotkeys[(int)_editor.Brush.DrawType, hotkey] = _editor.Brush.GetSelectedType();
                ShowStatus($"Assigned tile hotkey {hotkey}.");
            } else {
                var type = _tileHotkeys[(int)_editor.Brush.DrawType, hotkey];
                var empty = _editor.Brush.DrawType == EditorDrawType.Ground ? -1 : 0;
                if (type != empty) {
                    _editor.Brush.SetSelectedType(type);
                    _palette.SetSelectedType(type);
                    ShowStatus($"Selected tile hotkey {hotkey}.");
                }
            }
            return;
        }
        if (args.Ctrl) {
            switch (args.Key) {
                case Key.Z: _editor.Undo(); return;
                case Key.Y: _editor.Redo(); return;
                case Key.C: _editor.Copy(); return;
                case Key.V when _mouseTileX >= 0: _editor.Paste(_mouseTileX, _mouseTileY); return;
            }
        }
        switch (args.Key) {
            case Key.S: SelectTool(EditorToolType.Select); break;
            case Key.D: SelectTool(EditorToolType.Pencil); break;
            case Key.L: SelectTool(EditorToolType.Line); break;
            case Key.U: SelectTool(EditorToolType.Shape); break;
            case Key.F: SelectTool(EditorToolType.Bucket); break;
            case Key.A: SelectTool(EditorToolType.Picker); break;
            case Key.E: SelectTool(EditorToolType.Eraser); break;
            case Key.I: SelectTool(EditorToolType.Edit); break;
            case Key.T: SelectDrawType((EditorDrawType)(((int)_editor.Brush.DrawType + 1) % 3)); break;
            case Key.Escape: _editor.ClearSelection(); break;
            case Key.Delete: _editor.DeleteSelection(); break;
            case Key.LeftArrow: _editor.MoveSelection(-1, 0); break;
            case Key.RightArrow: _editor.MoveSelection(1, 0); break;
            case Key.UpArrow: _editor.MoveSelection(0, -1); break;
            case Key.DownArrow: _editor.MoveSelection(0, 1); break;
            case Key.G when args.Shift: ToggleGrid(); break;
        }
    }

    private void SelectTool(EditorToolType tool) {
        _editor.Tool = tool;
        foreach (var button in _editorToolButtons) button.SetSelected(button.Tool == tool);
        ShowStatus($"{tool} tool selected.");
        RefreshStatus();
    }

    private void SelectDrawType(EditorDrawType drawType) {
        _editor.Brush.DrawType = drawType;
        foreach (var button in _drawTypeButtons) button.SetSelected(button.DrawType == drawType);
        _palette.SetDrawType(drawType, _editor.Brush.GetSelectedType());
        ShowStatus($"Drawing {drawType.ToString().ToLowerInvariant()}.");
        RefreshStatus();
    }

    private void SelectDefaultGround() {
        var entries = EditorCatalog.GetEntries(EditorDrawType.Ground);
        if (entries.Count == 0) return;
        _editor.Brush.GroundType = entries[0].Type;
        _palette.SetSelectedType(entries[0].Type);
    }

    private void NewMap() {
        ShowPrompt("Create map", ["Name", "Width", "Height"], ["untitled", "64", "64"], values => {
            if (!int.TryParse(values[1], out var width) || !int.TryParse(values[2], out var height)) {
                ShowStatus("Width and height must be numbers.");
                return;
            }
            var document = AddDocument(new EditorMapData(values[0], width, height));
            document.JsonPath = null;
            document.WmapPath = null;
            _panX = 0;
            _panY = 0;
            _zoom = 100f;
            SaveActiveViewState();
            ShowStatus($"Created {values[0]} ({_editor.Map.Width} x {_editor.Map.Height}).");
            RefreshAll();
        });
    }

    private void ResizeMap(int width, int height) {
        _editor.Map.ResizeMap(width, height);
        ClampOffset();
        ShowStatus($"Map resized to {width} x {height}.");
        RefreshAll();
    }

    private void ShowResizePrompt() {
        ShowPrompt("Resize map", ["Width", "Height"], [_editor.Map.Width.ToString(), _editor.Map.Height.ToString()], values => {
            if (!int.TryParse(values[0], out var width) || !int.TryParse(values[1], out var height)) {
                ShowStatus("Width and height must be numbers.");
                return;
            }
            ResizeMap(width, height);
        });
    }

    private void ShowObjectNamePrompt(int x, int y) {
        var tile = _editor.Map.GetTile(x, y);
        if (tile is null || tile.ObjectType == 0) {
            ShowStatus("There is no object on this tile to edit.");
            return;
        }
        ShowPrompt("Edit object", ["Name / config"], [tile.ObjectConfig ?? string.Empty], values => {
            _editor.SetObjectConfig(x, y, values[0]);
            ShowStatus($"Updated object at ({x}, {y}).");
        });
    }

    private void ShowPrompt(string title, string[] labels, string[] values, Action<string[]> accepted) {
        if (_prompt is not null) return;
        _prompt = new EditorPrompt(title, labels, values, accepted, ClosePrompt);
        _root.AddChild(_prompt);
    }

    private void ClosePrompt() {
        if (_prompt is null) return;
        _root.RemoveChild(_prompt);
        _prompt = null;
    }

    private void LoadMap() {
        var path = NativeFileDialog.OpenMap();
        if (string.IsNullOrWhiteSpace(path)) return;
        try {
            var map = EditorMapSerializer.Load(path);
            var document = AddDocument(map);
            if (Path.GetExtension(path).Equals(".wmap", StringComparison.OrdinalIgnoreCase)) document.WmapPath = path;
            else document.JsonPath = path;
            _panX = 0;
            _panY = 0;
            _zoom = 100f;
            SaveActiveViewState();
            ShowStatus($"Loaded {Path.GetFileName(path)}.");
            RefreshAll();
        } catch (Exception error) {
            ShowStatus($"Load failed: {error.Message}");
        }
    }

    private void SaveMap(bool wmap) {
        if (!_mapOpen) {
            ShowStatus("No map is open.");
            return;
        }
        var path = wmap ? _activeDocument.WmapPath : _activeDocument.JsonPath;
        if (string.IsNullOrWhiteSpace(path)) path = NativeFileDialog.SaveMap(_editor.Map.Name, wmap);
        if (string.IsNullOrWhiteSpace(path)) return;
        try {
            EditorMapSerializer.Save(_editor.Map, path);
            if (wmap) _activeDocument.WmapPath = path;
            else _activeDocument.JsonPath = path;
            ShowStatus($"Saved {Path.GetFileName(path)}.");
            UpdateMapSelector();
            RefreshStatus();
        } catch (Exception error) {
            ShowStatus($"Save failed: {error.Message}");
        }
    }

    private void BackToTitle() {
        if (_documents.TrueForAll(document => document.Map.SavedChanges)) {
            LeaveEditor();
            return;
        }
        DialogManager.Enqueue(new Dialog(
            "Unsaved map",
            "Leave the editor and discard unsaved changes?",
            new DialogOption("leave", LeaveEditor),
            new DialogOption("stay")
        ));
    }

    private void LeaveEditor() => ScreenManager.FadeTo(new TitleScreen());

    private void CloseCurrentMap() {
        if (!_mapOpen) return;
        RequestCloseDocument(_activeDocument);
    }

    private void RequestCloseDocument(EditorMapDocument document) {
        if (document is null) return;
        if (document.Map.SavedChanges) {
            RemoveDocument(document);
            return;
        }
        DialogManager.Enqueue(new Dialog(
            "Unsaved map",
            "Close this map and discard unsaved changes?",
            new DialogOption("close", () => RemoveDocument(document)),
            new DialogOption("stay")
        ));
    }

    private void RemoveDocument(EditorMapDocument document) {
        var index = _documents.IndexOf(document);
        if (index < 0) return;

        var wasActive = ReferenceEquals(document, _activeDocument);
        if (wasActive) SaveActiveViewState();
        _documents.RemoveAt(index);

        if (_documents.Count == 0) {
            _activeDocument = null;
            _mapOpen = false;
            _tileLayer.Visible = false;
            _mouseTileX = -1;
            _mouseTileY = -1;
            _mapInfo.SetText(string.Empty);
            _tileInfo.SetText(string.Empty);
            RebuildMapTabs();
            RefreshAll();
            return;
        }

        if (wasActive) {
            _activeDocument = null;
            ActivateDocument(_documents[Math.Min(index, _documents.Count - 1)]);
        } else {
            UpdateMapSelector();
        }
    }

    private void UpdateMapSelector() {
        RebuildMapTabs();
    }

    private void ToggleGrid() {
        _grid = !_grid;
        SaveActiveViewState();
        _gridCheckbox.SetChecked(_grid);
        ShowStatus(_grid ? "Grid enabled." : "Grid disabled.");
        RefreshCanvas();
    }

    private void ToggleAutoSave() {
        _autoSave = !_autoSave;
        _autoSaveCheckbox.SetChecked(_autoSave);
        ShowStatus(_autoSave ? "Autosave enabled." : "Autosave disabled.");
    }

    private void ToggleReplace() {
        _editor.Brush.Replace = !_editor.Brush.Replace;
        ShowStatus(_editor.Brush.Replace ? "Brush replacement enabled." : "Brush only paints empty tiles.");
    }

    private static int GetNumberKey(Key key) {
        return key switch {
            Key.D0 => 0, Key.D1 => 1, Key.D2 => 2, Key.D3 => 3, Key.D4 => 4,
            Key.D5 => 5, Key.D6 => 6, Key.D7 => 7, Key.D8 => 8, Key.D9 => 9,
            _ => -1
        };
    }

    private void BuildVisibleTiles() {
        _tileLayer.RemoveChildren();
    }

    private void RefreshAll() {
        RefreshCanvas();
        RefreshStatus();
    }

    private void OnEditorChanged() {
        _mapRenderDirty = true;
        UpdateMapSelector();
        RefreshAll();
    }

    private void RefreshCanvas() {
        if (!_mapOpen) return;
        if (_mapRenderDirty) {
            _mapRenderer.Rebuild(_editor.Map);
            _mapRenderDirty = false;
        }
        RefreshMapOverlays();
    }

    private void RefreshMapOverlays() {
        _tileLayer.RemoveChildren();
        if (!_mapOpen) return;
        var tileSize = BaseTileSize * _zoom / 100f;
        GetMapOrigin(out var preciseOriginX, out var preciseOriginY);
        var firstX = Math.Max(0, (int)Math.Floor(-preciseOriginX / tileSize));
        var firstY = Math.Max(0, (int)Math.Floor(-preciseOriginY / tileSize));
        var lastX = Math.Min(_editor.Map.Width - 1, (int)Math.Ceiling((_screenWidth - preciseOriginX) / tileSize));
        var lastY = Math.Min(_editor.Map.Height - 1, (int)Math.Ceiling((_screenHeight - preciseOriginY) / tileSize));
        for (var y = firstY; y <= lastY; y++) {
            for (var x = firstX; x <= lastX; x++) {
                var region = _editor.Map.GetTile(x, y).RegionType;
                if (region == 0) continue;
                var left = ProjectMapCoordinate(preciseOriginX, tileSize, x);
                var right = ProjectMapCoordinate(preciseOriginX, tileSize, x + 1);
                var top = ProjectMapCoordinate(preciseOriginY, tileSize, y);
                var bottom = ProjectMapCoordinate(preciseOriginY, tileSize, y + 1);
                _tileLayer.AddChild(new ColorRect(new ColorRectConfig {
                    X = left, Y = top,
                    Width = Math.Max(1, right - left), Height = Math.Max(1, bottom - top),
                    Color = RegionColor(region), Alpha = 0.32f
                }));
            }
        }
        if (_grid) AddGridOverlay(preciseOriginX, preciseOriginY, tileSize, firstX, firstY, lastX, lastY);
        AddMapBoundsOverlay(preciseOriginX, preciseOriginY, tileSize);
        AddSelectionOverlay(preciseOriginX, preciseOriginY, tileSize);
        AddHoverOverlay(preciseOriginX, preciseOriginY, tileSize);
    }

    private void AddMapBoundsOverlay(float originX, float originY, float tileSize) {
        var left = ProjectMapCoordinate(originX, tileSize, 0);
        var right = ProjectMapCoordinate(originX, tileSize, _editor.Map.Width);
        var top = ProjectMapCoordinate(originY, tileSize, 0);
        var bottom = ProjectMapCoordinate(originY, tileSize, _editor.Map.Height);
        AddOutline(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top), 0xFFFFFF, 1f);
    }

    private void AddHoverOverlay(float originX, float originY, float tileSize) {
        if (_mouseTileX < 0 || _mouseTileY < 0) return;

        var radius = _editor.Tool is EditorToolType.Pencil or EditorToolType.Eraser or EditorToolType.Shape
            ? _editor.Brush.Size
            : 0;
        if (radius > 0) {
            AddBrushHoverOutline(originX, originY, tileSize, radius);
            return;
        }

        var left = ProjectMapCoordinate(originX, tileSize, _mouseTileX);
        var right = ProjectMapCoordinate(originX, tileSize, _mouseTileX + 1);
        var top = ProjectMapCoordinate(originY, tileSize, _mouseTileY);
        var bottom = ProjectMapCoordinate(originY, tileSize, _mouseTileY + 1);
        AddOutline(
            left,
            top,
            Math.Max(1, right - left),
            Math.Max(1, bottom - top),
            0xFFFFFF,
            1f
        );
    }

    private void AddBrushHoverOutline(float originX, float originY, float tileSize, int radius) {
        for (var offsetY = -radius; offsetY <= radius; offsetY++) {
            for (var offsetX = -radius; offsetX <= radius; offsetX++) {
                if (offsetX * offsetX + offsetY * offsetY > radius * radius) continue;

                var tileX = _mouseTileX + offsetX;
                var tileY = _mouseTileY + offsetY;
                if (tileX < 0 || tileY < 0 || tileX >= _editor.Map.Width || tileY >= _editor.Map.Height) continue;

                var left = offsetX == -radius || !IsBrushTile(offsetX - 1, offsetY, radius);
                var right = offsetX == radius || !IsBrushTile(offsetX + 1, offsetY, radius);
                var top = offsetY == -radius || !IsBrushTile(offsetX, offsetY - 1, radius);
                var bottom = offsetY == radius || !IsBrushTile(offsetX, offsetY + 1, radius);
                var leftEdge = ProjectMapCoordinate(originX, tileSize, tileX);
                var rightEdge = ProjectMapCoordinate(originX, tileSize, tileX + 1);
                var topEdge = ProjectMapCoordinate(originY, tileSize, tileY);
                var bottomEdge = ProjectMapCoordinate(originY, tileSize, tileY + 1);
                var x = leftEdge;
                var y = topEdge;
                var width = Math.Max(1, rightEdge - leftEdge);
                var height = Math.Max(1, bottomEdge - topEdge);

                if (top) AddHoverEdge(x, y, width, 1);
                if (bottom) AddHoverEdge(x, y + height - 1, width, 1);
                if (left) AddHoverEdge(x, y, 1, height);
                if (right) AddHoverEdge(x + width - 1, y, 1, height);
            }
        }
    }

    private static bool IsBrushTile(int offsetX, int offsetY, int radius) {
        return offsetX * offsetX + offsetY * offsetY <= radius * radius;
    }

    private void AddHoverEdge(int x, int y, int width, int height) {
        _tileLayer.AddChild(new ColorRect(new ColorRectConfig {
            X = x, Y = y, Width = width, Height = height, Color = 0xFFFFFF, Alpha = 1f
        }));
    }

    private void AddGridOverlay(float originX, float originY, float tileSize, int firstX, int firstY, int lastX, int lastY) {
        if (lastX < firstX || lastY < firstY) return;
        var top = ProjectMapCoordinate(originY, tileSize, firstY);
        var bottom = ProjectMapCoordinate(originY, tileSize, lastY + 1);
        var left = ProjectMapCoordinate(originX, tileSize, firstX);
        var right = ProjectMapCoordinate(originX, tileSize, lastX + 1);
        for (var x = firstX; x <= lastX + 1; x++)
            _tileLayer.AddChild(new ColorRect(new ColorRectConfig {
                X = ProjectMapCoordinate(originX, tileSize, x), Y = top,
                Width = 1, Height = bottom - top, Color = 0, Alpha = 0.42f
            }));
        for (var y = firstY; y <= lastY + 1; y++)
            _tileLayer.AddChild(new ColorRect(new ColorRectConfig {
                X = left, Y = ProjectMapCoordinate(originY, tileSize, y),
                Width = right - left, Height = 1, Color = 0, Alpha = 0.42f
            }));
    }

    private void AddSelectionOverlay(float originX, float originY, float tileSize) {
        if (!_editor.Selection.IsActive()) return;
        var x = ProjectMapCoordinate(originX, tileSize, _editor.Selection.StartX);
        var y = ProjectMapCoordinate(originY, tileSize, _editor.Selection.StartY);
        var right = ProjectMapCoordinate(originX, tileSize, _editor.Selection.EndX + 1);
        var bottom = ProjectMapCoordinate(originY, tileSize, _editor.Selection.EndY + 1);
        AddOutline(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y), 0xFFFFFF, 1f);
    }

    private static int ProjectMapCoordinate(float origin, float tileSize, int coordinate) {
        return (int)MathF.Round(origin + coordinate * tileSize);
    }

    private void AddOutline(int x, int y, int width, int height, uint color, float alpha) {
        _tileLayer.AddChild(new ColorRect(new ColorRectConfig { X = x, Y = y, Width = width, Height = 1, Color = color, Alpha = alpha }));
        _tileLayer.AddChild(new ColorRect(new ColorRectConfig { X = x, Y = y + height - 1, Width = width, Height = 1, Color = color, Alpha = alpha }));
        _tileLayer.AddChild(new ColorRect(new ColorRectConfig { X = x, Y = y, Width = 1, Height = height, Color = color, Alpha = alpha }));
        _tileLayer.AddChild(new ColorRect(new ColorRectConfig { X = x + width - 1, Y = y, Width = 1, Height = height, Color = color, Alpha = alpha }));
    }

    private static uint RegionColor(int type) {
        unchecked {
            var hash = (uint)(type * 2654435761);
            return (80u + (hash & 0x7Fu)) << 16
                | (80u + ((hash >> 8) & 0x7Fu)) << 8
                | 80u + ((hash >> 16) & 0x7Fu);
        }
    }

    private void RefreshStatus() {
        if (_mapInfo is null) return;
        if (!_mapOpen) {
            _mapInfo.SetText(string.Empty);
            _tileInfo.SetText(string.Empty);
            return;
        }
        _mapInfo.SetText($"{_editor.Map.Width} x {_editor.Map.Height}   zoom {MathF.Round(_zoom)}%");
        if (_mouseTileX < 0) {
            _tileInfo.SetText(string.Empty);
        } else {
            var tile = _editor.Map.GetTile(_mouseTileX, _mouseTileY);
            _tileInfo.SetText($"({_mouseTileX}, {_mouseTileY})  G:0x{(ushort)tile.GroundType:X} O:0x{(ushort)tile.ObjectType:X} R:0x{(ushort)tile.RegionType:X}");
        }
        var selectedId = EditorCatalog.GetId(_editor.Brush.DrawType, _editor.Brush.GetSelectedType());
        _status.SetText($"{_editor.Tool} | {_editor.Brush.DrawType} | {selectedId} | size {_editor.Brush.Size}");
    }

    private void ShowStatus(string text) {
        if (_status is not null) _status.SetText(text);
    }

    private bool TryGetMapTile(Vector2i physical, out int mapX, out int mapY) {
        if (!_mapOpen) {
            mapX = -1;
            mapY = -1;
            return false;
        }
        var logical = ToLogical(physical);
        var tileSize = BaseTileSize * _zoom / 100f;
        GetMapOrigin(out var originX, out var originY);
        mapX = (int)Math.Floor((logical.X - originX) / tileSize);
        mapY = (int)Math.Floor((logical.Y - originY) / tileSize);
        return _editor.Map.InBounds(mapX, mapY);
    }

    private Vector2i ToLogical(Vector2i physical) {
        return new Vector2i(physical.X - _root.X, physical.Y - _root.Y);
    }

    private void ClampOffset() {
        var scale = _zoom / 100f;
        var mapWidth = _editor.Map.Width * BaseTileSize * scale;
        var mapHeight = _editor.Map.Height * BaseTileSize * scale;
        var minX = -mapWidth / 2f - _screenWidth / 2f + 64f;
        var maxX = mapWidth / 2f + _screenWidth / 2f - 64f;
        var minY = -mapHeight / 2f - _screenHeight / 2f + 64f;
        var maxY = mapHeight / 2f + _screenHeight / 2f - 64f;
        _panX = Math.Clamp(_panX, minX, maxX);
        _panY = Math.Clamp(_panY, minY, maxY);
    }

    private void GetMapOrigin(out float x, out float y) {
        var scale = _zoom / 100f;
        x = (_screenWidth - _editor.Map.Width * BaseTileSize * scale) / 2f + _panX;
        y = (_screenHeight - _editor.Map.Height * BaseTileSize * scale) / 2f + _panY;
    }

    private int AddTopButton(string text, int x, Action callback) {
        var button = new EditorSmallButton(text, callback) { X = x, Y = 15 };
        _root.AddChild(button);
        _topButtons.Add(button);
        return x + button.Width + 10;
    }

    private SimpleText AddLabel(int x, int y, UiAnchor anchor) {
        var label = new SimpleText(new TextConfig {
            Text = string.Empty, FontSize = 18, FontType = FontType.Bold,
            X = x, Y = y, Anchor = anchor, Color = 0xFFFFFF
        });
        _root.AddChild(label);
        return label;
    }

    public override void Update(GameTime gameTime) {
        if (!_mapOpen || !_autoSave || _editor.Map.SavedChanges || gameTime.TotalMs - _lastAutoSave < 30000) return;
        _lastAutoSave = gameTime.TotalMs;
        try {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave");
            Directory.CreateDirectory(directory);
            EditorMapSerializer.SaveWmap(_editor.Map, Path.Combine(directory, _editor.Map.Name + ".wmap"));
            ShowStatus("Autosaved map.");
        } catch (Exception error) {
            ShowStatus($"Autosave failed: {error.Message}");
        }
    }

    public override void Draw(GameTime gameTime) {
        _backdropRenderer.Draw();
        if (!_mapOpen) return;
        var tileSize = BaseTileSize * _zoom / 100f;
        var center = new Vector2(
            _editor.Map.Width * 0.5f - _panX / (float)tileSize,
            _editor.Map.Height * 0.5f - _panY / (float)tileSize
        );
        var camera = Camera.Update(center, new Vector3i(_screenWidth, _screenHeight, 0), 0f, tileSize / 50f);
        _mapRenderer.Draw(gameTime, camera);
    }
}
