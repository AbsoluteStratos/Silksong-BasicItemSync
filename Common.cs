using BasicItemSync.Modules.Network.Server;

namespace BasicItemSync;

internal class Common
{
    internal const string AddonName = "BasicItemSync";
    internal const string AddonVersion = "0.1.0";
    internal const int AddonApiVersion = 1;


    public static bool FlagAllowed(SyncServerSettings settings, FlagType flag)
    {
        if (ServerAddon.Settings.KillSwitch) return false;

        return flag switch
        {
            FlagType.Ability => settings.SyncAbilities,
            FlagType.Map => settings.SyncMaps,
            FlagType.Pin => settings.SyncPins,
            FlagType.Bellshrine => settings.SyncProgression,
            FlagType.Boss => settings.SyncBattles,
            FlagType.Arena => settings.SyncBattles,
            FlagType.Progression => settings.SyncProgression,
            FlagType.Flea => settings.SyncCollectables,
            FlagType.Collectable => settings.SyncCollectables,
            FlagType.Bellway => settings.SyncTransit,
            FlagType.Ventrica => settings.SyncTransit,
            FlagType.Currency => settings.SyncCurrency,

            FlagType.Mask => settings.SyncUpgrades,
            FlagType.Spool => settings.SyncUpgrades,
            FlagType.Pouch => settings.SyncUpgrades,
            FlagType.CraftingKit => settings.SyncUpgrades,
            FlagType.Quest => settings.SyncQuests,
            FlagType.Tool => settings.SyncTools,
            FlagType.Crest => settings.SyncTools,
            FlagType.SilkHeart => settings.SyncUpgrades,
            FlagType.Needle => settings.SyncUpgrades,
            FlagType.Bench => settings.SyncProgression,
            FlagType.Shortcut => true,
            FlagType.QuestItem => settings.SyncQuestItems,
            FlagType.DoNotSync => false,
            _ => false,
        };
    }
}

internal enum FlagType
{
    Ability,
    Map,
    Pin,
    Bellshrine,
    Boss,
    Arena,
    Progression,
    Collectable,
    Bellway,
    Ventrica,
    Mask,
    Spool,
    Pouch,
    CraftingKit,
    SilkHeart,
    Needle,
    Quest,
    QuestItem,
    Tool,
    Crest,
    Currency,
    Shortcut,
    Bench,
    Flea,
    DoNotSync
}

internal enum InternalCurrencyType
{
    Rosary,
    ShellShard,
}

internal enum Packets
{
    BoolPlayerData,
    IntPlayerData,
    FloatPlayerData,
    Collectable,
    Currency,
    Quest,
    Tool,
    Upgrade,
    Settings,
    PersistentBool,
    PersistentInt,
}