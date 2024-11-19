using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Common;
using MonoClient.Utils;

namespace MonoClient.Assets.XmlStructs;


public class ObjectProperties {
    public readonly ushort ObjectType;
    public readonly string ObjectId;
    
    public readonly string DisplayId;
    public readonly string DisplayName;
    
    public readonly List<ProjectileProperties> Projectiles;
    public readonly Dictionary<int, ProjectileProperties> ProjectilesDict;
    
    public readonly PlayerProperties PlayerProperties;

    public readonly string Class;
    public readonly string Model;
    public readonly float Rotation;
    public readonly float AngleCorrection;

    public readonly bool IsPlayer;
    public readonly bool IsEnemy;
    public readonly bool IsAlly;

    public readonly bool EnemyOccupySquare;
    public readonly bool OccupySquare;
    public readonly bool FullOccupy;

    public readonly bool Static;
    public readonly bool DrawOnGround;

    public readonly int RealSize;
    public readonly int MinSize;
    public readonly int MaxSize;
    public readonly int SizeStep = 5;
    
    public readonly string Description;
    
    public readonly ushort PlayerClassType;
    public readonly bool Skin;
    
    public readonly List<int> SlotTypes;
    public readonly List<ushort?> Equipment;

    public ObjectProperties(XElement e) {
        ObjectType = e.GetAttribute<ushort>("type");
        ObjectId = e.GetAttribute<string>("id");
        
        DisplayId = e.GetValue<string>("DisplayId");
        DisplayName = string.IsNullOrWhiteSpace(DisplayId) ? ObjectId : DisplayId;
        
        Projectiles = [];
        ProjectilesDict = [];
        foreach (var proj in e.Elements("Projectile")) {
            var props = new ProjectileProperties(proj);
            Projectiles.Add(props);
            ProjectilesDict.TryAdd(props.BulletType, props);
        }

        Class = e.GetValue<string>("Class");
        Model = e.GetValue<string>("Model");
        Rotation = e.GetValue<float>("Rotation");
        AngleCorrection = e.GetValue<float>("AngleCorrection");

        IsPlayer = e.GetValue<bool>("Player");
        IsEnemy = e.GetValue<bool>("Enemy");
        IsAlly = e.GetValue<bool>("Ally");
        
        if (IsPlayer) {
            PlayerProperties = new PlayerProperties(e);
        }
        
        EnemyOccupySquare = e.GetValue<bool>("EnemyOccupySquare");
        OccupySquare = e.GetValue<bool>("OccupySquare");
        FullOccupy = e.GetValue<bool>("FullOccupy");

        Static = e.GetValue<bool>("Static");
        DrawOnGround = e.GetValue<bool>("DrawOnGround");

        RealSize = e.GetValue<int>("RealSize", -1);
        MinSize = e.GetValue<int>("MinSize");
        MaxSize = e.GetValue<int>("MaxSize");
        SizeStep = e.GetValue("SizeStep", 5);
        
        Description = e.GetValue<string>("Description");
        
        PlayerClassType = e.GetValue<ushort>("PlayerClassType");
        Skin = e.GetValue<bool>("Skin");

        SlotTypes = [];
        var slotTypes = e.GetValue<string>("SlotTypes");
        if (slotTypes != null) {
            foreach (var slot in slotTypes.Split(',')) {
                SlotTypes.Add(int.Parse(slot));
            }
        }

        Equipment = [];
        var equipment = e.GetValue<string>("Equipment");
        if (equipment != null) {
            foreach (var equip in equipment.Split(',')) {
                var clean = equip.Replace(" ", "");
                if (clean == "-1") {
                    Equipment.Add(null);
                    continue;
                }
                
                var objectType = Convert.ToUInt16(clean, XmlUtil.GetBase(clean));
                Equipment.Add(objectType);
            }
        }
    }
}