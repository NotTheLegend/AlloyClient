using System;
using System.Linq;
using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class ProjectileDesc : ItemData {
    
    public int BulletType;
    public string ObjectId;
    public float Speed;
    public float RealSpeed;
    public float LifetimeMS;
    public bool MultiHit;
    public bool PassesCover;
    public bool Parametric;
    public bool Boomerang;
    public bool ArmorPiercing;
    public bool Wavy;
    public ConditionEffectDesc[] Effects;
    public float Frequency;
    public float Magnitude;
    public bool FaceDir;
    public bool NoRotation;
    public int Acceleration;
    public int AccelerationDelay;
    public int SpeedClamp;
    public float MaxProjTravel;
    public string Sound;
    public ExplodeDesc Explode;
    public float Radius;
    public float Circles;
    public bool BeginNormal;
    public bool CounterClockwise;
    public int Phase;
    public bool Homing;
    public float HomingMaxDeflection;
    public float HomingAcquireRange;
    public float HomingStrength;
    public bool ContactDamage;
    public bool DontShowEffectText;
    public int Size;
    public ParticleTrailDesc Trail;
    public int AlternativeMinDamage;
    public int AlternativeMaxDamage;
    public float HitRadius = 0.5f;
    public int CircleTime;
    public int ResetHitsAfter;
    public bool FaceAwayFromMid;

    private int _minDamage;
    public int MinDamage {
        get => (int)(_minDamage + (_essenceUpgrades?.DamageBoost ?? 0));
    }
    
    private int _maxDamage;
    public int MaxDamage {
        get => (int)(_maxDamage + (_essenceUpgrades?.DamageBoost ?? 0));
    }
    
    private float _amplitude;
    public float Amplitude {
        get => _amplitude + (_essenceUpgrades?.Amplitude ?? 0);
    }

    private readonly EssenceUpgradeManager _essenceUpgrades;

    public ProjectileDesc(XElement xml, ProjectileDesc desc = null, EssenceUpgradeManager essenceUpgrades = null) {
        _essenceUpgrades = essenceUpgrades;

        BulletType = desc?.BulletType ?? xml.GetAttribute<int>("id");
        ObjectId = desc?.ObjectId ?? xml.GetValue<string>("ObjectId");
        RealSpeed = desc?.RealSpeed ?? xml.GetValue<float>("Speed");
        Speed = RealSpeed / 10000.0f;

        if (desc != null) {
            _minDamage = desc.MinDamage;
            _maxDamage = desc.MaxDamage;
        } else {
            if (xml.HasElement("Damage")) {
                _minDamage = xml.GetValue<int>("Damage");
                _maxDamage = _minDamage;
            } else {
                _minDamage = xml.GetValue<int>("MinDamage");
                _maxDamage = xml.GetValue<int>("MaxDamage");
            }
        }
        
        LifetimeMS = desc?.LifetimeMS ?? xml.GetValue<float>("LifetimeMS");
        MultiHit = desc?.MultiHit ?? xml.GetValue<bool>("MultiHit");
        PassesCover = desc?.PassesCover ?? xml.GetValue<bool>("PassesCover");
        Parametric = desc?.Parametric ?? xml.GetValue<bool>("Parametric");
        Boomerang = desc?.Boomerang ?? xml.GetValue<bool>("Boomerang");
        ArmorPiercing = desc?.ArmorPiercing ?? xml.GetValue<bool>("ArmorPiercing");
        Wavy = desc?.Wavy ?? xml.GetValue<bool>("Wavy");

        if (desc != null) {
            Effects = desc.Effects;
        } else if (xml.HasElement("ConditionEffect")) {
            Effects = xml.Elements("ConditionEffect").Select(x => new ConditionEffectDesc(x.Value, x.GetAttribute<float>("duration"))).ToArray();
        }

        _amplitude = desc?.Amplitude ?? xml.GetValue<float>("Amplitude");
        Frequency = desc?.Frequency ?? xml.GetValue<float>("Frequency", 1);
        Magnitude = desc?.Magnitude ?? xml.GetValue<float>("Magnitude", 3);
        FaceDir = desc?.FaceDir ?? xml.GetValue<bool>("FaceDir");
        NoRotation = desc?.NoRotation ?? xml.GetValue<bool>("NoRotation");
        
        Acceleration  = desc?.Acceleration ?? xml.GetValue<int>("Acceleration");
        AccelerationDelay = desc?.AccelerationDelay ?? xml.GetValue<int>("AccelerationDelay");
        SpeedClamp = desc?.SpeedClamp ?? xml.GetValue<int>("SpeedClamp");

        if (Acceleration == 0 || LifetimeMS < AccelerationDelay) {
            MaxProjTravel = Speed * LifetimeMS;
        } else {
            var baseSpeed = Speed;
            var speedNeeded = MathF.Abs(SpeedClamp - RealSpeed);
            var timeTillmaxSpeed = speedNeeded / MathF.Abs(Acceleration) * 1000.0f;
            if ((Acceleration < 0 && SpeedClamp > RealSpeed) || (Acceleration > 0 && SpeedClamp < RealSpeed)) {
                timeTillmaxSpeed = LifetimeMS;
            }
            var timeAccelerating = MathF.Min(timeTillmaxSpeed, LifetimeMS - AccelerationDelay);
            var timeClamped = MathF.Max(0, LifetimeMS - AccelerationDelay - timeTillmaxSpeed);
            var clampedSpeed = SpeedClamp / 10000.0f;
            MaxProjTravel = AccelerationDelay * baseSpeed + timeAccelerating * baseSpeed +
                            (timeAccelerating * timeAccelerating / 1000.0f) * (1 / 2) * (Acceleration / 10000.0f) +
                            timeClamped * clampedSpeed;
        }
        
        Sound = desc?.Sound ?? xml.GetValue<string>("Sound");
        if (desc != null) {
            Explode = desc.Explode;
        } else if (xml.HasElement("Explode")) {
            Explode = new ExplodeDesc(xml.Element("Explode"));
        }
        
        DontShowEffectText = desc?.DontShowEffectText ?? xml.GetValue<bool>("DontShowEffectText");
        Size = desc?.Size ?? xml.GetValue<int>("Size");
        if (xml.HasElement("ParticleTrail")) {
            Trail = new ParticleTrailDesc(xml.Element("ParticleTrail"));
            if (desc?.Trail != null) {
                Trail.Color = desc.Trail.Color;
                Trail.LifetimeMS = desc.Trail.LifetimeMS;
                Trail.Size = desc.Trail.Size;
            }
        }

        if (desc != null) {
            AlternativeMinDamage = desc?.AlternativeMinDamage ?? 0;
            AlternativeMaxDamage = desc?.AlternativeMaxDamage ?? 0;
        } else {
            if (xml.HasElement("AlternativeDamage")) {
                AlternativeMinDamage = xml.GetValue<int>("AlternativeDamage");
                AlternativeMaxDamage = AlternativeMinDamage;
            } else {
                AlternativeMinDamage = xml.GetValue<int>("AlternativeMinDamage");
                AlternativeMaxDamage = xml.GetValue<int>("AlternativeMaxDamage");
            }
        }
        
        Radius = desc?.Radius ?? xml.GetValue<float>("Radius");
        Circles = desc?.Circles ?? xml.GetValue<float>("Circles", 1);
        BeginNormal = desc?.BeginNormal ?? xml.GetValue<bool>("BeginNormal");
        CircleTime = desc?.CircleTime ?? xml.GetValue<int>("CircleTime");
        CounterClockwise = desc?.CounterClockwise ?? xml.GetValue<bool>("CounterClockwise");
        
        // Calculate speed if CircleTime is specified (the time it takes for a full circle to finish)
        if (Speed == 0 && CircleTime > 0) {
            const float twoPi = MathF.PI * 2;
            RealSpeed = (twoPi / Radius) / CircleTime * 10000;
            Speed = RealSpeed / 10000.0f;
        }
        // Calculate the lifetime if not specified
        if (Radius > 0 && LifetimeMS == 0 && desc == null) {
            if (BeginNormal) {
                LifetimeMS += Radius / MathF.Abs(Speed);
            }

            LifetimeMS += (2 * MathF.PI * Radius) / MathF.Abs(Speed) * Circles;
        }
        
        Phase = desc?.Phase ?? xml.GetValue<int>("Phase");
        Homing = desc?.Homing ?? xml.GetValue<bool>("Homing");
        HomingMaxDeflection = desc?.HomingMaxDeflection ?? 0;
        HomingAcquireRange  = desc?.HomingAcquireRange ?? 0;
        HomingStrength = desc?.HomingStrength ?? 0;
        ContactDamage = desc?.ContactDamage ?? xml.GetValue<bool>("ContactDamage");
        HitRadius = desc?.HitRadius ?? xml.GetValue<int>("HitRadius");
        ResetHitsAfter = desc?.ResetHitsAfter ?? xml.GetValue<int>("ResetHitsAfter");
        FaceAwayFromMid = desc?.FaceAwayFromMid ?? false;
    }
}