using System;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Ui.Components.Elements;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Enums;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Components.Tooltips;

public sealed class EquipmentToolTip : Tooltip 
{
    private ItemDesc _itemDesc;

    private ObjectRect Icon;
    private TierText TierTag;
    private SimpleText TitleText;
    private SimpleText DescText;

    private SimpleText DamageText;
    private SimpleText StatsText;
    private string statsText;
    public EquipmentToolTip(ItemDesc itemDesc) : base(220, 100)
    {
        _itemDesc = itemDesc;
        AddIcon();
        AddTitle();
        AddTierTag();
        AddDescription();
        AddStatBonus();
        Position();
        DrawSprite();
    }

    private void AddIcon()
    {
        ushort obj = _itemDesc.ObjectType;
        Icon = new ObjectRect(new ObjectRectConfig
        {
            Texture = TextureHelper.FromGameAtlas(obj <= 0 ? (ushort)0x0096 : obj),
            Width = 40,
            Height = 40
        });
        AddChild(Icon);
    }

    private void AddTitle()
    {
        TitleText = new SimpleText(SimpleConfig(_itemDesc.ObjectId, 16, FontType.Bold, maxWidth: 204));
        TitleText.SetAnchor(UiAnchor.MiddleLeft);
        AddChild(TitleText);
    }

    private void AddTierTag()
    {
        if(_itemDesc.Consumable || _itemDesc.SlotType == 10)
        {
            return; 
        }

        TierTag = new TierText(_itemDesc);
        TierTag.SetAnchor(UiAnchor.MiddleRight);
        AddChild(TierTag);
    }
    
    private void AddDescription()
    {
        DescText = new SimpleText(SimpleConfig(_itemDesc.Description, 14, FontType.Normal, 0xaaaaaa, 0x0, 0.5f, 204));
        AddChild(DescText);
    }

    private void AddStatBonus() //Yes this is kinda awful, im going to do make tooltips soon
    {
        statsText = "";
        if (_itemDesc.StatBoosts != null)
        {
            foreach (var stat in _itemDesc.StatBoosts)
            {
                /*Logger.Debug(stat.ToString());
                Logger.Debug(stat.Amount.ToString());
                Logger.Debug(stat.Stat.ToString());*/
                statsText += $"\n{StatsUtil.FromId(stat.Stat)}: {stat.Amount}";
            }
        }
        /*if (Item.Activate != null)
        {
            foreach (var Activate in Item.Activate)
            {
                //Logger.Debug(stat.ToString());
                //Logger.Debug(stat.Amount.ToString());
                //Logger.Debug(stat.Stat.ToString());
                statsText += $"\n{Activate.EffectName}: {Activate.DurationMS}";
            }
        }*/
        if (_itemDesc.FameBonus != 0)
        {
            statsText += $"\nFame: {_itemDesc.FameBonus}%";
        }
        if (statsText != "")
        {
            StatsText = new SimpleText(SimpleConfig(statsText, 14, FontType.Normal, 0xaaaaaa, 0x0, 0.5f, 204));
            AddChild(StatsText);
        }
    }

    private void Position()
    {
        Icon.X = Icon.Y = 5;
        TitleText.X = Icon.X + Icon.Width + 3;
        TitleText.Y = Icon.Height / 2 + TitleText.Height / 2;
        if(TierTag != null)
        {
            TierTag.X = ToolWidth - 15;
            TierTag.Y = Icon.Height / 2 + TierTag.Height / 2;
        }
        DescText.X = 8;
        DescText.Y = Icon.Y + Icon.Width + 3;
        if (StatsText != null)
        {
            StatsText.X = 8;
            StatsText.Y = DescText.Y + DescText.Height + 3;
        }
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
    public static TextConfig SimpleConfig(string text = "", int size = 12, FontType type = FontType.Normal, uint color = 0xffffff, uint outline = 0x0, float thickness = 1f, int maxWidth = 220)
    {
        return new TextConfig()
        {
            FontSize = size,
            FontType = type,
            Text = text,
            Color = color, 
            OutlineColor = outline,
            OutlineThickness = thickness,
            MaxWidth = maxWidth,
        };
    }
}