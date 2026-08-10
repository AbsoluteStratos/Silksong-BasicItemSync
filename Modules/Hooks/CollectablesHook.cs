using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(CollectableItemPickup))]
internal class CollectableItemPickupHook
{
    static Dictionary<string, FlagType> CollectableTypes = new()
    {
        { "Ant Trapper Item", FlagType.QuestItem },
        { "Architect Key", FlagType.Collectable },
        { "Beastfly Remainss", FlagType.QuestItem },
        { "Belltown House Key", FlagType.Progression },
        { "Blue Goop Jar", FlagType.QuestItem },
        { "Broken SilkShot", FlagType.Tool },
        { "Broodmother Remains", FlagType.QuestItem },
        { "Clover Heart", FlagType.Progression },
        { "Cog Heart Pieces", FlagType.Progression },
        { "Common Spine", FlagType.QuestItem },
        { "Conchfly Remains", FlagType.QuestItem },
        { "Coral Chunk", FlagType.QuestItem },
        { "Coral Heart", FlagType.Progression },
        { "Coral Ingredient", FlagType.QuestItem },
        { "Courier Supplies", FlagType.DoNotSync },
        { "Courier Supplies Mask Maker", FlagType.DoNotSync },
        { "Courier Supplies Slave", FlagType.DoNotSync },
        { "Craw Summons", FlagType.Progression },
        { "Crawbell", FlagType.Collectable },
        { "Crest Socket Unlocker", FlagType.Collectable },
        { "Crow Feather", FlagType.QuestItem },
        { "Crowman Memento", FlagType.Collectable },
        { "Dock Demo Key", FlagType.DoNotSync },
        { "Dock Key", FlagType.Progression },
        { "Dresses", FlagType.DoNotSync },
        { "Enemy Morsel Seared", FlagType.QuestItem },
        { "Enemy Morsel Shredded", FlagType.QuestItem },
        { "Enemy Morsel Speared", FlagType.QuestItem },
        { "Extractor Machine Pins", FlagType.QuestItem },
        { "Farsight", FlagType.Collectable },
        { "Fine Pins", FlagType.QuestItem },
        { "Fixer Idol", FlagType.Quest },
        { "Flower Heart", FlagType.Progression },
        { "Great Shard", FlagType.Collectable },
        { "Grey Memento", FlagType.Collectable },
        { "Growstone", FlagType.Quest },
        { "Hunter Heart", FlagType.Collectable },
        { "Hunter Memento", FlagType.Collectable },
        { "Materium", FlagType.Collectable },
        { "Memento Garmond", FlagType.Collectable },
        { "Memento Seth", FlagType.Collectable },
        { "Memento Surface", FlagType.Collectable },
        { "Mossberry", FlagType.QuestItem },
        { "Mossberry Stew", FlagType.QuestItem },
        { "Pale Oil", FlagType.QuestItem },
        { "Pickled Roach Egg", FlagType.QuestItem },
        { "Pilgrim Rag", FlagType.QuestItem },
        { "Plasmium", FlagType.QuestItem },
        { "Plasmium Blood", FlagType.QuestItem },
        { "Plasmium Gland", FlagType.Quest },
        { "Pristine Core", FlagType.Progression },
        { "Quill", FlagType.Collectable },
        { "R Ancient Egg", FlagType.Collectable },
        { "R Bone Record", FlagType.Collectable },
        { "R Librarian Melody Cylinder", FlagType.Progression },
        { "R Psalm Cylinder", FlagType.Collectable },
        { "R Seal Chit", FlagType.Collectable },
        { "R Weaver Record", FlagType.Collectable },
        { "R Weaver Totem", FlagType.Collectable },
        { "Roach Corpse Item", FlagType.QuestItem },
        { "Rock Roller Item", FlagType.QuestItem },
        { "Rosary_Set_Frayed", FlagType.Collectable },
        { "Rosary_Set_Huge_White", FlagType.Collectable },
        { "Rosary_Set_Large", FlagType.Collectable },
        { "Rosary_Set_Medium", FlagType.Collectable },
        { "Rosary_Set_Small", FlagType.Collectable },
        { "Shard Pouch", FlagType.Collectable },
        { "Shell Flower", FlagType.QuestItem },
        { "Shining Cog", FlagType.QuestItem },
        { "Silk Grub", FlagType.Collectable },
        { "Silver Bellclapper", FlagType.QuestItem },
        { "Simple Key", FlagType.Collectable },
        { "Skull King Fragment", FlagType.QuestItem },
        { "Slab Key", FlagType.Progression },
        { "Snare Soul Bell Hermit", FlagType.Progression },
        { "Snare Soul Churchkeeper", FlagType.Progression },
        { "Snare Soul Swamp Bug", FlagType.Progression },
        { "Song Pilgrim Cloak", FlagType.QuestItem },
        { "Sprintmaster Memento", FlagType.Collectable },
        { "Tool Metal", FlagType.Collectable },
        { "Vintage Nectar", FlagType.QuestItem },
        { "Ward Boss Key", FlagType.Progression },
        { "Ward Key", FlagType.Progression },
        { "White Flower", FlagType.Progression },
        { "Wood Witch Item", FlagType.Progression }
    };
    [HarmonyPatch(nameof(CollectableItemPickup.DoPickupAction))]
    [HarmonyPostfix]
    public static void DoPickupAction(CollectableItemPickup __instance, ref bool __result)
    {
        var key = __instance.Item.name;
        if (ClientState.WasItemReceived(key)) return;

        
        if (!__result || !__instance.Item) return;
        if (!CollectableTypes.TryGetValue(__instance.Item.name, out var flagType))
        {
            if (__instance.Item is CollectableRelic) flagType = FlagType.Collectable;
            else return;
        }

        if (flagType == FlagType.DoNotSync) return;

        var boolName = __instance.playerDataBool;
        PlayerDataHook.BoolUpdated(boolName, true);

        var displayName = __instance.Item.GetPopupName();

        string persistentKey;
        string persistentScene;
        if (__instance.persistent)
        {
            persistentKey = __instance.persistent.ItemData.ID;
            persistentScene = __instance.persistent.ItemData.SceneName;
        }
        else
        {
            persistentKey = __instance.gameObject.name;
            persistentScene = SceneManager.GetActiveScene().name;
        }

        NetworkSender.SendCollectable(key, displayName, persistentKey, persistentScene, flagType);
    }
}