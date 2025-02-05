using MonoClient.Screens.Game.Components.Hud.Inventory;

namespace MonoClient.Networking.Packets.Incoming;

public class InvResult : IncomingPacket<InvResult> {
    public int Result;

    public override PacketId PacketId => PacketId.InvResult;

    public override void Reset() {
        Result = 0;
    }

    public override void Read(NetworkReader reader) {
        Result = reader.ReadInt32();
    }

    public override void Handle() {
        if (Result < 1) 
            return;
        
        //OldInventoryGrid.Initialized = false;
        //ContainerGrid.Initialized = false;
    }

    public override string ToString() {
        return $"Result: {Result}";
    }
}