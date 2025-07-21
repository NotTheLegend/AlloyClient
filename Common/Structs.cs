namespace Common;

public readonly struct GameTime(double total, double elapsed) {

    public readonly double TotalMs = total;

    public readonly double ElapsedMs = elapsed;
}