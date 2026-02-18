using System.Xml.Linq;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Networking.Packets.Incoming;
using Common;

namespace AlloyClient.Assets.XmlStructs;

public sealed class ProjectileProperties {
    
    public int BulletType {get; private set;}
    public string ObjectId {get; private set;}
    public float LifetimeMs {get; private set;}
    public float Speed {get; private set;}
    public float RealSpeed {get; private set;}
    public int Size {get; private set;}
    public int MinDamage {get; private set;}
    public int MaxDamage {get; private set;}
    public ConditionEffectIndex[] Effects {get; private set;}
    public bool MultiHit {get; private set;}
    public bool PassesCover {get; private set;}
    public bool ArmorPiercing {get; private set;}
    public bool ParticleTrail {get; private set;}
    public bool Wavy {get; private set;}
    public bool Parametric {get; private set;}
    public bool Boomerang {get; private set;}
    public float Amplitude {get; private set;}
    public float Frequency {get; private set;}
    public float Magnitude {get; private set;}
    public bool NoRotation {get; private set;}
    public uint ParticleTrailColor {get; private set;}
    public int ParticleTrailLifetime {get; private set;}
    public float ParticleTrailIntensity {get; private set;}

    private ProjectileProperties() {}
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

    public static ProjectileProperties FromEnemyShoot(EnemyShoot shoot) {
        return new ProjectileProperties() {
            BulletType = shoot.ProjType,
            LifetimeMs = shoot.Lifetime,
            MinDamage = shoot.Damage,
            MaxDamage = shoot.Damage,
            MultiHit = shoot.MultiHit,
            PassesCover = shoot.PassesCover,
            ArmorPiercing = shoot.ArmorPiercing
        };
    }
}