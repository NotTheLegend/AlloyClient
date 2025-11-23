using System.Linq;
using System.Xml.Linq;
using Common;

namespace RealmClient.Data;

public sealed class CharacterListData(XElement xml) : IGlobalData {

    public readonly int NextCharId = xml.GetAttribute("nextCharId", 0);

    public readonly int MaxNumChars = xml.GetAttribute("maxNumChars", 0);

    public readonly int CharSlotCost = xml.GetAttribute("charSlotCost", 0);

    public readonly int[] OwnedSkins = xml.GetValue("OwnedSkins", "").FromCommaSepString();

    public readonly Character[] Characters = xml.Elements("Char").Select(c => new Character(c)).ToArray();
}

public sealed class Character(XElement xml) {
    
    //TODO: rest of character data parsing
    
    public readonly int Id = xml.GetAttribute("id", 0);
    
    public readonly ushort ObjectType = xml.GetValue<ushort>("ObjectType");
    
    //
    
    //
    
    //
    
    //
    
    //
    
    //

    public readonly int CurrentFame = xml.GetValue("Fame", 0);
    
    //
    
    //
    
    public readonly int MaxHitPoints = xml.GetValue("MaxHitPoints", 0);

    public readonly int HitPoints = xml.GetValue("HitPoints", 0);
    
    public readonly int MaxMagicPoints = xml.GetValue("MaxMagicPoints", 0);

    public readonly int MagicPoints = xml.GetValue("MagicPoints", 0);
    
    //
    
    //

    public readonly int Attack = xml.GetValue("Attack", 0);
    
    public readonly int Defense = xml.GetValue("Defense", 0);
    
    public readonly int Dexterity = xml.GetValue("Dexterity", 0);

    public readonly int Wisdom = xml.GetValue("Wisdom", 0);
        
    public readonly int Speed = xml.GetValue("Wisdom", 0);
    
    public readonly int Vitality = xml.GetValue("LifeRegeneration", 0);
}