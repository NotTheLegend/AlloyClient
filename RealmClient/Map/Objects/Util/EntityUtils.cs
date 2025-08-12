using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace RealmClient.Objects.Util;

public static class EntityUtils {
    public static float CalculateDistance(Vector2 point1, Vector2 point2) {
        float deltaX = point1.X - point2.X;
        float deltaY = point1.Y - point2.Y;
        return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public static List<Entity> FindEntitiesInRadius(Entity player, IEnumerable<Entity> entities, float radius) {
        List<Entity> entitiesInRadius = new List<Entity>();

        foreach (Entity entity in entities) {
            float distance = CalculateDistance(player.Position, entity.Position);

            if (distance <= radius) {
                entitiesInRadius.Add(entity);
            }
        }

        return entitiesInRadius;
    }
    
    public static Entity FindClosestEntityInRadius(Entity player, IEnumerable<Entity> entities, float radius) {
        Entity closestEntity = null;
        var closestDistance = float.MaxValue;

        foreach (var entity in entities) {
            var distance = CalculateDistance(player.Position, entity.Position);
            
            if (entity == Map.LocalPlayer)
                continue;
            
            if (distance <= radius && distance < closestDistance) {
                closestDistance = distance;
                closestEntity = entity;  
            }
        }
        
        return closestEntity;
    }
    
    public static Entity FindClosestSpecialInRadius(Entity player, IEnumerable<Entity> entities, float radius) {
        Entity closestEntity = null;
        var closestDistance = float.MaxValue;

        foreach (var entity in entities) {
            var distance = CalculateDistance(player.Position, entity.Position);
            
            if (IsCharacter(entity))
                continue;
            
            if (distance <= radius && distance < closestDistance) {
                closestDistance = distance;
                closestEntity = entity;  
            }
        }
        
        return closestEntity;
    }
    
    public static Entity GetClosestPlayer(Vector2 position, float radius) {
        var entities = Map.Entities.Values;
        Entity en = null;
        var enDist = float.MaxValue;

        foreach (var entity in entities) {
            if (entity is not Player)
                continue;
            
            var dist = CalculateDistance(position, entity.Position);
            
            if (dist > radius || dist >= enDist)
                continue;
            en = entity;
            enDist = dist;
        }

        return en;
    }
    
    public static Entity GetClosestEnemy(Vector2 position, float radius) {
        var entities = Map.Entities.Values;
        Entity en = null;
        var enDist = float.MaxValue;

        foreach (var entity in entities) {
            if (!entity.Properties.IsEnemy)
                continue;
            
            var dist = CalculateDistance(position, entity.Position);
            
            if (dist > radius || dist >= enDist)
                continue;
            en = entity;
            enDist = dist;
        }

        return en;
    }

    public static bool IsCharacter(Entity entity) => entity.Properties.IsEnemy || entity.Properties.IsAlly || entity.Properties.IsPlayer || entity.Properties.Class == "Character";
}