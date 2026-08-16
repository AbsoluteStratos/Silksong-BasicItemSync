using SSMP.Api.Client.Networking;
using System.Collections.Generic;
using System.Linq;

namespace BasicItemSync.Modules.Network.Client
{
    internal class NetworkSender
    {
        static IClientAddonNetworkSender<Packets>? Sender;
        static int RosariesToSend = 0;
        static int ShardsToSend = 0;
        static SendPersistentBoolsPacket PersistentBoolsToSend = new();
        static SendPersistentIntsPacket PersistentIntsToSend = new();
        public static void Initialize()
        {
            Sender = ClientAddon.api.NetClient.GetNetworkSender<Packets>(ClientAddon.Instance);
        }

        static void SendCollectionData(Packets type, Packet packet)
        {
            if (!ClientAddon.api.NetClient.IsConnected || Sender == null)
            {
                Log.LogDebug("Not connected");
                return;
            }

            if (packet is SendFlagPacket flagPacket && !CanSync(flagPacket.FlagType))
            {
                return;
            }

            Log.LogDebug($"[CLI] Sending {type}");
            Sender.SendCollectionData(type, packet);
        }

        static void SendSingleData(Packets type, Packet packet)
        {
            if (!ClientAddon.api.NetClient.IsConnected || Sender == null)
            {
                Log.LogDebug("Not connected");
                return;
            }

            if (packet is SendFlagPacket flagPacket && !CanSync(flagPacket.FlagType))
            {
                return;
            }

            Log.LogDebug($"[CLI] Sending {type}");
            Sender.SendSingleData(type, packet);
        }

        static bool CanSync(FlagType flagType)
        {
            if (!ClientAddon.Settings.FlagAllowed(flagType))
            {
                Log.LogDebug($"[CLI] Syncing {flagType} is disabled.");
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
            if (RosariesToSend == 0 && ShardsToSend == 0) return;

            SendCollectionData(Packets.Currency, new SendCurrencyPacket
            {
                Rosaries = (short)RosariesToSend,
                Shards = (short)ShardsToSend,
            });

            RosariesToSend = 0;
            ShardsToSend = 0;
        }

        public static void SendPendingPersistents()
        {
            if (PersistentBoolsToSend.Values.Count > 0)
            {
                SendCollectionData(Packets.PersistentBool, PersistentBoolsToSend);
            }

            if (PersistentIntsToSend.Values.Count > 0)
            {
                SendCollectionData(Packets.PersistentInt, PersistentIntsToSend);
            }

            PersistentBoolsToSend = new();
            PersistentIntsToSend = new();
        }

        public static void SendFlag(string playerDataKey, FlagType flagType, string name, bool state = true)
        {
            SendCollectionData(Packets.BoolPlayerData, new SendBoolItemPacket
            {
                Key = playerDataKey,
                Name = name,
                FlagType = flagType,
                State = state
            });
        }

        public static void SendInt(string playerDataKey, FlagType flagType, string name, int state)
        {
            SendCollectionData(Packets.IntPlayerData, new SendIntItemPacket
            {
                Key = playerDataKey,
                Name = name,
                FlagType = flagType,
                Number = state
            });
        }

        public static void SendQuestComplete(string internalName, string displayName)
        {
            SendCollectionData(Packets.Quest, new SendBoolItemPacket
            {
                Key = internalName,
                Name = displayName,
                FlagType = FlagType.Quest,
                State = true
            });
        }

        public static void SendTool(string toolName, string displayName, bool state, bool isCrest)
        {
            SendCollectionData(Packets.Tool, new SendBoolItemPacket
            {
                Key = toolName,
                Name = displayName,
                FlagType = isCrest ? FlagType.Crest : FlagType.Tool,
                State = state
            });
        }

        public static void SendUpgrade(string sceneName, FlagType upgradeType)
        {
            SendCollectionData(Packets.Upgrade, new SendFlagPacket
            {
                Key = sceneName,
                Name = "",
                FlagType = upgradeType
            });
        }

        public static void SendCollectable(string key, string itemName, int amount, FlagType flagType = FlagType.Collectable)
        {
            SendCollectionData(Packets.Collectable, new SendIntItemPacket
            {
                Key = key,
                Name = itemName,
                Number = amount,
                FlagType = flagType
            });
        }

        public static void AddPersistentIntData(string id, string scene, int value, FlagType flagType)
        {
            if (PersistentIntsToSend.Values.ContainsKey((scene, id))) return;
            if (!CanSync(flagType)) return;

            PersistentIntsToSend.Values[(scene, id)] = (value, flagType);

            if (PersistentIntsToSend.Values.Count == 1 && PersistentBoolsToSend.Values.Count == 0) SyncPlugin.AddNextFrameAction(SendPendingPersistents);
        }

        public static void AddPersistentBoolData(string id, string scene, bool value, FlagType flagType)
        {
            if (PersistentBoolsToSend.Values.ContainsKey((scene, id))) return;
            if (!CanSync(flagType)) return;

            PersistentBoolsToSend.Values[(scene, id)] = (value, flagType);

            if (PersistentBoolsToSend.Values.Count == 1 && PersistentIntsToSend.Values.Count == 0) SyncPlugin.AddNextFrameAction(SendPendingPersistents);
        }

        public static void SendSettings(Dictionary<string, bool> settings)
        {
            foreach (var setting in settings)
            {
                Log.LogDebug(setting.Key, setting.Value);
            }
            var packet = new SettingsUpdatePacket
            {
                Settings = Server.SyncServerSettings.PopulateFromValues(settings.Values.ToList())
            };

            SendSingleData(Packets.Settings, packet);
        }
    }
}
