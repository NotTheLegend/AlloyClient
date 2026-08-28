using System;
using System.Collections.Generic;
using AlloyClient.Game;
using AlloyClient.Game.Components.Hud.Chat;
using AlloyClient.Rendering;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Diagnostics;

public static class RenderStress {

    private const int MaxTiles = 30000;
    private const int MaxObjects = 30000;
    private const int MaxParticles = 30000;

    public static int TileCount;
    public static int ObjectCount;
    public static int ParticleCount;

    internal static void HandleCommand(ClientCommandContext command) {
        var arguments = command.Arguments;
        if (arguments.Length == 0) {
            command.Reply($"Render stress: tiles {TileCount}, objects {ObjectCount}, particles {ParticleCount}.");
            return;
        }

        if (arguments.Length == 1 && arguments[0].Equals("off", StringComparison.OrdinalIgnoreCase)) {
            TileCount = 0;
            ObjectCount = 0;
            ParticleCount = 0;
            command.Reply("Render stress disabled.");
            return;
        }

        if (arguments.Length != 2 || !int.TryParse(arguments[1], out var count) || count < 0) {
            ReplyWithUsage(command);
            return;
        }

        if (arguments[0].Equals("tiles", StringComparison.OrdinalIgnoreCase)) {
            TileCount = Math.Min(count, MaxTiles);
        } else if (arguments[0].Equals("objects", StringComparison.OrdinalIgnoreCase)) {
            ObjectCount = Math.Min(count, MaxObjects);
        } else if (arguments[0].Equals("particles", StringComparison.OrdinalIgnoreCase)) {
            ParticleCount = Math.Min(count, MaxParticles);
        } else if (arguments[0].Equals("mixed", StringComparison.OrdinalIgnoreCase)) {
            TileCount = Math.Min(count, MaxTiles);
            ObjectCount = Math.Min(count, MaxObjects);
            ParticleCount = Math.Min(count, MaxParticles);
        } else {
            ReplyWithUsage(command);
            return;
        }

        command.Reply($"Render stress: tiles {TileCount}, objects {ObjectCount}, particles {ParticleCount}.");
    }

    private static void ReplyWithUsage(ClientCommandContext command) {
        command.Reply("Usage: /renderstress off | tiles/objects/particles/mixed <count>.");
    }

    public static void AddTiles(List<TileData> tiles, in Camera camera) {
        if (TileCount == 0 || tiles.Count == 0) {
            return;
        }

        var count = Math.Min(TileCount, Render.TileBufferSize - tiles.Count);
        if (count <= 0) {
            return;
        }

        var source = tiles[0];
        var width = Math.Max(1, (int)MathF.Ceiling(MathF.Sqrt(count)));
        var originX = MathF.Floor(camera.Position.X) - width * 0.5f;
        var originY = MathF.Floor(camera.Position.Y) - width * 0.5f;

        for (var i = 0; i < count; i++) {
            var tile = source;
            tile.Position.X = originX + i % width;
            tile.Position.Y = originY + i / width;
            tiles.Add(tile);
        }
    }

    public static int AddObjects(List<VertexObject> objects, in Camera camera) {
        if (ObjectCount == 0 || objects.Count == 0) {
            return 0;
        }

        var source = objects[0];
        var width = Math.Max(1, (int)MathF.Ceiling(MathF.Sqrt(ObjectCount)));
        var spacing = 1.25f;
        var originX = camera.Position.X - width * spacing * 0.5f;
        var originY = camera.Position.Y - width * spacing * 0.5f;
        var depthMatrix = camera.DepthMatrix;

        for (var i = 0; i < ObjectCount; i++) {
            var item = source;
            item.Position.X = originX + i % width * spacing;
            item.Position.Y = originY + i / width * spacing;
            item.Extra.Y = 0.5f + 0.4f * (item.Position.X * depthMatrix.M12 + item.Position.Y * depthMatrix.M22 + depthMatrix.M42);
            objects.Add(item);
        }

        return ObjectCount;
    }

    public static int AddParticles(ParticleData[] particles, int startIndex, in Camera camera) {
        if (ParticleCount == 0 || startIndex >= particles.Length) {
            return startIndex;
        }

        var count = Math.Min(ParticleCount, particles.Length - startIndex);
        var width = Math.Max(1, (int)MathF.Ceiling(MathF.Sqrt(count)));
        var spacing = 0.35f;
        var originX = camera.Position.X - width * spacing * 0.5f;
        var originY = camera.Position.Y - width * spacing * 0.5f;
        var color = new Vector4(0.35f, 0.65f, 1f, 0.8f);

        for (var i = 0; i < count; i++) {
            var position = new Vector3(originX + i % width * spacing, originY + i / width * spacing, 0.1f);
            particles[startIndex + i] = new ParticleData(position, color);
        }

        return startIndex + count;
    }
}