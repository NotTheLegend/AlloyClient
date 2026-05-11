using System.Xml.Linq;
using Alloy.Common;

namespace AlloyClient.Assets.XmlStructs;

public class PlayerProperties {
    public readonly int Hp;
    public readonly int MaxHp;
    
    public readonly int Mp;
    public readonly int MaxMp;
    
    public readonly int Attack;
    public readonly int MaxAttack;
    
    public readonly int Defense;
    public readonly int MaxDefense;
    
    public readonly int Speed;
    public readonly int MaxSpeed;
    
    public readonly int Dexterity;
    public readonly int MaxDexterity;
    
    public readonly int Vitality;
    public readonly int MaxVitality;
    
    public readonly int Wisdom;
    public readonly int MaxWisdom;

    public PlayerProperties(XElement e) {
        Hp = e.GetValue<int>("MaxHitPoints");
        var maxHitPointsElement = e.Element("MaxHitPoints");
        MaxHp = maxHitPointsElement.GetAttribute<int>("max");
        
        Mp = e.GetValue<int>("MaxMagicPoints");
        var maxMagicPointsElement = e.Element("MaxMagicPoints");
        MaxMp = maxMagicPointsElement.GetAttribute<int>("max");
        
        Attack = e.GetValue<int>("Attack");
        var attackElement = e.Element("Attack");
        MaxAttack = attackElement.GetAttribute<int>("max");
        
        Defense = e.GetValue<int>("Defense");
        var defenseElement = e.Element("Defense");
        MaxDefense = defenseElement.GetAttribute<int>("max");
        
        Speed = e.GetValue<int>("Speed");
        var speedElement = e.Element("Speed");
        MaxSpeed = speedElement.GetAttribute<int>("max");
        
        Dexterity = e.GetValue<int>("Dexterity");
        var dexterityElement = e.Element("Dexterity");
        MaxDexterity = dexterityElement.GetAttribute<int>("max");
        
        Vitality = e.GetValue<int>("HpRegen");
        var hpRegenElement = e.Element("HpRegen");
        MaxVitality = hpRegenElement.GetAttribute<int>("max");
        
        Wisdom = e.GetValue<int>("MpRegen");
        var mpRegenElement = e.Element("MpRegen");
        MaxWisdom = mpRegenElement.GetAttribute<int>("max");
        
    }
    
}