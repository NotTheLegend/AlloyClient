using System.Text;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.MapEditor.Components;

public class EditorObjectDetailsPanel : Sprite {
    // create a 250x150 rect that contains data on the tile+object that is hovered over
    // this will be used to display the tile+object name, and the tile+object id
    // What to show:
    // Ground: Name, ObjectType
    // Object: Name, ObjectType
    
    // Position: 0, 0
    
    // Size: 250, 150
    
    // slightly transparent background
    // border
    // gray

    private readonly ColorRect _background;
    private readonly SimpleText _groundText;
    private readonly SimpleText _objectText;
    private readonly SimpleText _regionText;
    private readonly SimpleText _positionText;
    
    public EditorObjectDetailsPanel() {
        _background = new ColorRect(new ColorRectConfig {
            Alpha = 0.8f,
            Color = 0x2B2B2B,
            Width = 275,
            Height = 100,
            Anchor = UiAnchor.LeftBottom,
            X = 5,
            Y = Settings.DefaultScreenHeight - 5
        });
        AddChild(_background);

        var textConfig = new TextConfig {
            FontSize = 16,
            Bold = true,
            X = 5,
            Y = 5,
            Text = "Empty",
        };
        _groundText = new SimpleText(textConfig);
        _background.AddChild(_groundText);
        
        textConfig.Y += _groundText.Height + 5;
        _objectText = new SimpleText(textConfig);
        _background.AddChild(_objectText);
        
        textConfig.Y += _objectText.Height + 5;
        _regionText = new SimpleText(textConfig);
        _background.AddChild(_regionText);
        
        textConfig.Y += _regionText.Height * 2 + 5;
        _positionText = new SimpleText(textConfig);
        _background.AddChild(_positionText);
    }

    public void UpdateDetails(MapTile tile) {
        if (tile.Type == MapEditorScreen.MapEditorTileType) {
            _groundText.SetText("Tile: Empty");
        }
        else {
            var groundProperties = tile.GroundProperties;
            var id = groundProperties.ObjectId;
            var type = groundProperties.ObjectType;
            _groundText.SetText($"Tile: {id} (0x{type:X})");
        }

        if (tile.OccupiedObject == null) {
            _objectText.SetText("Object: Empty");
        }
        else {
            var objectProperties = tile.OccupiedObject.Properties;
            var id = objectProperties.ObjectId;
            var type = objectProperties.ObjectType;
            _objectText.SetText($"Object: {id} (0x{type:X})");
        }

        // TODO: Region
        _regionText.SetText("Region: Empty");
        
        _positionText.SetText($"Position: {tile.X}, {tile.Y}");
    }
}