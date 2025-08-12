namespace RealmClient.Networking.Structs.DataObjects;

public struct Position : IDataObject {
    public static readonly Position Zero = new();

    public float X;
    public float Y;

    public Position(float f, float f1) {
        X = f;
        Y = f1;
    }

    public void Read(NetworkReader reader) {
        X = reader.ReadSingle();
        Y = reader.ReadSingle();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(X);
        writer.Write(Y);
    }

    public void Reset() {
        X = 0;
        Y = 0;
    }

    public override string ToString() {
        return $"X: {X}, Y: {Y}";
    }
}