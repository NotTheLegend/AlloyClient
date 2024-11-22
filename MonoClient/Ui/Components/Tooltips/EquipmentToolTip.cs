using MonoClient.Objects.Util.ItemDatas;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;
using System;
using System.Reflection.Metadata.Ecma335;

namespace MonoClient.Ui.Components.Tooltips;

public class EquipmentToolTip : Tooltip 
{
    private ItemDesc Item;

    private ObjectRect Icon;
    private TierText TierTag;
    private SimpleText TitleText;
    private SimpleText DescText;

    public EquipmentToolTip(ItemDesc item) : base(220, 100)
    {
        Item = item;
        AddIcon();
        AddTitle();
        AddTierTag();
        AddDescription();
        Position();
        DrawSprite();
    }

    private void AddIcon()
    {
        ushort obj = Item.ObjectType;
        Icon = new ObjectRect(new ObjectRectConfig
        {
            Texture = AssetUtils.GetTextureInfo(obj <= 0 ? (ushort)0x0096 : obj),
            Width = 40,
            Height = 40
        });
        AddChild(Icon);
    }

    private void AddTitle()
    {
        TitleText = new SimpleText(SimpleConfig(Item.ObjectId, 16, true));
        TitleText.SetAnchor(UiAnchor.MiddleLeft);
        AddChild(TitleText);
    }

    private void AddTierTag()
    {
        TierTag = new TierText(Item);
        TierTag.SetAnchor(UiAnchor.MiddleRight);
        AddChild(TierTag);
    }
    
    private void AddDescription()
    {
        DescText = new SimpleText(SimpleConfig(Item.Description, 14, false, 0xaaaaaa, 0xaaaaaa, 1, 204));
        AddChild(DescText);
    }

    private void Position()
    {
        Icon.X = Icon.Y = 5;
        TitleText.X = Icon.X + Icon.Width + 3;
        TitleText.Y = Icon.Width / 2;
        TierTag.X = ToolWidth - 15;
        TierTag.Y = TitleText.Y;
        DescText.X = 8;
        DescText.Y = Icon.Y + Icon.Width + 3;
    }

    public override void DrawSprite()
    {
        ToolHeight = Height + 10;
        base.DrawSprite();
    }

    public static float Round(float number, int decimalPlaces = 1)
    {
        float exp = MathF.Pow(10, decimalPlaces);
        if (decimalPlaces > 0) {
            number = (int)(number * exp) / exp;
        }
        else if (decimalPlaces == 0) {
            number = (int)number;
        }

        return number;
    }

    public static TextConfig SimpleConfig(string text = "", int size = 12, bool bold = false, uint color = 0xffffff, uint outline = 0, int thickness = 1, int maxWidth = 200)
    {
        return new TextConfig()
        {
            FontSize = size,
            Bold = bold,
            Text = text,
            Color = color, 
            OutlineColor = outline,
            OutlineThickness = thickness,
            MaxWidth = maxWidth,
        };
    }
}