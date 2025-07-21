using Common;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Ui.Components.Elements;

public class TierText : Sprite
{
    private SimpleText Tag;
    public TierText(ItemDesc desc)
    {
        Color c = Color.White;

        string text;
        if (desc.Tier == -1)
        {
            c = Color.Purple;
            text = "UT";
        }
        else if (desc.Legendary)
        {
            c = Color.Gold;
            text = "LG";
        }
        else if (desc.Demonic)
        {
            c = Color.Crimson;
            text = "DC";
        }
        else
            text = "T" + desc.Tier;
        
        Tag = new SimpleText(new TextConfig()
        {
            FontSize = 16,
            FontType = FontType.Bold,
            Text = text,
            OutlineColor = 0,
            OutlineThickness = 2
        });
        Tag.Color = c;
        AddChild(Tag);
    }
}