using System;
using System.Collections.Generic;
using MonoClient.Assets.Libraries;
using MonoClient.Screens.MapEditor.Components.Types;
using MonoClient.State;
using MonoClient.Ui.Components.Scrollbars;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.MapEditor.Components;

public class EditorObjectsPanel : Sprite {
    private readonly MapEditorScreen _screen;
    
    private readonly ColorRect _background;
    private readonly TextInput _searchInput;
    
    private readonly Container _groundContainer;
    private readonly List<MapEditorObjectRect> _groundRects = [];
    
    private readonly Container _objectContainer;
    private readonly List<MapEditorObjectRect> _objectRects = [];
    
    private readonly Container _regionContainer;
    private readonly List<MapEditorObjectRect> _regionRects = [];
    
    private readonly VerticalScrollBar _scrollBar;

    public EditorObjectsPanel(MapEditorScreen screen) {
        _background = new ColorRect(new ColorRectConfig {
            Alpha = 0.8f,
            Color = 0x2B2B2B,
            Width = 140,
            Height = 650,
            Anchor = UiAnchor.RightBottom,
            X = Settings.DefaultScreenWidth - 10,
            Y = Settings.DefaultScreenHeight - 10,
        });
        _background.EnableClipRect = true;
        AddChild(_background);

        _searchInput = new TextInput(new InputConfig {
            X = _background.Width / 2,
            Y = 5,
            Width = _background.Width - 10,
            FontSize = 18,
            Anchor = UiAnchor.MiddleTop,
        });
        
        _background.AddChild(_searchInput);
        
        var scrollContainer = new Container(new ContainerConfig {
            Width = _background.Width,
            Height = _background.Height - _searchInput.Height - 10,
            Y = _searchInput.Y + _searchInput.Height + 10,
            EnableClip = true
        });
        _background.AddChild(scrollContainer);

        _groundContainer = new Container();
        scrollContainer.AddChild(_groundContainer);

        _objectContainer = new Container {
            Visible = false
        };
        scrollContainer.AddChild(_objectContainer);

        _regionContainer = new Container {
            Visible = false
        };
        scrollContainer.AddChild(_regionContainer);

        var groundPropLib = GroundLibrary.TypeToGroundProps;
        const int padding = 7;
        const int columns = 3;
        foreach (var groundProp in groundPropLib.Values) {
            var textureData = GroundLibrary.TypeToTextureData[groundProp.ObjectType];
            var atlasData = textureData.EditorTexture ?? textureData.GetTexture();
            var textureInfo = new TextureInfo(atlasData, TextureType.GameAtlas);
            var objectId = groundProp.ObjectId;
            var objectType = groundProp.ObjectType;
            var x = _groundRects.Count % columns;
            var y = _groundRects.Count / columns;
            var objectRect = new MapEditorObjectRect(objectId, objectType, textureInfo, x, y, padding, _searchInput.Height, () => {
                Console.WriteLine($"ObjectID: {objectId}, ObjectType: 0x{objectType:X}");
            });
            
            _groundRects.Add(objectRect);
            _groundContainer.AddChild(objectRect);
        }
        
        var objectPropLib = ObjectLibrary.TypeToObjectProps;
        foreach (var objectProp in objectPropLib.Values) {
            var textureData = ObjectLibrary.TypeToTextureData[objectProp.ObjectType];
            var atlasData = textureData.EditorTexture ?? textureData.GetTexture();
            var textureInfo = new TextureInfo(atlasData, TextureType.GameAtlas);
            var objectId = objectProp.ObjectId;
            var objectType = objectProp.ObjectType;
            var x = _objectRects.Count % columns;
            var y = _objectRects.Count / columns;
            var objectRect = new MapEditorObjectRect(objectId, objectType, textureInfo, x, y, padding, _searchInput.Height, () => {
                Console.WriteLine($"ObjectID: {objectId}, ObjectType: 0x{objectType:X}");
            });

            _objectRects.Add(objectRect);
            _objectContainer.AddChild(objectRect);
        }

        // TODO: Regions

        _scrollBar = new VerticalScrollBar(scrollContainer, new VerticalScrollBarConfig {
            X = _background.Width + 5,
            Y = scrollContainer.Y,
            Width = 10,
            Height = scrollContainer.Height - 10,
            TotalContentHeight = _groundRects.Count / columns * (32 + padding),
            VisibleContentHeight = scrollContainer.Height,
            OnValueChanged = value => {
                _groundContainer.Y = -value;
            },
            ScrollStep = 32 + padding
        });
        _scrollBar.SetAnchor(UiAnchor.RightTop);
        _background.AddChild(_scrollBar);
    }
}