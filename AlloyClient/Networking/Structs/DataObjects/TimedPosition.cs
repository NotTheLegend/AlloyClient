namespace AlloyClient.Networking.Structs.DataObjects;

public struct TimedPosition : IDataObject {
    
    public int Time;
    public Position Position;

    public void Reset() {
        Time = 0;
        Position.Reset();
    }

    public void Read(NetworkReader reader) {
        Time = reader.ReadInt32();
        Position.Read(reader);
    }

    public void Write(NetworkWriter writer) {
        writer.Write(Time);
        Position.Write(writer);
    }

    public override string ToString() {
        return $"Time: {Time}, Position: {Position}";
    }
}