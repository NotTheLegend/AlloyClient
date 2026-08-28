using System.Runtime.CompilerServices;

namespace Alloy.Engine.Diagnostics;

public static class FrameMetrics {

    public static long AllocatedBytes;
    public static long GpuUploadBytes;

    public static int DrawCalls;
    public static int UiNodesVisited;
    public static int PointerEvents;
    public static int PointerResolutions;
    public static int PointerNodesVisited;

    private static long _allocationStart;
    private static long _gpuUploadBytes;
    private static int _drawCalls;
    private static int _uiNodesVisited;
    private static int _pointerEvents;
    private static int _pointerResolutions;
    private static int _pointerNodesVisited;

    public static void BeginFrame() {
        _allocationStart = GC.GetAllocatedBytesForCurrentThread();
        _gpuUploadBytes = 0;
        _drawCalls = 0;
        _uiNodesVisited = 0;
        _pointerEvents = 0;
        _pointerResolutions = 0;
        _pointerNodesVisited = 0;
    }

    public static void EndFrame() {
        AllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - _allocationStart;
        GpuUploadBytes = _gpuUploadBytes;
        DrawCalls = _drawCalls;
        UiNodesVisited = _uiNodesVisited;
        PointerEvents = _pointerEvents;
        PointerResolutions = _pointerResolutions;
        PointerNodesVisited = _pointerNodesVisited;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordUpload(long bytes) {
        _gpuUploadBytes += bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordDrawCall() {
        _drawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordUiNode() {
        _uiNodesVisited++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordPointerEvent() {
        _pointerEvents++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordPointerResolution() {
        _pointerResolutions++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RecordPointerNode() {
        _pointerNodesVisited++;
    }
}
