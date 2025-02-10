using System;
using System.Collections.Generic;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Objects;
using MonoClient.Utils;

namespace MonoClient;

public static class PartyData {

    public const int MaxVisibleMembers = 6;

    private const int MaxDistance = 50 * 50;

    public static readonly HashSet<int> Locked = [];
    
    public static readonly HashSet<int> Ignored = [];

    private static double _lastUpdateTime;

    private static readonly PartyInfo[] _members = new PartyInfo[250];

    public static readonly ArraySegment<PartyInfo> PartyMembers = new(_members, 0, MaxVisibleMembers);

    public static void Clear() {
        Locked.Clear();
        Ignored.Clear();
        Array.Clear(_members);
    }

    public static void Update(double time) {
        if (time < _lastUpdateTime + 500)
            return;
        
        if (Map.LocalPlayer == null)
            return;
        
        _lastUpdateTime = time;
        
        Array.Clear(_members);

        var localPosition = Map.LocalPlayer.Position;
        var i = 0;
        
        foreach (var player in Map.Players.Values) {
            var dist = MathUtils.GetDistanceSquared(localPosition, player.Position);
            if (dist < MaxDistance) {
                _members[i] = new PartyInfo(player, player.Locked, dist, player.ObjectId);
                i++;
            }
        }
        
        Array.Sort(_members, (self, other) => (self.Starred && !other.Starred) || (self.Dist < other.Dist) || (self.ObjectId < other.ObjectId) ? -1 : 1);
    }

    public static void SetData(int id, int[] list) {
        var set = id == 0 ? Locked : Ignored;
        set.UnionWith(list);
    }

    public static void LockPlayer(Player player) {
        player.Locked = true;
        _lastUpdateTime = int.MinValue;
        Locked.Add(player.AccountId);

        var pkt = EditAccountList.CreatePacket();
        pkt.AccountListId = 0;
        pkt.Add = true;
        pkt.ObjectId = player.ObjectId;
        
        Client.QueuePacket(pkt);
    }
    
    public static void UnlockPlayer(Player player) {
        player.Locked = false;
        _lastUpdateTime = int.MinValue;
        Locked.Remove(player.AccountId);

        var pkt = EditAccountList.CreatePacket();
        pkt.AccountListId = 0;
        pkt.Add = false;
        pkt.ObjectId = player.ObjectId;
        
        Client.QueuePacket(pkt);
    }
    
    public static void IgnorePlayer(Player player) {
        player.Ignored = true;
        _lastUpdateTime = int.MinValue;
        Ignored.Add(player.AccountId);

        var pkt = EditAccountList.CreatePacket();
        pkt.AccountListId = 1;
        pkt.Add = true;
        pkt.ObjectId = player.ObjectId;
        
        Client.QueuePacket(pkt);
    }
    
    public static void UnignorePlayer(Player player) {
        player.Ignored = false;
        _lastUpdateTime = int.MinValue;
        Ignored.Remove(player.AccountId);

        var pkt = EditAccountList.CreatePacket();
        pkt.AccountListId = 1;
        pkt.Add = false;
        pkt.ObjectId = player.ObjectId;
        
        Client.QueuePacket(pkt);
    }

    public sealed record PartyInfo(Player Player, bool Starred = false, float Dist = float.MaxValue, int ObjectId = int.MaxValue);
}