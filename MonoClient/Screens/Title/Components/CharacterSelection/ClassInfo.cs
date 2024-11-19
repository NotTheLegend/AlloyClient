using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoClient.Assets.Libraries;
using MonoClient.Screens.Game.Components.Hud.Inventory;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens.Title.Components.CharacterList;

public class ClassInfo : Container {

    // things to refresh
    private ObjectRect _characterRect;
    private SimpleText _className;
    private SimpleText _classDescription;
    
    private ClassRect _selectedClass;
    private ColorRect _background;

    private List<InventoryTile> _classEquipment = [];
    
    public ClassInfo() {
        _background = new ColorRect(new ColorRectConfig {
            Width = 960,
            Height = 380,
            Color = 0x171717,
            Alpha = 1f
        });
        _background.X = Settings.DefaultScreenWidth / 2 - _background.Width / 2;
        _background.Y = 50;
        AddChild(_background);
        
        var scrollContainer = new Container(new ContainerConfig {
            Width = 200,
            Height = _background.Height,
            X = 600,
            EnableClip = true
        });
        _background.AddChild(scrollContainer);
    }
    
    public void Update(GameTime gameTime, ClassRect classRect) {
        if (_selectedClass == null || _selectedClass.Type != classRect.Type) {
            _selectedClass = classRect;
            UpdateInfo(classRect);
        }
        
        var frames = UiRender.GameAtlas.GetAnimationAtlasData("players", classRect.CharacterIndex);
        var duration = 250;
        
        var totalMs = (int)gameTime.TotalGameTime.TotalMilliseconds;
        var frameIndex = 1 + totalMs / duration % 2;

        _characterRect?.ChangeTexture(new TextureInfo(frames.FaceDown[frameIndex], TextureType.GameAtlas));
    }

    private void UpdateInfo(ClassRect classRect) {
        ClearChildren();
        
        var frames = UiRender.GameAtlas.GetAnimationAtlasData("players", classRect.CharacterIndex);
        _characterRect = new ObjectRect(new ObjectRectConfig {
            Texture = new TextureInfo(frames.FaceDown[0], TextureType.GameAtlas),
            Width = 120,
            Height = 120,
            X = _background.Width / 2 - 60,
            Y = 60
        });

        var props = ObjectLibrary.TypeToObjectProps[_selectedClass.Type];
        _className = new SimpleText(new TextConfig {
            Text = props.ObjectId,
            FontSize = 30,
            Bold = true
        });
        _className.X = _characterRect.X + _characterRect.Width / 2 - _className.Width / 2;;
        _className.Y = _characterRect.Y - 40;
        
        _background.AddChild(_characterRect);
        _background.AddChild(_className);
        
        _classDescription = new SimpleText(new TextConfig {
            Text = props.Description,
            FontSize = 20,
            Bold = true,
            MaxWidth = 200
        });
        _classDescription.X = 100;
        _classDescription.Y = 60;
        _background.AddChild(_classDescription);
        
        _background.AddChild(_characterRect);
        _background.AddChild(_className);
        
        for (var i = 0; i < 4; i++) {
            var tile = new InventoryTile(ObjectLibrary.CreateItem(props.Equipment[i] ?? 0), false) {
                X = _characterRect.X - 52 + i * 52,
                Y = 200,
                Slot = (byte)i,
                Owner = null
            };
            
            _classEquipment.Add(tile);
            _background.AddChild(tile);
        }
        
        foreach (var item in ObjectLibrary.TypeToSkins.Where(item => item.Item1 == _selectedClass.Type)) {
            Logger.Info("skin added to list: " + item.Item2);
        }
    }

    private void ClearChildren() {
        _classEquipment.ForEach(x => _background.RemoveChild(x));
        
        _background.RemoveChild(_characterRect);
        _background.RemoveChild(_className);
        _background.RemoveChild(_classDescription);
    }
}