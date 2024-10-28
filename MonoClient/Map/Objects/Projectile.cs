using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Assets.XmlStructs;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.Objects;

public class Projectile : Entity {
    public float StartX;
    public float StartY;
    
    public float Angle;
    public float AngleCorrection;

    public ProjectileProperties ProjDesc;
    public float StartTime;

    public bool PendingRemoval = false;

    public override bool Update(double time, double dt) {
        var elapsed = (float)time - StartTime;
        
        // Speed * dt looks correct but maybe need tweaking
        var dist = ProjDesc.Speed * (float)dt;
        
        if (Properties.Rotation != 0) {
            Rotation = (float)Map.LastGameTime.TotalGameTime.TotalMilliseconds / Properties.Rotation; 
        }
        else {
            Rotation = Angle + Camera.CameraAngle + AngleCorrection;
        }
        
        Position.X += dist * MathF.Cos(Angle);
        Position.Y += dist * MathF.Sin(Angle);
        
        if (elapsed > ProjDesc.LifetimeMs || !MoveTo(Position.X, Position.Y)) {
            PendingRemoval = true;
            Map.EntityStorage.Remove(this);
        }

        RenderBaseType.SetPosition(Position.X, Position.Y, Z);
        return true;
    }

    public void SetRotation() {
        if (Properties.AngleCorrection != 0)
            AngleCorrection = Properties.AngleCorrection * MathF.PI / 4;
    }
}