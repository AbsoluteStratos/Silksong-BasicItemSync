using SSMP.Api.Client.Networking;
using System.Collections.Generic;

namespace BasicItemSync.Modules.Network.Client
{
    internal class NetworkSender
    {
        static IClientAddonNetworkSender<Packets>? Sender;
        static int RosariesToSend = 0;
        static int ShardsToSend = 0;
        static Dictionary<(string, string), SendPersistentPacket> PersistentsToSend = [];
        public static void Initialize()
        {
            Sender = ClientAddon.api.NetClient.GetNetworkSender<Packets>(ClientAddon.Instance);
        }

        static void SendData(Packets type, ClientPacket packet)
        {
            if (!ClientAddon.api.NetClient.IsConnected || Sender == null)
            {
                Log.LogInfo("Not connected");
                return;
            }

            if (packet is SendFlagPacket flagPacket && !CanSync(flagPacket.FlagType))
            {
                return;
            }

            Log.LogInfo($"[CLI] Sending {type}");
            Sender.SendCollectionData(type, packet);
        }

        static bool CanSync(FlagType flagType)
        {
            if (!ClientAddon.Settings.FlagAllowed(flagType))
            {
                Log.LogWarning($"[CLI] Syncing {flagType} is disabled.");
                return false;
            }

            return true;
        }

        public static void AddCurrency(InternalCurrencyType currencyType, int amount)
        {
            if (!CanSync(FlagType.Currency)) return;

            if (currencyType == InternalCurrencyType.Rosary) RosariesToSend += amount;
            else ShardsToSend += amount;

        }

        public static void SendPendingCurrency()
        {
            if (RosariesToSend > 0)
            {
                SendData(Packets.Currency, new SendCurrencyPacket
                {
                    CurrencyType = InternalCurrencyType.Rosary,
                    Amount = RosariesToSend
                });
            }

            if (ShardsToSend > 0)
            {
                SendData(Packets.Currency, new SendCurrencyPacket
                {
                    CurrencyType = InternalCurrencyType.ShellShard,
                    Amount = ShardsToSend
                });
            }

            RosariesToSend = 0;
            ShardsToSend = 0;
        }

        public static void SendPendingPersistents()
        {
            if (PersistentsToSend.Count > 0)
            {
                foreach (var packet in PersistentsToSend.Values)
                {
                    SendData(Packets.PersistentBool, packet);
                }
            }

            PersistentsToSend.Clear();
        }

        public static void SendFlag(string playerDataKey, FlagType flagType, string name, bool state = true)
        {
            SendData(Packets.BoolPlayerData, new SendBoolItemPacket
            {
                Key = playerDataKey,
                Name = name,
                FlagType = flagType,
                State = state
            });
        }

        public static void SendInt(string playerDataKey, FlagType flagType, string name, int state)
        {
            SendData(Packets.IntPlayerData, new SendIntItemPacket
            {
                Key = playerDataKey,
                Name = name,
                FlagType = flagType,
                Number = state
            });
        }

        public static void SendQuestComplete(string internalName, string displayName)
        {
            SendData(Packets.Quest, new SendBoolItemPacket
            {
                Key = internalName,
                Name = displayName,
                FlagType = FlagType.Quest,
                State = true
            });
        }

        public static void SendTool(string toolName, string displayName, bool state, bool isCrest)
        {
            SendData(Packets.Tool, new SendBoolItemPacket
            {
                Key = toolName,
                Name = displayName,
                FlagType = isCrest ? FlagType.Crest : FlagType.Tool,
                State = state
            });
        }

        public static void SendUpgrade(string sceneName, FlagType upgradeType)
        {
            SendData(Packets.Upgrade, new SendFlagPacket
            {
                Key = sceneName,
                Name = "",
                FlagType = upgradeType
            });
        }

        public static void SendCollectable(string key, string itemName, string persistentKey, string persistentScene, FlagType flagType = FlagType.Collectable)
        {
            SendData(Packets.Collectable, new SendPersistentBoolPacket
            {
                Key = key,
                Name = itemName,
                FlagType = flagType,
                PersistentObject = persistentKey,
                PersistentScene = persistentScene
            });
        }

        public static void AddPersistentIntData(string id, string scene, int value, FlagType flagType)
        {
            if (PersistentsToSend.ContainsKey((scene, id))) return;
            PersistentsToSend[(scene, id)] = new SendPersistentIntPacket
            {
                Key = "",
                Name = "",
                FlagType = flagType,
                PersistentObject = id,
                PersistentScene = scene,
                State = value
            };

            if (PersistentsToSend.Count == 1) SyncPlugin.AddNextFrameAction(SendPendingPersistents);
        }

        public static void AddPersistentBoolData(string id, string scene, bool value, FlagType flagType)
        {
            if (PersistentsToSend.ContainsKey((scene, id))) return;
            PersistentsToSend[(scene, id)] = new SendPersistentBoolPacket
            {
                Key = "",
                Name = "",
                FlagType = flagType,
                PersistentObject = id,
                PersistentScene = scene,
                State = value
            };

            if (PersistentsToSend.Count == 1) SyncPlugin.AddNextFrameAction(SendPendingPersistents);
        }
    }
}
