using System;
using System.Collections.Concurrent;
using MonoClient.Networking.Packets.Incoming;

namespace MonoClient.Networking.Packets;

public interface IPacket {
    PacketId PacketId { get; }

    public void ReturnPacket() {
        throw new NotImplementedException();
    }
}

public interface IIncomingPacket : IPacket {
    void Read(NetworkReader reader);
    void Handle();
}

public interface IOutgoingPacket : IPacket {
    void Write(NetworkWriter writer);
}

public abstract class BasePacket<T> : IPacket where T : BasePacket<T>, new() {
    private static readonly ConcurrentQueue<T> PacketPool = new();

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
    public abstract void Read(NetworkReader reader);
    public abstract void Handle();
}

public abstract class OutgoingPacket<T> : BasePacket<T>, IOutgoingPacket where T : OutgoingPacket<T>, new() {
    public abstract void Write(NetworkWriter writer);
}

// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
public static class PacketUtils {
    public static IIncomingPacket CreateIncomingPacket(PacketId packetId) {
        return packetId switch {
            PacketId.AccountList => AccountList.CreatePacket(),
            PacketId.AllyShoot => AllyShoot.CreatePacket(),
            PacketId.Aoe => Aoe.CreatePacket(),
            PacketId.ArenaState => ArenaState.CreatePacket(),
            PacketId.AuctionBidUpdate => AuctionBidUpdate.CreatePacket(),
            PacketId.AuctionHistoryUpdate => AuctionHistoryUpdate.CreatePacket(),
            PacketId.AuctionItemUpdate => AuctionItemUpdate.CreatePacket(),
            PacketId.AuctionRoundTimeUpdate => AuctionRoundTimeUpdate.CreatePacket(),
            PacketId.BuyResult => BuyResult.CreatePacket(),
            PacketId.ChangeDarkness => ChangeDarkness.CreatePacket(),
            PacketId.ClientStat => ClientStat.CreatePacket(),
            PacketId.ConditionTime => ConditionTime.CreatePacket(),
            PacketId.CreateSuccess => CreateSuccess.CreatePacket(),
            PacketId.Damage => Damage.CreatePacket(),
            PacketId.DashIndicator => DashIndicator.CreatePacket(),
            PacketId.Death => Death.CreatePacket(),
            PacketId.DespawnProjectiles => DespawnProjectiles.CreatePacket(),
            PacketId.EnemyShoot => EnemyShoot.CreatePacket(),
            PacketId.FadePlayer => FadePlayer.CreatePacket(),
            PacketId.Failure => Failure.CreatePacket(),
            PacketId.File => File.CreatePacket(),
            PacketId.ForgeResult => ForgeResult.CreatePacket(),
            PacketId.GlobalNotification => GlobalNotification.CreatePacket(),
            PacketId.Goto => Goto.CreatePacket(),
            PacketId.GuildResult => GuildResult.CreatePacket(),
            PacketId.IECooldown => IECooldown.CreatePacket(),
            PacketId.InvitedToGuild => InvitedToGuild.CreatePacket(),
            PacketId.InvResult => InvResult.CreatePacket(),
            PacketId.MapInfo => MapInfo.CreatePacket(),
            PacketId.MarketMyItems => MarketMyItems.CreatePacket(),
            PacketId.MarketShop => MarketShop.CreatePacket(),
            PacketId.NameResult => NameResult.CreatePacket(),
            PacketId.NewTick => NewTick.CreatePacket(),
            PacketId.Notification => Notification.CreatePacket(),
            PacketId.Pic => Pic.CreatePacket(),
            PacketId.Ping => Ping.CreatePacket(),
            PacketId.PlaySound => PlaySound.CreatePacket(),
            PacketId.PotStorageGetResult => PotStorageGetResult.CreatePacket(),
            PacketId.QuestObjId => QuestObjId.CreatePacket(),
            PacketId.QueueTick => QueueTick.CreatePacket(),
            PacketId.Reconnect => Reconnect.CreatePacket(),
            PacketId.ReforgeItemResult => ReforgeItemResult.CreatePacket(),
            PacketId.ServerPetShoot => ServerPetShoot.CreatePacket(),
            PacketId.ServerPlayerShoot => ServerPlayerShoot.CreatePacket(),
            PacketId.ShowEffect => ShowEffect.CreatePacket(),
            PacketId.SwitchMusic => SwitchMusic.CreatePacket(),
            PacketId.Text => Text.CreatePacket(),
            PacketId.TradeAccepted => TradeAccepted.CreatePacket(),
            PacketId.TradeChanged => TradeChanged.CreatePacket(),
            PacketId.TradeDone => TradeDone.CreatePacket(),
            PacketId.TradeRequested => TradeRequested.CreatePacket(),
            PacketId.TradeStart => TradeStart.CreatePacket(),
            PacketId.Update => Update.CreatePacket(),
            PacketId.UpgradeItemResult => UpgradeItemResult.CreatePacket(),
            PacketId.WeaponMasterShoot => WeaponMasterShoot.CreatePacket(),
            PacketId.DungeonModifiersUpdate => DungeonModifiersUpdate.CreatePacket(),
            _ => throw new ArgumentException($"Unsupported packet ID: {packetId}")
        };
    }
}