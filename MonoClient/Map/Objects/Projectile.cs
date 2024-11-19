using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Assets.XmlStructs;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.Objects;

public class Projectile : Entity {
    
    public ProjectileProperties ProjDesc;
    public Entity Owner;
    
    public float Angle;
    public float AngleCorrection;

    public Vector2 StartPosition;
    
    public float StartTime;

    public bool PendingRemoval = false;
    
    // Used for smart projectile rotation
    private Vector2 _previousPosition = Vector2.Zero;

    public override bool Update(double time, double dt) {
        var elapsed = (float)time - StartTime;
        
        Position = PositionAt(elapsed);

        // Use smart projectile rotation if the projectile does not have its own rotation speed or the NoRotation tag
        if (Properties.Rotation != 0) {
            Rotation = elapsed / Properties.Rotation; 
        } 
        else if (!ProjDesc.NoRotation) {
            var direction = Position - _previousPosition;
            var angle = MathF.Atan2(direction.Y, direction.X);
            Rotation = angle + Camera.CameraAngle + AngleCorrection;
        }
        
        _previousPosition = Position;
        
        if (elapsed > ProjDesc.LifetimeMs || !MoveTo(Position.X, Position.Y) || HitTest(this)) {
            PendingRemoval = true;
            Map.EntityStorage.Remove(this);
        }

        RenderBaseType.SetPosition(Position.X, Position.Y, Z);
        return true;
    }

    private Vector2 PositionAt(float elapsed) {
        var finalPosition = StartPosition;
        
        // Projectiles cannot have multiple pattern effects at the same time
        // I'll keep it behaving like flash client
        
        if (ProjDesc.Wavy) {
            return ApplyWavyEffect(finalPosition, elapsed);
        }

        if (ProjDesc.Parametric) {
            return ApplyParametricEffect(finalPosition, elapsed);
        }

        if (ProjDesc.Boomerang) {
            return ApplyBoomerangEffect(finalPosition, elapsed);
        }
        
        // Straight projectile
        float distance = elapsed * ProjDesc.Speed;
        
        finalPosition.X += distance * MathF.Cos(Angle);
        finalPosition.Y += distance * MathF.Sin(Angle);

        return finalPosition;
    }

    private Vector2 ApplyWavyEffect(Vector2 origin, float elapsed) {
        float distance = elapsed * ProjDesc.Speed;
        float phase = ObjectId % 2 * MathF.PI;
        
        float period = 6 * MathF.PI;
        float amplitude = MathF.PI / 64;
        float theta = Angle + amplitude * MathF.Sin(phase + period + elapsed / 1000);

        origin.X += distance * MathF.Cos(theta);
        origin.Y += distance * MathF.Cos(theta);
            
        return origin;
    }

    private Vector2 ApplyParametricEffect(Vector2 origin, float elapsed) {
        float t = elapsed / ProjDesc.LifetimeMs * 2 * MathF.PI;

        float x = MathF.Sin(t) * (ObjectId % 2 == 0 ? -1 : 1);
        float y = MathF.Sin(2 * t) * (ObjectId % 4 < 2 ? 1 : -1);

        origin.X += (x * MathF.Cos(Angle) - y * MathF.Sin(Angle)) * ProjDesc.Magnitude;
        origin.Y += (x * MathF.Sin(Angle) + y * MathF.Cos(Angle)) * ProjDesc.Magnitude;

        return origin;
    }
    
    private Vector2 ApplyBoomerangEffect(Vector2 origin, float elapsed) {
        float distance = elapsed * ProjDesc.Speed;
        float phase = ObjectId % 2 * MathF.PI;
        
        float halfwayDistance = ProjDesc.LifetimeMs * ProjDesc.Speed / 2;
            
        if (distance > halfwayDistance) 
            distance = halfwayDistance - (distance - halfwayDistance);

        origin.X += distance * MathF.Cos(Angle);
        origin.Y += distance * MathF.Sin(Angle);

        if (ProjDesc.Amplitude != 0) {
            float deflection = ProjDesc.Amplitude *
                               MathF.Sin(phase + elapsed / ProjDesc.LifetimeMs * ProjDesc.Frequency * 2 * MathF.PI);
            origin.X += deflection * MathF.Cos(Angle + MathF.PI / 2);
            origin.Y += deflection * MathF.Sin(Angle + MathF.PI / 2);
        }
        
        return origin;
    }

    public void SetRotation() {
        if (Properties.AngleCorrection != 0)
            AngleCorrection = Properties.AngleCorrection * MathF.PI / 4;
    }
}