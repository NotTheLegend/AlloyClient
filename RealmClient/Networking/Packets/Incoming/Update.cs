using RealmClient.Assets.Libraries;
using RealmClient.Game;
using RealmClient.Game.Objects;
using RealmClient.Networking.Packets.Outgoing;
using RealmClient.Networking.Structs.DataObjects;
using RealmClient.Utils;

namespace RealmClient.Networking.Packets.Incoming;

public class Update : IncomingPacket<Update> {
    public TileData[] Tiles;
    public ObjectDef[] NewObjs;
    public int[] Drops;

    public override PacketId PacketId => PacketId.Update;

    public override void Reset() {
        Tiles = null;
        NewObjs = null;
        Drops = null;
    }

    public override void Read(NetworkReader reader) {
        Tiles = new TileData[reader.ReadInt16()];

        for (var i = 0; i < Tiles.Length; i++) {
            Tiles[i] = new TileData();
            Tiles[i].Read(reader);
        }

        NewObjs = new ObjectDef[reader.ReadInt16()];

        for (var i = 0; i < NewObjs.Length; i++) {
            NewObjs[i] = new ObjectDef();
            NewObjs[i].Read(reader);
        }

        Drops = new int[reader.ReadInt16()];

        for (var i = 0; i < Drops.Length; i++) {
            Drops[i] = reader.ReadInt32();
        }
    }

    public override void Handle() {
        Client.QueuePacket(UpdateAck.CreatePacket());

        foreach (var tile in Tiles) {
            Map.SetTileData(tile.X, tile.Y, tile.Type);
        }

        foreach (var newObj in NewObjs) {
            Entity entity;

            if (!ObjectLibrary.TypeToObjectProps.TryGetValue(newObj.ObjectType, out var props)) {
                Logger.Warn($"[Update] Unable to parse props for id: {newObj.ObjectType:x4}");
                props = ObjectLibrary.TypeToObjectProps[0x600]; //Pirate Placeholder
            }

            if (props.IsPlayer) {
                entity = new Player();
            }
            // else if (props.IsEnemy) {
            //    entity = new Enemy();
            // }
            // else if (props.IsAlly) {
            //     entity = new Ally();
            // }
            else {
                entity = new Entity();
            }

            entity.Properties = props;

            entity.SetObjectId(newObj.Id);
            entity.SetType(newObj.ObjectType);
            Map.AddEntity(entity, newObj.Position);

            entity.SetPos(newObj.Position.X, newObj.Position.Y);

            entity.UpdateStats(newObj.Stats);
            entity.OnTickPosition(newObj.Position.X, newObj.Position.Y, 0, 0, props.IsPlayer);

            if (newObj.Id == Map.LocalPlayerId) {
                Map.OnLocalPlayerCreated(entity);
            }
        }

        foreach (var dropId in Drops) {
            Map.RemoveEntity(dropId);
        }
    }

    public override string ToString() {
        return $"Tiles: {Tiles}, NewObjs: {NewObjs}, Drops: {Drops}";
    }
}