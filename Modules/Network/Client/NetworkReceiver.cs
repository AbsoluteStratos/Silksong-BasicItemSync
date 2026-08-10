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
        Receiver.RegisterPacketHandler<SendPersistentBoolPacket>(Packets.Collectable, OnCollectable);
        Receiver.RegisterPacketHandler<SendPersistentBoolPacket>(Packets.PersistentBool, OnPersistentBool);
        Receiver.RegisterPacketHandler<SendPersistentIntPacket>(Packets.PersistentInt, OnPersistentInt);
    }

    static void OnIntFlag(SendIntItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            PlayerData.instance.SetInt(packet.Key, packet.Number);
            ClientState.LastItem = "";
        });
    }

    static void OnFloatFlag(SendFloatItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            PlayerData.instance.SetFloat(packet.Key, packet.Number);
            ClientState.LastItem = "";
        });
    }

    static void OnBoolFlag(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            PlayerData.instance.SetBool(packet.Key, packet.State);
            ClientState.LastItem = "";

            if (packet.FlagType == FlagType.Boss || packet.FlagType == FlagType.Arena) BattleManager.DefeatBattleScene(packet.Key);
        });
    }

    static void OnCurrency(SendCurrencyPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastCurrency = packet.Amount;
            if (packet.CurrencyType == InternalCurrencyType.Rosary)
            {
                CurrencyManager.AddGeo(packet.Amount);
            }
            else
            {
                CurrencyManager.AddShards(packet.Amount);
            }
            ClientState.LastCurrency = 0;
        });
    }

    static void OnQuestItem(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            QuestHandler.EndQuest(packet.Key);
            ClientState.LastItem = "";
        });
    }

    static void OnTool(SendBoolItemPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;

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
                QuestHandler.EndQuestSilent(packet.Name);
                ClientState.LastItem = "";
            });
        }

        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastUpgrade = packet.FlagType;
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
            ClientState.LastUpgrade = FlagType.DoNotSync;
        });
    }


    static void OnCollectable(SendPersistentBoolPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            Upgrader.GiveCollectable(packet.PersistentScene, packet.PersistentObject, packet.Key);
            ClientState.LastItem = "";
        });
    }

    static void OnPersistentBool(SendPersistentBoolPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            PersistentHandler.SetPersistentBoolData(packet.PersistentScene, packet.PersistentObject, packet.State);
            ClientState.LastItem = "";
        });
    }

    static void OnPersistentInt(SendPersistentIntPacket packet)
    {
        SyncPlugin.AddNextFrameAction(() =>
        {
            ClientState.LastItem = packet.Key;
            PersistentHandler.SetPersistentIntData(packet.PersistentScene, packet.PersistentObject, packet.State, packet.FlagType);
            ClientState.LastItem = "";
        });
    }

    static void OnSettingsUpdate(SettingsUpdatePacket packet)
    {
        ClientAddon.Settings.CopyFrom(packet.Settings);
        Log.LogInfo("Received new settings");
    }
}
