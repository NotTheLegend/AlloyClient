using System;
using System.Collections.Generic;
using System.Threading;
using AlloyClient.Assets;
using AlloyClient.Game.Components;
using AlloyClient.Game.Components.Hud;
using AlloyClient.Game.Objects;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.ParticleEffects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.Types;
using AlloyClient.Rendering.VertexData;
using AlloyClient.UiLib.Signals;
using AlloyClient.Utils;
using Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public static class Map {
    public const int TileRenderDistance = 20;

    public static GameTime LastGameTime;

    public static GameSprite GameSprite;

    public static double CurrentTime;

    public static int Width;
    public static int Height;
    public static string Name;
    public static string DisplayName;
    public static int Difficulty;
    public static uint Seed;
    public static int Background;
    public static bool AllowPlayerTeleport;
    public static bool ShowDisplays;

    private static MapTile[,] _tiles;
    public static readonly RenderStorage EntityStorage = new();
    public static readonly Dictionary<int, Player> Players = new();
    public static readonly Dictionary<int, Entity> Entities = new(); // todo: add players to separate dic for minimap prio
    public static readonly Dictionary<int, Entity> InteractiveObjects = new();
    
    public static readonly List<ParticleEffect> ParticleGenerators = [];

    public static int ParticleGenCount;

    private static readonly List<Projectile> Projectiles = [];

    public static int LocalPlayerId;
    public static Player LocalPlayer;

    public static int LastTickId;

    public static Signal<Player> OnPlayerUpdate = new();

    private static int _particleCount;
    private static readonly ParticleData[] Particles = new ParticleData[30000];

    public static void InitMap(int width, int height, string name, string display, int diff, uint seed, int background, bool allowTp, bool showDisplays) {
        Width = width;
        Height = height;
        Name = name;
        DisplayName = display;
        Difficulty = diff;
        Seed = seed;
        Background = background;
        AllowPlayerTeleport = allowTp;
        ShowDisplays = showDisplays;

        _tiles = new MapTile[width + 1, height + 1];
        
        Minimap.OnNewMap.Dispatch(width, height);
    }

    public static void Update(double time, double dt) {
        CurrentTime = time;
        _particleCount = 0;
        var fullMatrix = Camera.WorldMatrix * Camera.ViewMatrix * Camera.ProjectionMatrix;
        var matrix = new DepthMatrix(fullMatrix);

        foreach (var (objectId, entity) in Entities) {
            if (!entity.Update(time, dt)) {
                Entities.Remove(objectId);
            }

            entity.UpdateVisibility(ref fullMatrix);
        }

        for (var i = ParticleGenCount - 1; i >= 0; i--) {
            var gen = ParticleGenerators[i];
            if (gen.Update(time, dt)) {
                continue;
            }

            ParticleGenCount--;
            ParticleGenerators[i] = ParticleGenerators[ParticleGenCount];
            ParticleGenerators[ParticleGenCount] = null;
        }
        
        for (var i = Projectiles.Count - 1; i >= 0; i--) {
            var proj = Projectiles[i];
            if (proj.Update(time, dt, matrix))
                continue;
            ObjectPools.Projectiles.Push(proj);
            EntityStorage.Remove(proj);

            var idx = Projectiles.Count - 1;
            Projectiles[i] = Projectiles[idx];
            Projectiles.RemoveAt(idx);
        }
    }
    
    private static Vector2 _lastPosition = Vector2.Zero;

    public static void Draw(GameTime gameTime) {
        if (LocalPlayer == null) return;

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        LastGameTime = gameTime;

        Render.SetShaderParams(gameTime);

        #region Tile
        
        var pos = Vector2.Floor(LocalPlayer.Position);
        if (pos != _lastPosition) {
            _lastPosition = pos;
            
            Render.StartNewDrawTile();

            for (var x = -TileRenderDistance; x < TileRenderDistance; x++) {
                for (var y = -TileRenderDistance; y < TileRenderDistance; y++) {
                    if (x * x + y * y >= TileRenderDistance * TileRenderDistance) continue;

                    var tile = GetTile(x + (int) LocalPlayer.Position.X, y + (int) LocalPlayer.Position.Y);
                    if (tile != null && tile.Type != 0xFF)
                        tile.DrawTile();
                }
            }

            Render.EndNewDrawTile();
        }

        Render.DrawTiles();

        #endregion

        //TODO: use tile shader for these
        /* #region GroundObjects

        if (EntityStorage.TryGetValue(ModelType.PbTile, out var ground)) {
            Render.StartDrawEntity();
            Render.SetEntityModel(ModelType.PbTile);

            foreach (var type in ground) {
                if (type.Visible)
                    type.Draw();
            }

            Render.FlushBufferEntity();
        }

        #endregion */

        #region Shadows

        if (EntityStorage.TryGetValue(ModelType.PbObject, out var go)) {
            Render.StartDrawShadow();

            foreach (var type in go) {
                type.DrawShadow();
            }

            Render.EndShadowDraw();
        }

        #endregion

        GL.Enable(EnableCap.DepthTest);

        #region Particles

        Render.DrawParticles(Particles, _particleCount);

        #endregion
        
        GL.Enable(EnableCap.CullFace);

        #region Entities
        
        Render.StartDrawModel();

        foreach (var kvp in EntityStorage) {
            if (kvp.Key == ModelType.Null || kvp.Key == ModelType.PbObject) continue;

            Render.SetEntityModel(kvp.Key);

            foreach (var entity in kvp.Value) {
                if (entity.Visible) {
                    entity.Draw();
                    Render.LastDrawCountEntities++;
                }

            }

            Render.FlushBufferModel();
        }

        return;
        Render.StartDrawEntity();

        if (EntityStorage.TryGetValue(ModelType.PbObject, out var entities)) {
            Render.SetEntityModel(ModelType.PbObject);
            entities.Sort();

            foreach (var type in entities) {
                if (type.Visible) {
                    type.Draw();
                    Render.LastDrawCountEntities++;
                }
            }

            Render.FlushBufferEntity();
        }

        #endregion


    }

    public static MapTile GetTile(Vector2 position) => GetTile((int)position.X, (int)position.Y);

    public static MapTile GetTile(int x, int y) {
        if (x < 0 || x > Width || y < 0 || y > Height) {
            return null;
        }

        try {
            var tile = _tiles[x, y];

            if (tile != null) {
                return tile;
            }

            tile = new MapTile(x, y);
            _tiles[x, y] = tile;

            return tile;
        } catch (IndexOutOfRangeException) {
            return null;
        }
    }

    public static void SetTileData(int x, int y, ushort type) {
        var tile = GetTile(x, y);
        if (tile == null) {
            return;
        }

        tile.SetType(type);
    }

    public static void AddParticleEffect(ParticleEffect effect) {
        if (ParticleGenCount == ParticleGenerators.Count) {
            ParticleGenerators.Add(effect);
        } else {
            ParticleGenerators[ParticleGenCount] = effect;
        }
        
        ParticleGenCount++;
    }

    public static void AddEntity(Entity en, Position position) {
        if (!Entities.TryAdd(en.ObjectId, en))
            return;

        EntityStorage.Add(en);

        if (en is Player p) {
            if(p.ObjectId != LocalPlayerId)
                Players.TryAdd(p.ObjectId, p);
            p.Ignored = PartyData.IgnoredPlayers.Contains(p.AccountId);
            p.Locked = PartyData.LockedPlayers.Contains(p.AccountId);
        }
            

        if (InteractPanel.IsInteractiveObject(en))
            InteractiveObjects.TryAdd(en.ObjectId, en);

        en.OnAddedToMap(position);
    }

    public static void RemoveEntity(int id) {
        if (!Entities.Remove(id, out var en)) 
            return;

        Players.Remove(id);
        InteractiveObjects.Remove(id);

        EntityStorage.Remove(en);
        en.OnRemovedFromMap();
    }

    public static void AddProjectile(Projectile proj) {
        Projectiles.Add(proj);
        EntityStorage.Add(proj);
    }

    public static void AddParticles(ParticleData[] particles, int count) {
        if (_particleCount + count > Particles.Length)
            count = Particles.Length - _particleCount;

        if (count < 1) return;
        
        Array.Copy(particles, 0, Particles, _particleCount, count);
        _particleCount += count;
    }

    public static void AddParticle(ParticleData particle) {
        if (_particleCount + 1 > Particles.Length) return;

        Particles[_particleCount] = particle;
        _particleCount++;
    }

    public static void Reset() {
        Camera.Reset();

        Height = 0;
        Name = null;
        DisplayName = null;
        Difficulty = 0;
        Seed = 0;
        Background = 0;
        AllowPlayerTeleport = false;
        ShowDisplays = false;

        PartyData.Clear();
        
        Entities.Clear();
        Players.Clear();
        InteractiveObjects.Clear();
        EntityStorage.Clear();
        
        Projectiles.Clear();

        LocalPlayerId = 0;
        LocalPlayer = null;

        LastTickId = 0;

        _tiles = null;
    }

    public static void OnLocalPlayerCreated(Entity entity) {
        if (LocalPlayer != null) {
            Logger.Error("Local player already exists");
            return;
        }

        if (entity is not Player player) {
            Logger.Error("Local player is not a player");
            return;
        }

        LocalPlayer = player;
        MinimapLayer.SetFocus(player);
        GameSprite.Hud.CreatePlayerDependentAssets();
        OnPlayerUpdate.Dispatch(player);
    }
}

public class RenderStorage : Dictionary<ModelType, List<RenderBase>> {
    public void Add(Entity entity) {
        var type = entity.RenderBaseType;
        if (!ContainsKey(type.ModelType)) {
            base[type.ModelType] = [];
        }

        base[type.ModelType].Add(type);

        switch (type) {
            case TypeWall w:
                Add(w.Top);
                break;
        }
    }
    
    public void Add(Projectile proj) {
        var type = proj.RenderBaseType;
        if (!ContainsKey(type.ModelType)) {
            base[type.ModelType] = [];
        }

        base[type.ModelType].Add(type);
    }

    private void Add(RenderBase type) {
        if (type == null) {
            return;
        }

        if (!ContainsKey(type.ModelType)) {
            base[type.ModelType] = [];
        }

        base[type.ModelType].Add(type);
    }

    public void Remove(Entity entity) {
        var type = entity.RenderBaseType;
        if (!ContainsKey(type.ModelType)) {
            return;
        }

        base[type.ModelType].Remove(type);

        if (type is TypeWall w) {
            Remove(w.Top);
        }
    }
    
    public void Remove(Projectile proj) {
        var type = proj.RenderBaseType;
        if (!ContainsKey(type.ModelType)) {
            return;
        }

        base[type.ModelType].Remove(type);
    }

    private void Remove(RenderBase type) {
        if (!ContainsKey(type.ModelType)) {
            return;
        }

        base[type.ModelType].Remove(type);
    }
}