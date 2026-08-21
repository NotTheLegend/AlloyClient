using System;
using System.Collections.Generic;
using System.Numerics;
using Alloy.Engine;
using AlloyClient.Rendering;
using Alloy.UiLib;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Ui.Components.Elements;

public class DebugStats : Sprite {
    
    private const int Outline = 3;

    private const int WindowTimeSeconds = 5; // was 30 but the sorting was eating up +100ms at 2000 fps, probably cuz i have black desert running in the background lmao, 30 seconds was overkill anyways
    private const int WindowTimeMs = 1000 * WindowTimeSeconds;
    private const int StartingFrameCount = WindowTimeSeconds * 3000;

    private double _framesSeconds;

    private readonly Queue<double> _frameTimes = new (StartingFrameCount);
    private double[] _workingFrameTimes = new double[StartingFrameCount];
    private double _statisticsTimer;
    private int _frameCount;

    private readonly SimpleText _frameTimeTimer = new (new TextConfig {Text = $"Frame time over the last {WindowTimeSeconds} seconds", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _avgFrameTime = new (new TextConfig {Text = "Avg: 0 ms", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _p90FrameTime = new (new TextConfig {Text = "P90: 0 ms", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _p99FrameTime = new (new TextConfig {Text = "P99: 0 ms", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _maxFrameTime = new (new TextConfig {Text = "Max: 0 ms", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _fps = new (new TextConfig {Text = "FPS: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _memory = new (new TextConfig {Text = "Memory", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _gcAlloc = new (new TextConfig {Text = "Allocated: 0 MB", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _gcAllocDelta = new (new TextConfig {Text = "Allocated delta: 0 B", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _gcGen0Count = new (new TextConfig {Text = "Gen0 count: 0", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _gcGen1Count = new (new TextConfig {Text = "Gen1 count: 0", X = 8, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _tiles = new (new TextConfig {Text = "Tiles: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _shadows = new (new TextConfig {Text = "Shadows: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _entities = new (new TextConfig {Text = "Entities: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _particles = new (new TextConfig {Text = "Particles: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _ui = new(new TextConfig {Text = "Ui: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    
    private long _lastGcBytes;
    
    public DebugStats() {
        AddChild(_fps);
        AddChild(_frameTimeTimer);
        AddChild(_avgFrameTime);
        AddChild(_p90FrameTime);
        AddChild(_p99FrameTime);
        AddChild(_maxFrameTime);
        AddChild(_memory);
        AddChild(_gcAlloc);
        AddChild(_gcAllocDelta);
        AddChild(_gcGen0Count);
        AddChild(_gcGen1Count);
        AddChild(_tiles);
        AddChild(_shadows);
        AddChild(_entities);
        AddChild(_particles);
        AddChild(_ui);
        
        _fps.Y = 4;
        _frameTimeTimer.Y = _fps.Y + _fps.Height + 4;
        _avgFrameTime.Y = _frameTimeTimer.Y + _frameTimeTimer.Height + 4;
        _p90FrameTime.Y = _avgFrameTime.Y + _avgFrameTime.Height + 4;
        _p99FrameTime.Y = _p90FrameTime.Y + _p90FrameTime.Height + 4;
        _maxFrameTime.Y = _p99FrameTime.Y + _p99FrameTime.Height + 4;
        _memory.Y = _maxFrameTime.Y + _maxFrameTime.Height + 4;
        _gcAlloc.Y = _memory.Y + _memory.Height + 4;
        _gcAllocDelta.Y = _gcAlloc.Y + _gcAlloc.Height + 4;
        _gcGen0Count.Y = _gcAllocDelta.Y + _gcAllocDelta.Height + 4;
        _gcGen1Count.Y = _gcGen0Count.Y + _gcGen0Count.Height + 4;
        _tiles.Y = _gcGen1Count.Y + _gcGen1Count.Height + 4;
        _shadows.Y = _tiles.Y + _tiles.Height + 4;
        _entities.Y = _shadows.Y + _shadows.Height + 4;
        _particles.Y = _entities.Y + _entities.Height + 4;
        _ui.Y = _particles.Y + _particles.Height + 4;
    }

    public void Update(GameTime gameTime) {
        var elapsed = gameTime.ElapsedMs;
        _frameTimes.Enqueue(elapsed);
        _framesSeconds += elapsed;
        _statisticsTimer += elapsed;

        // Drop frames that fall outside the 30s window
        while (_framesSeconds > WindowTimeMs) {
            _framesSeconds -= _frameTimes.Dequeue();
        }

        _frameCount++;

        if (_statisticsTimer < 1000) {
            return;
        }

        if (_workingFrameTimes.Length < _frameTimes.Count) {
            _workingFrameTimes = new double[BitOperations.RoundUpToPowerOf2((uint)_frameTimes.Count)];
        }
        
        _frameTimes.CopyTo(_workingFrameTimes, 0);

        var data = _workingFrameTimes.AsSpan(0, _frameTimes.Count);
        data.Sort();

        var sum = 0d;
        for (var i = 0; i < data.Length; i++) {
            sum += data[i];
        }
        
        var fps = _frameCount / 1.0;
        var count = data.Length;
        var avgFrameTime = sum / count;
        var p90FrameTime = data[(int) (count * 0.90f)];
        var p99FrameTime = data[(int) (count * 0.99f)];
        var maxFrameTime = data[count - 1];
        
        _statisticsTimer = 0;
        _frameCount = 0;

        _avgFrameTime.SetText($"Avg: {Math.Round(avgFrameTime, 3)} ms"); // Over 30 seconds
        _p90FrameTime.SetText($"P90: {Math.Round(p90FrameTime, 3)} ms");
        _p99FrameTime.SetText($"P99: {Math.Round(p99FrameTime, 3)} ms");
        _maxFrameTime.SetText($"Max: {Math.Round(maxFrameTime, 3)} ms");
        _fps.SetText($"FPS: {Math.Round(fps, 1)}");
        _gcAlloc.SetText($"Total: {Math.Round(GC.GetTotalMemory(false) / 1000000f, 2)} MB");
        var gcBytes = GC.GetTotalAllocatedBytes();
        _gcAllocDelta.SetText($"Allocated delta: {Math.Round((gcBytes - _lastGcBytes) / 1000.0, 2)} KB");
        _lastGcBytes = gcBytes;
        _gcGen0Count.SetText($"Gen0 count: {GC.CollectionCount(0)}");
        _gcGen1Count.SetText($"Gen1 count: {GC.CollectionCount(1)}");
        _tiles.SetText($"Tiles: {Render.LastDrawCountTiles}");
        _shadows.SetText($"Shadows: {Render.LastDrawCountShadows}");
        _entities.SetText($"Entities: {Render.LastDrawCountEntities}");
        _particles.SetText($"Particles: {Render.LastDrawParticleCount}");
        _ui.SetText($"Ui: {UiRender.LastRenderCount}");
    }
}