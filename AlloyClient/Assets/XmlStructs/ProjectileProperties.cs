using System.Xml.Linq;
using AlloyClient.Game.Objects.Util;
using Common;

namespace AlloyClient.Assets.XmlStructs;

public sealed class ProjectileProperties {
    
    public readonly int BulletType;
    public readonly string ObjectId;
    public readonly float LifetimeMs;
    public readonly float Speed;
    public readonly float RealSpeed;
    public readonly int Size;
    public readonly int MinDamage;
    public readonly int MaxDamage;
    public readonly ConditionEffectIndex[] Effects;
    public readonly bool MultiHit;
    public readonly bool PassesCover;
    public readonly bool ArmorPiercing;
    public readonly bool ParticleTrail;
    public readonly bool Wavy;
    public readonly bool Parametric;
    public readonly bool Boomerang;
    public readonly float Amplitude;
    public readonly float Frequency;
    public readonly float Magnitude;
    public readonly bool NoRotation;

    public readonly uint ParticleTrailColor;
    public readonly int ParticleTrailLifetime;
    public readonly float ParticleTrailIntensity;

    public ProjectileProperties(XElement e) {
        BulletType = e.GetAttribute<int>("id");
        ObjectId = e.GetValue<string>("ObjectId");
        LifetimeMs = e.GetValue<float>("LifetimeMS");
        RealSpeed = e.GetValue<float>("Speed");
        Speed = RealSpeed / 10000;
        Size = e.GetValue<int>("Size", -1);
        MinDamage = e.HasElement("Damage") ? e.GetValue<int>("Damage") : e.GetValue<int>("MinDamage");
        MaxDamage = e.HasElement("Damage") ? e.GetValue<int>("Damage") : e.GetValue<int>("MaxDamage");
        //Effects = e.Elements("ConditionEffect").Select(x => (uint) ConditionEffect.GetConditionEffectFromName(x.Value)).ToList();
        MultiHit = e.HasElement("MultiHit");
        PassesCover = e.HasElement("PassesCover");
        ArmorPiercing = e.HasElement("ArmorPiercing");
        ParticleTrail = e.HasElement("ParticleTrail");
        Wavy = e.HasElement("Wavy");
        Parametric = e.HasElement("Parametric");
        Boomerang = e.HasElement("Boomerang");
        Amplitude = e.GetValue<float>("Amplitude");
        Frequency = e.GetValue<float>("Frequency", 1);
        Magnitude = e.GetValue<float>("Magnitude", 3);
        NoRotation = e.HasElement("NoRotation");

        if (ParticleTrail) {
            var attr = e.Element("ParticleTrail");
            ParticleTrailColor = attr.GetValue<uint>("ParticleTrail", 0xFF00FF);
            ParticleTrailLifetime = attr.GetAttribute("lifetimeMS", 600);
            ParticleTrailIntensity = attr.GetAttribute("intensity", 0.3f);
        }
    }
}