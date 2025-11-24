namespace AlloyClient.Networking.Structs;

public interface IDataObject {
    public void Reset();
    public void Read(NetworkReader reader);
    public void Write(NetworkWriter writer);
}