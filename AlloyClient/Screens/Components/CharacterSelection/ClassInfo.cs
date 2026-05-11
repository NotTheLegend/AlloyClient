using System.Linq;
using AlloyClient.Assets.Libraries;
using AlloyClient.Game.Components.Hud.Inventory;
using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Enums;
using AlloyClient.Utils;
using Alloy.Common;

namespace AlloyClient.Screens.Components.CharacterSelection;

public class ClassInfo : Container {

    // things to refresh
    private ObjectRect _characterRect;
    private SimpleText _className;
    private SimpleText _classDescription;
    
    private ClassRect _selectedClass;
    private ColorRect _background;

    private EquippedGrid _classEquipment;
    
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
        
        var frames = Main.Atlas.GetAnimationAtlasData("players", classRect.CharacterIndex);
        var duration = 250;
        
        var totalMs = (int)gameTime.TotalMs;
        var frameIndex = 1 + totalMs / duration % 2;

        _characterRect?.ChangeTexture(TextureHelper.Create(frames.FaceDown[frameIndex], TextureType.GameAtlas));
    }

    private void UpdateInfo(ClassRect classRect) {
        ClearChildren();
        
        var frames = Main.Atlas.GetAnimationAtlasData("players", classRect.CharacterIndex);
        _characterRect = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.Create(frames.FaceDown[0], TextureType.GameAtlas),
            Width = 120,
            Height = 120,
            X = _background.Width / 2 - 60,
            Y = 60
        });

        var props = ObjectLibrary.TypeToObjectProps[_selectedClass.Type];
        _className = new SimpleText(new TextConfig {
            Text = props.ObjectId,
            FontSize = 30,
            FontType = FontType.Bold
        });
        _className.X = _characterRect.X + _characterRect.Width / 2 - _className.Width / 2;;
        _className.Y = _characterRect.Y - 40;
        
        _background.AddChild(_characterRect);
        _background.AddChild(_className);
        
        _classDescription = new SimpleText(new TextConfig {
            Text = props.Description,
            FontSize = 20,
            FontType = FontType.Bold,
            MaxWidth = 200
        });
        _classDescription.X = 100;
        _classDescription.Y = 60;
        _background.AddChild(_classDescription);
        
        _background.AddChild(_characterRect);
        _background.AddChild(_className);

        var startingItems = new [] { ObjectLibrary.GetItem(props.Equipment[0] ?? 0),ObjectLibrary.GetItem(props.Equipment[1] ?? 0),ObjectLibrary.GetItem(props.Equipment[2] ?? 0),ObjectLibrary.GetItem(props.Equipment[3] ?? 0),};
        var equipped = new EquippedGrid(startingItems, props.SlotTypes);
        equipped.X = _characterRect.X - 52;
        equipped.Y = 200;

        _classEquipment = equipped;
        _background.AddChild(_classEquipment);
        
        foreach (var item in ObjectLibrary.TypeToSkins.Where(item => item.Item1 == _selectedClass.Type)) {
            Logger.Info("skin added to list: " + item.Item2);
        }
    }

    private void ClearChildren() {
       _background.RemoveChild(_classEquipment);
        
        _background.RemoveChild(_characterRect);
        _background.RemoveChild(_className);
        _background.RemoveChild(_classDescription);
    }
}