using SSMP.Api.Client.Networking;

namespace BasicItemSync.Modules.Network.Client;

internal class NetworkReceiver
{
    static IClientAddonNetworkReceiver<Packets> Receiver;

    public static void Initialize()
    {
        Receiver = ClientAddon.api.NetClient.GetNetworkReceiver<Packets>(ClientAddon.Instance, PacketInstantiate.Instantiate);
        Receiver.RegisterPacketHandler<SettingsUpdatePacket>(Packets.Settings, OnSettingsUpdate);

        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.BoolPlayerData, OnBoolFlag);
        Receiver.RegisterPacketHandler<SendIntItemPacket>(Packets.IntPlayerData, OnIntFlag);
        Receiver.RegisterPacketHandler<SendFloatItemPacket>(Packets.FloatPlayerData, OnFloatFlag);
        
        Receiver.RegisterPacketHandler<SendCurrencyPacket>(Packets.Currency, OnCurrency);
        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.Quest, OnQuestItem);
        Receiver.RegisterPacketHandler<SendBoolItemPacket>(Packets.Tool, OnTool);
        Receiver.RegisterPacketHandler<SendFlagPacket>(Packets.Upgrade, OnUpgrade);
        Receiver.RegisterPacketHandler<SendIntItemPacket>(Packets.Collectable, OnCollectable);
        Receiver.RegisterPacketHandler<SendPersistentBoolsPacket>(Packets.PersistentBool, OnPersistentBools);
        Receiver.RegisterPacketHandler<SendPersistentIntsPacket>(Packets.PersistentInt, OnPersistentInt);
    }

    static bool HandleSpecialData(SendFlagPacket packet)
    {
        UI.ShowPopup(packet.FlagType, packet.Key, packet.Name);

        return false;
    }

    static void OnIntFlag(SendIntItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            if (!HandleSpecialData(packet))
            {
                PlayerData.instance.SetInt(packet.Key, packet.Number);
            }
            ClientState.LastItem = "";
        });
    }

    static void OnFloatFlag(SendFloatItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            if (!HandleSpecialData(packet))
            {
                PlayerData.instance.SetFloat(packet.Key, packet.Number);
            }
            ClientState.LastItem = "";
        });
    }

    static void OnBoolFlag(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            if (!HandleSpecialData(packet))
            {
                PlayerData.instance.SetBool(packet.Key, packet.State);
            }
            ClientState.LastItem = "";

            if (packet.FlagType == FlagType.Boss || packet.FlagType == FlagType.Arena) BattleManager.DefeatBattleScene(packet.Key);
        });
    }

    static void OnCurrency(SendCurrencyPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastCurrency = packet.Rosaries;
            if (packet.Rosaries > 0) CurrencyManager.AddGeo(packet.Rosaries);

            ClientState.LastCurrency = packet.Shards;
            if (packet.Shards > 0) CurrencyManager.AddShards(packet.Shards);
            
            ClientState.LastCurrency = 0;
        });
    }

    static void OnQuestItem(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            if (!HandleSpecialData(packet))
            {
                QuestHandler.EndQuest(packet.Key);
            }
            ClientState.LastItem = "";
        });
    }

    static void OnTool(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;

            if (!HandleSpecialData(packet))
            {
                if (packet.FlagType == FlagType.Crest)
                {
                    var crest = ToolItemManager.GetCrestByName(packet.Key);
                    if (crest)
                    {
                        crest.Unlock();
                    }
                }
                else
                {
                    var tool = ToolItemManager.GetToolByName(packet.Key);
                    if (tool)
                    {
                        PlayerData.instance.SeenToolGetPrompt = true;
                        PlayerData.instance.SeenToolWeaponGetPrompt = true;

                        if (packet.State) tool.Unlock();
                        else tool.Lock();
                    }
                }
            }

            ClientState.LastItem = "";
        });
    }

    static void OnUpgrade(SendFlagPacket packet)
    {
        // quest to handle
        if (!string.IsNullOrEmpty(packet.Name))
        {
            SyncPlugin.AddNextFrameAction(() =>
            {
                ClientState.LastItem = packet.Name;
                if (!HandleSpecialData(packet))
                {
                    QuestHandler.EndQuestSilent(packet.Name);
                }
                ClientState.LastItem = "";
            });
        }

        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastUpgrade = packet.FlagType;
            if (!HandleSpecialData(packet))
            {
                _ = packet.FlagType switch
                {
                    FlagType.Mask => Upgrader.UpgradeMask(packet.Key),
                    FlagType.Spool => Upgrader.UpgradeSpool(packet.Key),
                    FlagType.Pouch => Upgrader.UpgradePouch(),
                    FlagType.CraftingKit => Upgrader.UpgradeCraftingKit(),
                    FlagType.SilkHeart => Upgrader.UpgradeSilkHeart(packet.Key),
                    FlagType.Needle => Upgrader.UpgradeNeedle(),
                    _ => Upgrader.NoOp()
                };
            }
            ClientState.LastUpgrade = FlagType.DoNotSync;
        });
    }


    static void OnCollectable(SendIntItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            if (!HandleSpecialData(packet))
            {
                Upgrader.GiveCollectable(packet.Key, packet.Number);
            }
            ClientState.LastItem = "";
        });
    }

    static void OnPersistentBools(SendPersistentBoolsPacket packet)
    {
        foreach (var item in packet.Values)
        {
            var scene = item.Key.Item1;
            var id = item.Key.Item2;
            var value = item.Value.Item1;

            PersistentHandler.SetPersistentBoolData(scene, id, value);
        }
    }

    static void OnPersistentInt(SendPersistentIntsPacket packet)
    {
        foreach (var item in packet.Values)
        {
            var scene = item.Key.Item1;
            var id = item.Key.Item2;
            var value = item.Value.Item1;
            var flagType = item.Value.Item2;

            PersistentHandler.SetPersistentIntData(scene, id, value, flagType);
        }
    }

    static void OnSettingsUpdate(SettingsUpdatePacket packet)
    {
        ClientAddon.Settings.CopyFrom(packet.Settings);
        Log.LogInfo("[CLI] Received new settings");
    }
}
