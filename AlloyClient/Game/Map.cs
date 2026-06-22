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
using TileData = AlloyClient.Rendering.VertexData.TileData;

namespace AlloyClient.Game;

public readonly struct TileMap {
    public const int ChunkSize = 16;
    
    private readonly Dictionary<Vector2i, MapChunk> _tiles = [];
    
    public TileMap(){}
    
    public MapTile this[Vector2i coords] {
        get {
            var chunkId = new Vector2i(coords.X / ChunkSize, coords.Y / ChunkSize);
            var chunkCoords = new Vector2i(coords.X % ChunkSize, coords.Y % ChunkSize);
            return _tiles.TryGetValue(chunkId, out var chunk) ? chunk[chunkCoords] : null;
        }
        set {
            var chunkId = new Vector2i(coords.X / ChunkSize, coords.Y / ChunkSize);
            var chunkCoords = new Vector2i(coords.X % ChunkSize, coords.Y % ChunkSize);

            if (!_tiles.TryGetValue(chunkId, out var chunk)) {
                chunk = _tiles[chunkId] = new MapChunk();
            }

            chunk[chunkCoords] = value;
        }
    }

    public void Clear() => _tiles.Clear();

    private readonly struct MapChunk {
        
        private readonly MapTile[] _tiles = new MapTile[ChunkSize * ChunkSize];

        public MapChunk() { }

        public MapTile this[Vector2i coords] {
            get => _tiles[coords.Y * ChunkSize + coords.X];
            set => _tiles[coords.Y * ChunkSize + coords.X] = value;
        }
    }
}

public static class Map {

    private static readonly ILogger Logger = ILogger.CreateLogger(nameof(Map));
    
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
    
    private static readonly TileMap Tiles = new ();
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

    public static readonly Signal<Player> OnPlayerUpdate = new();

    private static int _particleCount;
    private static readonly ParticleData[] Particles = new ParticleData[30000];

    private static readonly List<VertexObject> RenderTargets = [];

    private static readonly HashSet<Vector2i> SightCircle = [];

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
    

    private static readonly List<TileData> VisibleTiles = new (Render.TileBufferSize);

    public static void Draw(in GameTime gameTime, in Camera camera) {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        LastGameTime = gameTime;

        #region Tile
        
        VisibleTiles.Clear();

        foreach (var position in SightCircle) {
            if (LookupTile(position + camera.Position, out var tile) && tile.Type != Const.DefaultTile) {
                VisibleTiles.AddRange(tile.DrawTile());
            }
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
    
    public static bool LookupTile(Vector2 position, out MapTile tile) => (tile = LookupTile(position)) != null;

    public static MapTile LookupTile(Vector2 position) => LookupTile((int)position.X, (int)position.Y);

    public static bool LookupTile(int x, int y, out MapTile tile) => (tile = LookupTile(x, y)) != null;
    
    public static MapTile LookupTile(int x, int y) {
        if (x < 0 || x > Width || y < 0 || y > Height) {
            return null;
        }

        return Tiles[(x, y)] ?? (Tiles[(x, y)] = new MapTile(x, y));
    }

    private static readonly MapTile[] RebuildData = new MapTile[9];

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
        var idx = 0;
        for (var y1 = tile.Y - 1; y1 <= tile.Y + 1; y1++){
            for (var x1 = tile.X - 1; x1 <= tile.X + 1; x1++) {
                RebuildData[idx++] = LookupTile(x1, y1);
            }
        }
        
        tile.Rebuild(RebuildData);
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