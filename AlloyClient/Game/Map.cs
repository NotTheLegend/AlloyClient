using System;
using System.Collections.Generic;
using AlloyClient.Assets;
using AlloyClient.Game.Components.Hud;
using AlloyClient.Game.Objects;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.ParticleEffects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.Types;
using AlloyClient.Rendering.VertexData;
using Alloy.UiLib.Signals;
using Alloy.Engine;
using AlloyClient.Logging;
using AlloyClient.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public class TileMap {
    
    public const int ChunkSize = 16;
    public const int ChunkArea = ChunkSize * ChunkSize;
    public const int ChunkRenderData = ChunkArea * MapTile.MaxTileData;

    private int _width;
    private int _height;
    
    private readonly Dictionary<Vector2i, TileChunk> _chunks = [];

    public MapTile this[Vector2i coords] => Get(coords);

    public void SetDimensions(int width, int height) {
        _width = width;
        _height = height;
    }

    public void SetTileChange(Vector2i coords) {
        var chunkId = new Vector2i(coords.X / ChunkSize, coords.Y / ChunkSize);
        
        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            return;
        }
        
        chunk.SetDirty();
    }

    public bool GetChunkData(Vector2i chunkCoords, out ReadOnlySpan<TileData> data) {
        if (!_chunks.TryGetValue(chunkCoords, out var chunk)) {
            data = default;
            return false;
        }

        data = chunk.GetTileData();
        return true;
    }
    
    public void Clear() => _chunks.Clear();
    
    private MapTile Get(Vector2i coords) {
        if (coords.X < 0 || coords.X > _width || coords.Y < 0 || coords.Y > _height) {
            return null;
        }
        
        var chunkId = new Vector2i(coords.X / ChunkSize, coords.Y / ChunkSize);

        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            chunk = _chunks[chunkId] = new TileChunk();
        }

        return chunk.Get(coords);
    }
    
    private class TileChunk {
        
        private readonly MapTile[] _tiles = new MapTile[ChunkArea];
        
        private readonly TileData[] _data = new TileData[ChunkRenderData];

        private bool _dirty;

        private int _renderCount;

        public MapTile Get(Vector2i coords) {
            var index = (coords.Y % ChunkSize) * ChunkSize + (coords.X % ChunkSize);
            return _tiles[index] ?? (_tiles[index] = new MapTile(coords));
        }

        public void SetDirty() => _dirty = true;

        public ReadOnlySpan<TileData> GetTileData() {
            if (!_dirty) {
                return new ReadOnlySpan<TileData>(_data, 0, _renderCount);
            }

            var count = 0;
            foreach (var tile in _tiles) {
                if (tile is null || tile.Type == Const.DefaultTile) {
                    continue;
                }

                var chunkData = _data.AsSpan(count);
                var tileData = tile.DrawTile();
                
                tileData.CopyTo(chunkData);
                count += tileData.Length;
            }

            _renderCount = count;
            _dirty = false;
            
            return new ReadOnlySpan<TileData>(_data, 0, _renderCount);
        }
    }
}

public static class Map {
    
    public const int ViewRadiusX = 3;
    public const int ViewRadiusY = 2;
    public const int ViewDiameterX = ViewRadiusX * 2 + 1;
    public const int ViewDiameterY = ViewRadiusY * 2 + 1;
    public const int VisibleChunks = ViewDiameterX * ViewDiameterY;

    private readonly static ILogger Logger = ILogger.CreateLogger(nameof(Map));
    
    public const int TileRenderDistance = 20;

    public static GameTime LastGameTime;

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
    
    private readonly static TileMap Tiles = new ();
    public readonly static RenderStorage EntityStorage = new();
    public readonly static Dictionary<int, Player> Players = new();
    public readonly static Dictionary<int, Entity> Entities = new(); // todo: add players to separate dic for minimap prio
    public readonly static Dictionary<int, Entity> InteractiveObjects = new();
    
    public readonly static List<ParticleEffect> ParticleGenerators = [];

    public static int ParticleGenCount;

    private readonly static List<Projectile> Projectiles = [];

    public static int LocalPlayerId;
    public static Player LocalPlayer;

    public static int LastTickId;

    public readonly static Signal<Player> OnPlayerUpdate = new();

    private static int _particleCount;
    private readonly static ParticleData[] Particles = new ParticleData[30000];

    private readonly static List<VertexObject> RenderTargets = [];

    private readonly static HashSet<Vector2i> SightCircle = [];

    static Map() {
        for (var x = -TileRenderDistance; x < TileRenderDistance; x++) {
            for (var y = -TileRenderDistance; y < TileRenderDistance; y++) {
                if (x * x + y * y >= TileRenderDistance * TileRenderDistance) {
                    continue;
                }

                SightCircle.Add(new Vector2i(x, y));
            }
        }
    }

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
        
        Minimap.OnNewMap.Dispatch(width, height);
        Tiles.SetDimensions(width, height);
    }

    public static void Update(in GameTime gameTime, in Camera camera) {
        CurrentTime = gameTime.TotalMs;
        var time = gameTime.TotalMs;
        var dt = gameTime.ElapsedMs;
        
        _particleCount = 0;
        var fullMatrix = camera.Matrix;
        var matrix = new DepthMatrix(camera.Matrix);

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
            if (proj.Update(gameTime))
                continue;
            
            ObjectPools.Projectiles.Push(proj);

            var idx = Projectiles.Count - 1;
            Projectiles[i] = Projectiles[idx];
            Projectiles.RemoveAt(idx);
        }
    }

    public static void FixedUpdate(in GameTime gameTime) {
        foreach (var projectile in Projectiles) {
            projectile.FixedUpdate(in gameTime);
        }
    }
    

    private readonly static List<TileData> VisibleTiles = new (Render.TileBufferSize);

    public static void Draw(in GameTime gameTime, in Camera camera) {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        LastGameTime = gameTime;

        #region Tile
        
        VisibleTiles.Clear();
        
        var camChunkPos = new Vector2i((int)camera.Position.X / TileMap.ChunkSize, (int)camera.Position.Y / TileMap.ChunkSize) - new Vector2i(ViewRadiusX, ViewRadiusY);

        for (var i = 0; i < VisibleChunks; i++) {
            if (!Tiles.GetChunkData(camChunkPos + new Vector2i(i % ViewDiameterX, i / ViewDiameterX), out var data)) {
                continue;
            }
            
            VisibleTiles.AddRange(data); // should probably not do this but cant be bothered currently
        }
        
        Render.DrawTiles(VisibleTiles.AsReadOnlySpan());
        
        #endregion

        #region Shadows

        Render.StartDrawShadow();

        foreach (var type in EntityStorage[ModelType.PbObject]) {
            type.DrawShadow();
        }

        foreach (var projectile in Projectiles) {
            Render.DrawShadow(projectile.DrawShadow());
        }

        Render.EndShadowDraw();

        #endregion

        GL.Enable(EnableCap.DepthTest);

        #region Particles

        Render.DrawParticles(Particles, _particleCount);

        #endregion
        
        GL.Enable(EnableCap.CullFace);

        #region Entities
        
        RenderTargets.Clear();
        
        Render.StartDrawModel();

        for (var i = 0; i < EntityStorage.Types.Length; i++) {
            var type = (ModelType)i;
            var list = EntityStorage.Types[i];
            if (type == ModelType.Null || type == ModelType.PbObject) continue;

            Render.SetEntityModel(type);

            foreach (var entity in list) {
                if (entity.Visible) {
                    entity.Draw(RenderTargets, gameTime.TotalMs);
                    Render.LastDrawCountEntities++;
                }

            }

            Render.FlushBufferModel();
        }
        
        GL.Disable(EnableCap.CullFace);
        
        Render.StartDrawEntity();
        
        foreach (var type in EntityStorage[ModelType.PbObject]) {
            if (type.Visible) {
                type.Draw(RenderTargets, gameTime.TotalMs);
            }
        }

        foreach (var projectile in Projectiles) {
            RenderTargets.Add(projectile.Draw(in camera.DepthMatrix));
        }

        Render.FlushBufferEntity(RenderTargets);
        Render.LastDrawCountEntities += RenderTargets.Count;

        #endregion
    }

    public static MapTile LookupTile(Vector2 position) => LookupTile((int)position.X, (int)position.Y);

    public static bool LookupTile(int x, int y, out MapTile tile) => (tile = LookupTile(x, y)) != null;
    
    public static MapTile LookupTile(int x, int y) => LookupTile(new Vector2i(x, y));
    
    public static bool LookupTile(Vector2i position, out MapTile tile) => (tile = LookupTile(position)) != null;
    
    public static MapTile LookupTile(Vector2i position) => Tiles[position];

    private readonly static MapTile[] RebuildData = new MapTile[9];

    public static void SetTileData(int x, int y, ushort type) {
        if (!LookupTile(x, y, out var tile)) {
            return;
        }

        tile.SetType(type);
        
        for (var y1 = y - 1; y1 <= y + 1; y1++){
            for (var x1 = x - 1; x1 <= x + 1; x1++) {
                RebuildTile(LookupTile(x1, y1));
            }
        }
        
        Array.Clear(RebuildData);
    }

    private static void RebuildTile(MapTile tile) {
        if (tile is null) {
            return;
        }
        
        var idx = 0;
        for (var y1 = tile.Y - 1; y1 <= tile.Y + 1; y1++){
            for (var x1 = tile.X - 1; x1 <= tile.X + 1; x1++) {
                RebuildData[idx++] = LookupTile(x1, y1);
            }
        }
        
        tile.Rebuild(RebuildData);
        Tiles.SetTileChange(new Vector2i(tile.X, tile.Y));
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
        
        Tiles.Clear();
    }

    public static void OnLocalPlayerCreated(Entity entity) {
        if (LocalPlayer != null) {
            Logger.Log(LogLevel.Error, "Local player already exists");
            return;
        }

        if (entity is not Player player) {
            Logger.Log(LogLevel.Error, "Local player is not a player");
            return;
        }

        LocalPlayer = player;
        MinimapLayer.SetFocus(player);
        GameScreen.GameSprite.CreatePlayerDependentAssets();
        OnPlayerUpdate.Dispatch(player);
    }
}

public class RenderStorage {
    public readonly HashSet<RenderBase>[] Types = new HashSet<RenderBase>[(int)ModelType.Count];

    public HashSet<RenderBase> this[ModelType modelType] => Types[(int)modelType];
    
    public RenderStorage() {
        for (var i = 0; i < Types.Length; i++)
            Types[i] = new HashSet<RenderBase>();
    }

    public void Clear() {
        for (var i = 0; i < Types.Length; i++)
            Types[i].Clear();
    }
    
    public void Add(Entity entity) {
        var type = entity.RenderBaseType;
        var list = Types[(int)type.ModelType];
        list.Add(type);

        switch (type) {
            case TypeWall w:
                Add(w.Top);
                break;
        }
    }

    private void Add(RenderBase type) {
        if (type == null) {
            return;
        }

        var list = Types[(int)type.ModelType];
        list.Add(type);
    }

    public void Remove(Entity entity) {
        var type = entity.RenderBaseType;
        var list = Types[(int)type.ModelType];

        list.Remove(type);

        if (type is TypeWall w) {
            Remove(w.Top);
        }
    }

    private void Remove(RenderBase type) {
        var list = Types[(int)type.ModelType];

        list.Remove(type);
    }
}