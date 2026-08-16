using BasicItemSync.Modules.Network.Server;

namespace BasicItemSync;

internal class Common
{
    internal const string AddonName = "BasicItemSync";
    internal const string AddonVersion = "0.1.1";
    internal const int AddonApiVersion = 1;
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
    QuestStart,
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