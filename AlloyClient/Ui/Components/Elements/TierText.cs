using Common;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.Assets.XmlStructs;

namespace RealmClient.Ui.Components.Elements;

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
        } else text = "T" + desc.Tier;
        
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