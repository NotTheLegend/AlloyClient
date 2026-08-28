using System;
using System.Collections.Generic;
using System.Numerics;
using Alloy.Engine;
using Alloy.Engine.Diagnostics;
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
    private long _sampleAllocatedBytes;
    private long _sampleUploadBytes;
    private long _sampleDrawCalls;
    private long _sampleUiNodes;
    private long _samplePointerEvents;
    private long _samplePointerResolutions;
    private long _samplePointerNodes;

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
    private readonly SimpleText _frameWork = new(new TextConfig {Text = "Frame avg: 0 draws | 0 KB upload | 0 KB alloc", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    private readonly SimpleText _visibility = new(new TextConfig {Text = "Visible/culled: E 0/0 | P 0/0 | FX 0/0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    private readonly SimpleText _gpuWorld = new(new TextConfig {Text = "GPU: G 0 | S 0 | P 0 | M 0 | O 0 ms", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    private readonly SimpleText _gpuUi = new(new TextConfig {Text = "GPU UI: 0 ms", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    private readonly SimpleText _uiTraversal = new(new TextConfig {Text = "UI nodes: 0 | pointer 0/0/0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});
    
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
        AddChild(_frameWork);
        AddChild(_visibility);
        AddChild(_gpuWorld);
        AddChild(_gpuUi);
        AddChild(_uiTraversal);
        
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
        _frameWork.Y = _ui.Y + _ui.Height + 4;
        _visibility.Y = _frameWork.Y + _frameWork.Height + 4;
        _gpuWorld.Y = _visibility.Y + _visibility.Height + 4;
        _gpuUi.Y = _gpuWorld.Y + _gpuWorld.Height + 4;
        _uiTraversal.Y = _gpuUi.Y + _gpuUi.Height + 4;
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
        _sampleAllocatedBytes += FrameMetrics.AllocatedBytes;
        _sampleUploadBytes += FrameMetrics.GpuUploadBytes;
        _sampleDrawCalls += FrameMetrics.DrawCalls;
        _sampleUiNodes += FrameMetrics.UiNodesVisited;
        _samplePointerEvents += FrameMetrics.PointerEvents;
        _samplePointerResolutions += FrameMetrics.PointerResolutions;
        _samplePointerNodes += FrameMetrics.PointerNodesVisited;

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
        
        var sampleFrames = Math.Max(1, _frameCount);
        var fps = _frameCount * 1000d / _statisticsTimer;
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
        _frameWork.SetText($"Frame avg: {Math.Round(_sampleDrawCalls / (double)sampleFrames, 2)} draws | {Math.Round(_sampleUploadBytes / (double)sampleFrames / 1000.0, 2)} KB upload | {Math.Round(_sampleAllocatedBytes / (double)sampleFrames / 1000.0, 2)} KB alloc");
        _visibility.SetText($"Visible/culled: E {Render.LastVisibleEntities}/{Render.LastCulledEntities} | P {Render.LastVisibleProjectiles}/{Render.LastCulledProjectiles} | FX {Render.LastVisibleParticles}/{Render.LastCulledParticles}");
        _gpuWorld.SetText($"GPU: G {Math.Round(Render.GpuGround.LastMilliseconds, 3)} | S {Math.Round(Render.GpuShadows.LastMilliseconds, 3)} | P {Math.Round(Render.GpuParticles.LastMilliseconds, 3)} | M {Math.Round(Render.GpuModels.LastMilliseconds, 3)} | O {Math.Round(Render.GpuObjects.LastMilliseconds, 3)} ms");
        _gpuUi.SetText($"GPU UI: {Math.Round(UiRender.GpuDraw.LastMilliseconds, 3)} ms");
        _uiTraversal.SetText($"UI avg: {Math.Round(_sampleUiNodes / (double)sampleFrames, 2)} nodes | pointer {Math.Round(_samplePointerEvents / (double)sampleFrames, 2)}/{Math.Round(_samplePointerResolutions / (double)sampleFrames, 2)}/{Math.Round(_samplePointerNodes / (double)sampleFrames, 2)}");

        _sampleAllocatedBytes = 0;
        _sampleUploadBytes = 0;
        _sampleDrawCalls = 0;
        _sampleUiNodes = 0;
        _samplePointerEvents = 0;
        _samplePointerResolutions = 0;
        _samplePointerNodes = 0;
    }
}
