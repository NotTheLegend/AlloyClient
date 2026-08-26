using System;
using System.Collections.Concurrent;
using AlloyClient.Networking.Packets.Incoming;

namespace AlloyClient.Networking.Packets;

public interface IPacket {
    PacketId PacketId { get; }

    public void ReturnPacket() {
        throw new NotImplementedException();
    }
}

public interface IIncomingPacket : IPacket {
    void Read(ref SpanReader reader);
    void Handle();
}

public interface IOutgoingPacket : IPacket {
    void Write(ref SpanWriter writer);
}

public abstract class BasePacket<T> : IPacket where T : BasePacket<T>, new() {
    private readonly static ConcurrentQueue<T> PacketPool = new();

    public static T CreatePacket() {
        if (PacketPool.TryDequeue(out var packet)) {
            return packet;
        }

        var packetInstance = new T();
        packetInstance.Reset();
        return packetInstance;
    }

    public void ReturnPacket() {
        Reset();
        PacketPool.Enqueue((T)this);
    }

    public virtual PacketId PacketId => PacketId.Unknown;

    public abstract void Reset();
}

public abstract class IncomingPacket<T> : BasePacket<T>, IIncomingPacket where T : IncomingPacket<T>, new() {
    public abstract void Read(ref SpanReader reader);
    public abstract void Handle();
}

public abstract class OutgoingPacket<T> : BasePacket<T>, IOutgoingPacket where T : OutgoingPacket<T>, new() {
    public abstract void Write(ref SpanWriter writer);
}

// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
public static class PacketUtils {
    public static IIncomingPacket CreateIncomingPacket(PacketId packetId) {
        return packetId switch {
            PacketId.AccountList => AccountList.CreatePacket(),
            //PacketId.AllyShoot => AllyShoot.CreatePacket(),
            PacketId.Aoe => Aoe.CreatePacket(),
            PacketId.BuyResult => BuyResult.CreatePacket(),
            //PacketId.ClientStat => ClientStat.CreatePacket(),
            PacketId.CreateSuccess => CreateSuccess.CreatePacket(),
            PacketId.Damage => Damage.CreatePacket(),
            PacketId.Death => Death.CreatePacket(),
            PacketId.EnemyShoot => EnemyShoot.CreatePacket(),
            PacketId.Failure => Failure.CreatePacket(),
            //PacketId.File => File.CreatePacket(),
            //PacketId.GlobalNotification => GlobalNotification.CreatePacket(),
            PacketId.Goto => Goto.CreatePacket(),
            PacketId.GuildResult => GuildResult.CreatePacket(),
            PacketId.InvitedToGuild => InvitedToGuild.CreatePacket(),
            PacketId.InvResult => InvResult.CreatePacket(),
            PacketId.MapInfo => MapInfo.CreatePacket(),
            //PacketId.NameResult => NameResult.CreatePacket(),
            PacketId.NewTick => NewTick.CreatePacket(),
            PacketId.Notification => Notification.CreatePacket(),
            //PacketId.Pic => Pic.CreatePacket(),
            //PacketId.Ping => Ping.CreatePacket(),
            PacketId.PlaySound => PlaySound.CreatePacket(),
            //PacketId.QuestObjId => QuestObjId.CreatePacket(),
            PacketId.Reconnect => Reconnect.CreatePacket(),
            PacketId.ServerPlayerShoot => ServerPlayerShoot.CreatePacket(),
            PacketId.ShowEffect => ShowEffect.CreatePacket(),
            PacketId.Text => Text.CreatePacket(),
            PacketId.TradeAccepted => TradeAccepted.CreatePacket(),
            PacketId.TradeChanged => TradeChanged.CreatePacket(),
            PacketId.TradeDone => TradeDone.CreatePacket(),
            PacketId.TradeRequested => TradeRequested.CreatePacket(),
            PacketId.TradeStart => TradeStart.CreatePacket(),
            PacketId.Update => Update.CreatePacket(),
            PacketId.ServerProjectileProps => ServerProjectileProps.CreatePacket(),
            _ => throw new ArgumentException($"Unsupported packet ID: {packetId}")
        };
    }
}