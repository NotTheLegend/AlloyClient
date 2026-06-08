using System.Collections.Concurrent;
using AlloyClient.Assets.Libraries;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.Utils;
using Alloy.Common;
using Microsoft.Extensions.Logging;

namespace AlloyClient.Networking.Packets.Incoming;

public class Update : IncomingPacket<Update> {
    
    public override PacketId PacketId => PacketId.Update;
    
    private static TileData[]  _tilesBuffer   = new TileData[256];
    private static ObjectDef[] _newObjsBuffer = new ObjectDef[256];
    private static int[]       _dropsBuffer   = new int[256];

    public int TileCount;
    public int NewObjCount;
    public int DropCount;

    public TileData[]  Tiles   => _tilesBuffer;
    public ObjectDef[] NewObjs => _newObjsBuffer;
    public int[]       Drops   => _dropsBuffer;

    public override void Reset() {
        TileCount = NewObjCount = DropCount = 0;
    }

    public override void Read(ref SpanReader reader) {
        ObjectDef.StatsPoolIndex = 0;
        
        TileCount = reader.ReadInt16();
        EnsureCapacity(ref _tilesBuffer, TileCount);
        for (int i = 0; i < TileCount; i++)
            _tilesBuffer[i].Read(ref reader);

        NewObjCount = reader.ReadInt16();
        EnsureCapacity(ref _newObjsBuffer, NewObjCount);
        for (int i = 0; i < NewObjCount; i++)
            _newObjsBuffer[i].Read(ref reader);

        DropCount = reader.ReadInt16();
        EnsureCapacity(ref _dropsBuffer, DropCount);
        for (int i = 0; i < DropCount; i++) {
            _dropsBuffer[i] = reader.ReadInt32();
            _ = reader.ReadBoolean();
        }
    }
    
    private static void EnsureCapacity<T>(ref T[] array, int needed) {
        if (array.Length < needed)
            array = new T[needed * 2]; // double to avoid frequent resizes
    }

    public override void Handle() {
        Client.QueuePacket(UpdateAck.CreatePacket());

        for (int i = 0; i < TileCount; i++)
            Map.SetTileData(Tiles[i].X, Tiles[i].Y, Tiles[i].Type);

        for (int i = 0; i < NewObjCount; i++) {
            var newObj = NewObjs[i];
            Entity entity;

            if (!ObjectLibrary.TypeToObjectProps.TryGetValue(newObj.ObjectType, out var props)) {
                Client.Logger.Log(LogLevel.Warning, $"[Update] Unable to parse props for id: {newObj.ObjectType:x4}");
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

            entity.UpdateStats(ObjectDef.StatsPool, newObj.StatOffset, newObj.StatCount);
            entity.OnTickPosition(newObj.Position.X, newObj.Position.Y, 0, 0, props.IsPlayer);

            if (newObj.Id == Map.LocalPlayerId) {
                Map.OnLocalPlayerCreated(entity);
            }
        }

        for (int i = 0; i < DropCount; i++)
            Map.RemoveEntity(Drops[i]);
    }

    public override string ToString() {
        return $"Tiles: {Tiles}, NewObjs: {NewObjs}, Drops: {Drops}";
    }
}