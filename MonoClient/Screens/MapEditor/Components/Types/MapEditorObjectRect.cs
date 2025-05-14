using System;
using MonoClient.UiLib.Assets;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.MapEditor.Components.Types;

public class MapEditorObjectRect : Sprite {
    private static ObjectRectConfig _config;
    
    public readonly string ObjectId;
    public readonly ushort ObjectType;
    
    private readonly ObjectRect _objectRect;
    
    static MapEditorObjectRect() {
        _config = new ObjectRectConfig {
            Width = 32,
            Height = 32,
            Anchor = UiAnchor.LeftTop,
            MouseEnabled = true
        };
    }
    
    public MapEditorObjectRect(string objectId, ushort objectType, TextureInfo textureInfo, int x, int y, int padding, int searchInputHeight, Action onClick) {
        ObjectId = objectId;
        ObjectType = objectType;
        
        var atlasData = textureInfo.AtlasData;
        atlasData.RemovePadding();
        _config.Texture = new TextureInfo(atlasData, textureInfo.TextureType);

        _config.X = x * (_config.Width + padding) + 10;
        _config.Y = searchInputHeight + (y - 1) * (_config.Height + padding) + 10;
        
        _objectRect = new ObjectRect(_config);
        AddChild(_objectRect);

        _objectRect.AddEventListener(MouseEvent.LeftClick, onClick);
    }
}