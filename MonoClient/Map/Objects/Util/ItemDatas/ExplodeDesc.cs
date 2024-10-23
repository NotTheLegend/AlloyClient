using System;
using System.Xml.Linq;
using Common;
using MonoClient.Assets.XmlStructs;

namespace MonoClient.Objects.Util.ItemDatas;

public class ExplodeDesc : ItemData {

    public int NumProjectiles;
    public float ArcGap;
    public ProjectileDesc Projectile;
    public bool AimAtCursor;
    public int Cooldown;
    public float RotateAngle;
    public float AngleOffset;
    
    public ExplodeDesc(XElement xml) {
        NumProjectiles = xml.GetAttribute<int>("numProjectiles");
        ArcGap = xml.GetAttribute<float>("arcGap");
        Projectile = new ProjectileDesc(xml.Element("Projectile"));
        AimAtCursor = xml.GetAttribute<bool>("aimAtCursor");
        Cooldown = xml.GetAttribute<int>("coolDown");
        RotateAngle = xml.GetAttribute<float>("rotateAngle");
        if (RotateAngle != 0) {
            RotateAngle *= MathF.PI / 180f;
        }
        AngleOffset = xml.GetAttribute<float>("angleOffset");
        if (AngleOffset != 0) {
            AngleOffset *= MathF.PI / 180f;
        }
    }
}