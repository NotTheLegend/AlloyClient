using MonoClient.Objects.Util.ItemDatas;
using MonoClient.UiLib.Core;
using Microsoft.Xna.Framework;
namespace MonoClient.UiLib.BuiltIn;

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
            Bold = 1,
            Text = text,
            OutlineColor = 0,
            OutlineThickness = 2
        });
        Tag.Color = c;
        AddChild(Tag);
    }
}