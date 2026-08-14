using SSMP.Api.Server.Networking;
using System.Collections.Generic;

namespace BasicItemSync.Modules.Network.Server;

internal class NetworkForwarder
{
    static IServerAddonNetworkReceiver<Packets> Receiver;
    static IServerAddonNetworkSender<Packets> Sender;

    public static void Initialize()
    {
        Sender = ServerAddon.api.NetServer.GetNetworkSender<Packets>(ServerAddon.Instance);
        Receiver = ServerAddon.api.NetServer.GetNetworkReceiver<Packets>(ServerAddon.Instance, PacketInstantiate.Instantiate);

        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.BoolPlayerData, OnBool);
        Receiver.RegisterPacketHandler<SendIntItemPacket>(Packets.IntPlayerData, OnInt);
        Receiver.RegisterPacketHandler<SendFloatItemPacket>(Packets.FloatPlayerData, OnFloat);

        Receiver.RegisterPacketHandler<SendCurrencyPacket>(Packets.Currency, OnCurrency);
        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.Quest, OnQuest);
        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.Tool, OnTool);
        Receiver.RegisterPacketHandler<SendFlagPacket>(Packets.Upgrade, OnUpgrade);
        Receiver.RegisterPacketHandler<SendIntItemPacket>(Packets.Collectable, OnCollectable);
        Receiver.RegisterPacketHandler<SendPersistentBoolsPacket>(Packets.PersistentBool, OnPersistentBool);
        Receiver.RegisterPacketHandler<SendPersistentIntsPacket>(Packets.PersistentInt, OnPersistentInt);
    }

    static void Broadcast(ushort senderId, Packets type, ClientPacket packet)
    {
        if (Sender == null) return;

        foreach (var player in ServerAddon.api.ServerManager.Players)
        {
            if (player.Id == senderId) continue;
            Sender.SendCollectionData(type, packet, player.Id);
        }
    }

    public static void SendSettingsUpdate()
    {
        var packet = new SettingsUpdatePacket { Settings = ServerAddon.Settings };

        ServerAddon.api.ServerManager.BroadcastMessage("Sync settings have been updated");
        Sender.BroadcastSingleData(Packets.Settings, packet);
    }

    public static void SendSettingsUpdate(ushort id)
    {
        var packet = new SettingsUpdatePacket { Settings = ServerAddon.Settings };

        Sender.SendSingleData(Packets.Settings, packet, id);
    }

    static void OnCurrency(ushort id, SendCurrencyPacket packet)
    {
        if (!ServerAddon.Settings.FlagAllowed(FlagType.Currency)) return;
        Broadcast(id, Packets.Currency, packet);
    }

    static void OnBool(ushort id, SendBoolItemPacket packet) => OnFlag(id, packet, Packets.BoolPlayerData, true);
    static void OnInt(ushort id, SendIntItemPacket packet) => OnFlag(id, packet, Packets.IntPlayerData, true);
    static void OnFloat(ushort id, SendFloatItemPacket packet) => OnFlag(id, packet, Packets.FloatPlayerData, true);

    static void OnQuest(ushort id, SendBoolItemPacket packet) => OnFlag(id, packet, Packets.Quest, false);
    static void OnTool(ushort id, SendBoolItemPacket packet) => OnFlag(id, packet, Packets.Tool, false);
    static void OnUpgrade(ushort id, SendFlagPacket packet) => OnFlag(id, packet, Packets.Upgrade, false);
    static void OnCollectable(ushort id, SendIntItemPacket packet) => OnFlag(id, packet, Packets.Collectable, false);
    static void OnPersistentBool(ushort id, SendPersistentBoolsPacket packet)
    {
        var newPacket = new SendPersistentBoolsPacket();

        foreach (var item in packet.Values)
        {
            if (!ServerAddon.Settings.FlagAllowed(item.Value.Item2)) continue;
            newPacket.Values.Add(item.Key, item.Value);
        }

        if (newPacket.Values.Count == 0) return;
        Broadcast(id, Packets.PersistentBool, newPacket);
    }
    static void OnPersistentInt(ushort id, SendPersistentIntsPacket packet)
    {
        var newPacket = new SendPersistentIntsPacket();

        foreach (var item in packet.Values)
        {
            if (!ServerAddon.Settings.FlagAllowed(item.Value.Item2)) continue;
            newPacket.Values.Add(item.Key, item.Value);
        }

        if (newPacket.Values.Count == 0) return;
        Broadcast(id, Packets.PersistentInt, newPacket);
    }
    static void Announce(ushort id, SendFlagPacket packet, bool isPossiblySilent)
    {
        var type = packet.FlagType;
        Log.LogDebug($"[SERVER] Received {type}");
        if (isPossiblySilent && string.IsNullOrEmpty(packet.Name)) return;

        string template = type switch
        {
            FlagType.Ability => "obtained $",
            FlagType.Map => "obtained $ Map",
            FlagType.Pin => "obtained $",
            FlagType.Bellshrine => "activated the $ Bellshrine",
            FlagType.Boss => "defeated $",
            FlagType.Arena => "defeated $",
            FlagType.Progression => "obtained the $",
            FlagType.Collectable => "collected a $",
            FlagType.Bellway => "unlocked the $ Bellway Station",
            FlagType.Ventrica => "unlocked the $ Ventrica",
            FlagType.Mask => "obtained the Mask Shard in $",
            FlagType.Spool => "obtained the Spool Fragment in $",
            FlagType.Pouch => "obtained a Tool Pouch from $",
            FlagType.CraftingKit => "obtained a Crafting Kit from $",
            FlagType.SilkHeart => "obtained a Silk Heart",
            FlagType.Needle => "upgraded their needle",
            FlagType.Quest => "finished the $ quest",
            FlagType.QuestItem => "",
            FlagType.Tool => "obtained $",
            FlagType.Crest => "obtained $ Crest",
            FlagType.Currency => "",
            FlagType.Bench => "unlocked a bench",
            FlagType.Flea => "saved a flea from $",
            _ => "",
        };

        if (string.IsNullOrEmpty(template)) return;

        var player = ServerAddon.api.ServerManager.GetPlayer(id);

        var name = string.IsNullOrEmpty(packet.Name) ? packet.Key : packet.Name;
        var message = $"{player?.Username ?? "Unknown Player"} {template.Replace("$", name)}";

        ServerAddon.api.ServerManager.BroadcastMessage(message);
    }

    static void OnFlag(ushort id, SendFlagPacket packet, Packets type, bool isPD)
    {
        if (!ServerAddon.Settings.FlagAllowed(packet.FlagType)) return;

        Announce(id, packet, isPD);
        Broadcast(id, type, packet);
    }

}
