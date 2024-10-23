using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Common;

namespace MonoClient.Assets.XmlStructs;

public class ProjectileProperties {
    public readonly int BulletType;
    public readonly string ObjectId;
    public readonly float LifetimeMs;
    public readonly float Speed;
    public readonly float RealSpeed;
    public readonly int Size;
    public readonly int MinDamage;
    public readonly int MaxDamage;
    public readonly List<uint> Effects;
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
    public readonly bool FaceDir;
    public readonly bool NoRotation;
    public readonly float Acceleration;
    public readonly float AccelerationDelay;
    public readonly float SpeedClamp;
    public readonly float MaxProjTravel;
    public readonly bool Homing;
    public readonly float HomingAcquireRange;
    public readonly float HomingMaxDeflection;
    public readonly float HomingStrength;
    public readonly XElement Xml;

    public ProjectileProperties(XElement e) {
        Xml = e;
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
        FaceDir = e.HasElement("FaceDir");
        NoRotation = e.HasElement("NoRotation");
        Acceleration = e.GetValue<float>("Acceleration");
        AccelerationDelay = e.GetValue<float>("AccelerationDelay");
        SpeedClamp = e.GetValue("SpeedClamp", -1);
        Homing = e.HasElement("Homing");
        HomingAcquireRange = e.GetValue<float>("HomingAcquireRange", 32);
        HomingMaxDeflection = e.GetValue<float>("HomingMaxDeflection", 180);
        HomingStrength = e.GetValue<float>("HomingStrength", 1);

        if (Acceleration == 0 || LifetimeMs < AccelerationDelay) {
            MaxProjTravel = Speed * LifetimeMs;
        }
        else {
            var baseSpeed = Speed;
            var speedNeeded = Math.Abs(SpeedClamp - RealSpeed);
            var timeTillMaxSpeed = speedNeeded / Math.Abs(Acceleration) * 1000;
            if (Acceleration < 0 && SpeedClamp > RealSpeed || Acceleration > 0 && SpeedClamp < RealSpeed) {
                timeTillMaxSpeed = LifetimeMs;
            }

            var timeAccelerating = Math.Min(timeTillMaxSpeed, LifetimeMs - AccelerationDelay);
            var timeClamped = Math.Max(0, LifetimeMs - AccelerationDelay - timeTillMaxSpeed);
            var clampedSpeed = SpeedClamp / 10000;
            MaxProjTravel = AccelerationDelay * baseSpeed +
                            timeAccelerating * baseSpeed + (timeAccelerating * timeAccelerating / 1000) * (1 / 2f) * (Acceleration / 10000f) +
                            timeClamped * clampedSpeed;
        }
    }
}