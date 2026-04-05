namespace AlloyClient.Networking.Structs.DataObjects;

public struct TimedPosition : IDataObject {
    
    public int Time;
    public Position Position;

    public void Reset() {
        Time = 0;
        Position.Reset();
    }

    public void Read(ref SpanReader reader) {
        Time = reader.ReadInt32();
        Position.Read(ref reader);
    }

    public void Write(ref SpanWriter writer) {
        writer.Write(Time);
        Position.Write(ref writer);
    }

    public override string ToString() {
        return $"Time: {Time}, Position: {Position}";
    }
}