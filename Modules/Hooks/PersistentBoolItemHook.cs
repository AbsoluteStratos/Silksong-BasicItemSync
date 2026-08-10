using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(PersistentItem<bool>))]
internal class PersistentBoolItemHook
{
    static readonly Dictionary<string, FlagType> PersistentTypes = new()
    {
        { "moss vine cluster",              FlagType.Shortcut },
        { "abyss_collapse_wall_bot",        FlagType.Shortcut },
        { "shaman_collapse_wall",           FlagType.Shortcut },
        { "explode_wall",                   FlagType.Shortcut },
        { "explode_wall 4",                 FlagType.Shortcut },
        { "explode_wall fake",              FlagType.Shortcut },
        { "abyss_collapse_wall",            FlagType.Shortcut },
        { "weaver_lift_power_chamber",      FlagType.Shortcut },
        { "ant_lever_persistent",           FlagType.Shortcut },
        { "one way wall",                   FlagType.Shortcut },
        { "collapser small",                FlagType.Shortcut },
        { "collapser small_bell_type",      FlagType.Shortcut },
        { "breakable wall aqueduct start",  FlagType.Shortcut },
        { "breakable wall",                 FlagType.Shortcut },
        { "breakable wall bot mid",         FlagType.Shortcut },
        { "breakable wall_silhouette",      FlagType.Shortcut },
        { "drop_planks",                    FlagType.Shortcut },
        { "inverse remasker",               FlagType.Shortcut },
        { "grouped remasker",               FlagType.Shortcut },
        { "grouped remasker_large",         FlagType.Shortcut },
        { "secret mask",                    FlagType.Shortcut },
        { "secret mask new",                FlagType.Shortcut },
        { "secret mask new sharp ultra",    FlagType.Shortcut },
        { "secret mask new gate",           FlagType.Shortcut },
        { "slab_vent_mask",                 FlagType.Shortcut },
        { "breakable window",               FlagType.Shortcut },
        { "breakable window arborium",      FlagType.Shortcut },
        { "coral crust wall sphere",        FlagType.Shortcut },
        { "crust wall",                     FlagType.Shortcut },
        { "coral crust wall tall",          FlagType.Shortcut },
        { "coral crust wall mid",           FlagType.Shortcut },
        { "coral crust wall small",         FlagType.Shortcut },
        { "song_lever_side old",            FlagType.Shortcut },
        { "song_lever_side",                FlagType.Shortcut },
        { "song_lever_side right",          FlagType.Shortcut },
        { "bell wall tall",                 FlagType.Shortcut },
        { "mines lift",                     FlagType.Shortcut },
        { "act3_bellway_01_breakable_wall", FlagType.Shortcut },
        { "wind rock",                      FlagType.Shortcut },
        { "plank_wall_cluster",             FlagType.Shortcut },
        { "understore lever",               FlagType.Shortcut },
        { "gg_breakable_junk_pile_red",     FlagType.Shortcut },
        { "charm_break_wall",               FlagType.Shortcut },
        { "bone lever",                     FlagType.Shortcut },
        { "chain drop platform",            FlagType.Shortcut },
        { "bone gate",                      FlagType.Shortcut },
        { "fall_platform_barrels",          FlagType.Shortcut },
        { "lever",                          FlagType.Shortcut },
        { "song_gate_small",                FlagType.Shortcut },
        { "trapdoor lever",                 FlagType.Shortcut },
        { "plate",                          FlagType.Shortcut },
        { "bone carriage",                  FlagType.Shortcut },
        { "barrel plat lift",               FlagType.Shortcut },
        { "dockdashexploderock",            FlagType.Shortcut },
        { "explode_wall (7) - boneEast07_openedMidRoof",    FlagType.Shortcut },
        { "bone_east_10_church",            FlagType.Shortcut },
        { "toll door",                      FlagType.Shortcut },
        { "toll door breakable cog",        FlagType.Shortcut },
        { "dropbomb rock",                  FlagType.Shortcut },
        { "bone east 11 cross over group",  FlagType.Shortcut },
        { "coal bucket plat",               FlagType.Shortcut },
        { "explode_wall_norck",             FlagType.Shortcut },
        { "hornet_pressure_plate_small_persistent",         FlagType.Shortcut },
        { "clover gate",                    FlagType.Shortcut },
        { "cog_lever",                      FlagType.Shortcut },
        { "active",                         FlagType.Shortcut },
        { "cog_junk_pile_break",            FlagType.Shortcut },
        { "pipe_vent_hatch broken",         FlagType.Shortcut },
        { "plank_wall_cluster_metal",       FlagType.Shortcut },
        { "slide_gate_ring",                FlagType.Shortcut },
        { "coral10rightsidegate",           FlagType.Shortcut },
        { "coral crust mask",               FlagType.Shortcut },
        { "coral_gate_large",               FlagType.Shortcut },
        { "stalactite group bottom",        FlagType.Shortcut },
        { "stalactite group top",           FlagType.Shortcut },
        { "breakable blocker struts",       FlagType.Shortcut },
        { "cradle_plat",                    FlagType.Shortcut },
        { "cradle_spike_plat",              FlagType.Shortcut },
        { "aspid_sealed_gate_stone",        FlagType.Shortcut },
        { "harpoon ring pull switch",       FlagType.Shortcut },
        { "dock_pressure_plate_lock",       FlagType.Shortcut },
        { "greymoor_lever_simple_edited",   FlagType.Shortcut },
        { "Dust_02Gate",                    FlagType.Shortcut },
        { "greymoor_lever_simple",          FlagType.Shortcut },
        { "break wall dustpen cage",        FlagType.Shortcut },
        { "plat collider",                  FlagType.Shortcut },
        { "bell chain plat",                FlagType.Shortcut },
        { "greymoor stand lever - bridge",  FlagType.Shortcut },
        { "greymoor stand lever - crane",   FlagType.Shortcut },
        { "hornet_cage_rect_lift_greymoor", FlagType.Shortcut },
        { "propeller",                      FlagType.Shortcut },
        { "greymoor_breakable_plat",        FlagType.Shortcut },
        { "belltown_lever_drop_plat",       FlagType.Shortcut },
        { "greymoor_drop_propeller",        FlagType.Shortcut },
        { "grey_wood_chain_plat_mid_breakable",             FlagType.Shortcut },
        { "greymoor stand lever",           FlagType.Shortcut },
        { "greystore_floor_shortcut",       FlagType.Shortcut },
        { "bank_door",                      FlagType.Shortcut },
        { "bot blocker",                    FlagType.Shortcut },
        { "innerworks_lever",               FlagType.Shortcut },
        { "top shortcut",                   FlagType.Shortcut },
        { "attic_ladder",                   FlagType.Shortcut },
        { "understore propeller",           FlagType.Shortcut },
        { "gg_breakable_junk_pile_gold",    FlagType.Shortcut },
        { "understore big plate gate",      FlagType.Shortcut },
        { "barrel plat lift thin",          FlagType.Shortcut },
        { "plug_gate",                      FlagType.Shortcut },
        { "stalactite group",               FlagType.Shortcut },
        { "vine platform",                  FlagType.Shortcut },
        { "bone lever 2",                   FlagType.Shortcut },
        { "thick silk vines",               FlagType.Shortcut },
        { "shellwood twig wall",            FlagType.Shortcut },
        { "hornet_cage_rect_lift_organ",    FlagType.Shortcut },
        { "organ_lift_broken_drop",         FlagType.Shortcut },
        { "one way wall crystal",           FlagType.Shortcut },
        { "junk_pile_break_peak",           FlagType.Shortcut },
        { "crystal break chunk",            FlagType.Shortcut },
        { "drop pillar_chunk",              FlagType.Shortcut },
        { "sack_multi_breakable",           FlagType.Shortcut },
        { "diving_bell_door_breakable",     FlagType.Shortcut },
        { "dock_sealed_gate",               FlagType.Shortcut },
        { "cage_mid_small_long_sway drop",  FlagType.Shortcut },
        { "breakablewallswampbenchshortcut",FlagType.Shortcut },
        { "plank_wall_cluster_swamp",       FlagType.Shortcut },
        { "witch cluster vine",             FlagType.Shortcut },
        { "drop_planks_horizontal",         FlagType.Shortcut },
        { "gloom_lift_destroy",             FlagType.Shortcut },
        { "hornet_pressure_plate",          FlagType.Shortcut },
        { "shellwood hive",                 FlagType.Shortcut },
        { "hornet_cage_rect_lift",          FlagType.Shortcut },
        { "plank solid 2",                  FlagType.Shortcut },
        { "shellwood twig wall start",      FlagType.Shortcut },
        { "shellwood twig wall top",        FlagType.Shortcut },
        { "slab_jail_lever",                FlagType.Shortcut },
        { "door_slabcaged",                 FlagType.Shortcut },
        { "slab lock",                      FlagType.Shortcut },
        { "slab_cage_plat_thin_fall",       FlagType.Shortcut },
        { "slab lock l",                    FlagType.Shortcut },
        { "slab lock r",                    FlagType.Shortcut },
        { "breakable wall waterways",       FlagType.Shortcut },
        { "jail gate door - persistent",    FlagType.Shortcut },
        { "breakable floor_basic song_01",  FlagType.Shortcut },
        { "breakable wall entrance secret", FlagType.Shortcut },
        { "breakable wall ruin lift",       FlagType.Shortcut },
        { "chain platform breaker",         FlagType.Shortcut },
        { "breakable wall  second",         FlagType.Shortcut },
        { "propeller understore propeller", FlagType.Shortcut },
        { "hatch",                          FlagType.Shortcut },
        { "song_lever_side - merchant quest accepted",      FlagType.Shortcut },
        { "breakable floor_basic",          FlagType.Shortcut },
        { "cradle_pipe_trapdoor",           FlagType.Shortcut },
        { "one way vinewall",               FlagType.Shortcut },
        { "snail_collapse_wall",            FlagType.Shortcut },
        { "pipe_cog_lever",                 FlagType.Shortcut },
        { "barrel plat lift dropper bottom",                FlagType.Shortcut },
        { "barrel plat lift dropper",       FlagType.Shortcut },
        { "collapser small understore grate",               FlagType.Shortcut },
        { "fall_platform_barrel wide",      FlagType.Shortcut },
        { "cog_05_shortcut",                FlagType.Shortcut },
        { "innerworks trap bridge large",   FlagType.Shortcut },
        { "architect shrine door",          FlagType.Shortcut },
        { "fall platform support pole",     FlagType.Shortcut },
        { "fall_platform_barrel",           FlagType.Shortcut },
        { "understore lever l main",        FlagType.Shortcut },
        { "under_lift_large",               FlagType.Shortcut },
        { "lock_machine",                   FlagType.Shortcut },
        { "pipe_vent_hatch",                FlagType.Shortcut },
        { "ward_junk_pile_break",           FlagType.Shortcut },
        { "ward_junk_pile_break 1",         FlagType.Shortcut },
        { "ward_junk_pile_break 2",         FlagType.Shortcut },
        { "ward_junk_pile_break 3",         FlagType.Shortcut },
        { "ward_junk_pile_break 4",         FlagType.Shortcut },
        { "junk hatch",                     FlagType.Shortcut },
        { "crest_shrine_break_tube",        FlagType.Shortcut },
        { "loom_room_jar2",                 FlagType.Shortcut },

        { "ant_item_string",                    FlagType.Collectable },
        { "tool_metal_deposit",                 FlagType.Collectable },
        { "ladybug craft pickup",               FlagType.CraftingKit },
        { "silk grub large cocoon",             FlagType.Collectable },
        { "silk grub small cocoon 1",           FlagType.Collectable },
        { "silk grub small cocoon 2",           FlagType.Collectable },
        { "breakable hang sack memory locket",  FlagType.Collectable },
        { "collectable item pickup",            FlagType.Collectable },
        { "slab_item_chain",                    FlagType.Collectable },
        { "breakable hang sack 3",              FlagType.Collectable },

        { "heart piece",                        FlagType.Mask },
        { "library_glass_heart_piece",          FlagType.Mask },
        { "silk spool",                         FlagType.Spool },
        { "lamp_hang_top",                      FlagType.Spool },
        { "moss_berry_fruit",                   FlagType.QuestItem },
        { "nectar pickup",                      FlagType.QuestItem },

        { "bone chest",                         FlagType.Currency },
        { "geo small persistent",               FlagType.Currency },
        { "geo mid persistent",                 FlagType.Currency },
        { "ant chest",                          FlagType.Currency },
        { "shell shard 01 persistent",          FlagType.Currency },
        { "shell shard 02 persistent",          FlagType.Currency },
        { "shell shard 03 persistent",          FlagType.Currency },
        { "city shard chest",                   FlagType.Currency },
        { "pilgrim chest",                      FlagType.Currency },
        { "craw_home_front",                    FlagType.Currency },
        { "craw_home_right",                    FlagType.Currency },
        { "craw_home_hang",                     FlagType.Currency },
        { "chest",                              FlagType.Currency },
        { "shell shard fossil shellwood",       FlagType.Currency },
        { "churchkeeper_rosary",                FlagType.Currency },

        { "cog_choir_cylinder",                 FlagType.Progression },
        { "thread_memory_orb_source",           FlagType.Progression },
        { "shrine pressure plate l",            FlagType.Progression },
        { "shrine pressure plate TR",           FlagType.Progression },
        { "shrine pressure plate BR",           FlagType.Progression },
        { "plaque pressure plate",              FlagType.Progression },
        { "memory orb group",                   FlagType.Progression },
        { "song knight tube",                   FlagType.Progression },
        { "diving bell door",                   FlagType.Progression },
        { "prince cell door",                   FlagType.Progression },
        { "slab_break_weaver_gate_ring",        FlagType.Progression },

        { "organ webbed bench",                 FlagType.Bench },
        { "fake bench collapser",               FlagType.Bench },
    };

    [HarmonyPatch(nameof(PersistentBoolItem.Start))]
    [HarmonyPostfix]
    static void Start(PersistentItem<bool> __instance)
    {
        if (!__instance.fsm || __instance.ItemData.Value) return;

        foreach (var state in __instance.fsm.FsmStates)
        {
            var index = -1;
            for (var i = 0; i < state.Actions.Length; i++)
            {
                if (
                    (state.Actions[i] is SetFsmBool setBool && setBool.variableName.Value == "Activated") ||
                    (state.Actions[i] is SetBoolValue setValue && setValue.boolVariable.Name == "Activated"))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1) continue;

            var action = new PersistentHook();
            action.Init(state);

            var actions = state.Actions.ToList();
            actions.Insert(index + 1, action);
            state.Actions = actions.ToArray();
            state.SaveActions();
            break;
        }
    }

    [HarmonyPatch(nameof(PersistentBoolItem.SaveStateNoCondition))]
    [HarmonyPrefix]
    static void SaveStateNoConditionPrefix(PersistentItem<bool> __instance, out bool __state)
    {
        __state = __instance.ItemData.Value;
    }

    [HarmonyPatch(nameof(PersistentBoolItem.SaveStateNoCondition))]
    [HarmonyPostfix]
    public static void SaveStateNoConditionPostfix(PersistentItem<bool> __instance, bool __state) {
        if (__state == __instance.ItemData.Value)// || __instance.ItemData.Value == __instance.DefaultValue)
        {
            Log.LogDebug($"persistent '{__instance.ItemData.ID}' value was the same ({__instance.ItemData.Value}), skipping");
            return;
        }

        if (__instance.itemData.IsSemiPersistent || __instance.dontSave)
        {
            Log.LogDebug($"persistent '{__instance.ItemData.ID}' value was semipersistent");
            return;
        }

        var id = __instance.itemData.ID;
        var scene = __instance.itemData.SceneName;
        var value = __instance.itemData.Value;

        FlagType flagType;

        var commonId = Regex.Replace(id.ToLower(), " ?\\((\\d+|Clone)\\)$", "");
        if (__instance.TryGetComponent<Gate>(out var _)) flagType = FlagType.Shortcut;
        //else if (__instance.TryGetComponent<Lever>(out var _)) flagType = FlagType.Shortcut; // pesky dial door bridge in Song_20b
        //else if (__instance.TryGetComponent<BattleScene>(out var _)) flagType = FlagType.Arena;
        else if (PersistentTypes.TryGetValue(commonId, out var flag)) flagType = flag;
        else if (commonId.StartsWith("remask")) flagType = FlagType.Shortcut;
        else if (commonId.StartsWith("black_thread_core")) flagType = FlagType.Boss;
        else if (commonId.StartsWith("bell_toll")) flagType = FlagType.Bench;
        else if (commonId.StartsWith("battle scene") || commonId.StartsWith("black thread battle scene") || commonId.StartsWith("boss scene")) flagType = FlagType.Arena;
        else
        {
            Log.LogDebug($"persistent '{__instance.ItemData.ID}' value was not sent");
            return;
        }

        Log.LogDebug($"[CLI: PERSISTENT] {commonId}, {flagType}");

        NetworkSender.AddPersistentBoolData(id, scene, value, flagType);
    }
}


[HarmonyPatch(typeof(Lever))]
internal static class LeverHook
{
    [HarmonyPatch(nameof(Lever.Hit))]
    [HarmonyPostfix]
    public static void Hit(Lever __instance, ref IHitResponder.HitResponse __result)
    {
        if (__result.response == IHitResponder.Response.None || __instance.doesNotActivate) return;
        var boolName = __instance.playerDataBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);

        if (__instance.persistent && !__instance.GetComponentInParent<DialDoorBridge>())
        {
            __instance.persistent.ItemData.Value = true;
            PersistentBoolItemHook.SaveStateNoConditionPostfix(__instance.persistent, false);

            //NetworkSender.AddPersistentData(info.ID, info.SceneName, value, FlagType.Shortcut);
        }
    }
}

[HarmonyPatch(typeof(Lever_tk2d))]
internal static class Lever_tk2dHook
{
    [HarmonyPatch(nameof(Lever_tk2d.Hit))]
    [HarmonyPrefix]
    public static void Hit(Lever_tk2d __instance, HitInstance damageInstance)
    {
        var damage = damageInstance;
        if (!damage.IsNailDamage || (__instance.canHitTrigger && !__instance.canHitTrigger.IsInside) || __instance.activated) return;

        if (__instance.TryGetComponent<PersistentBoolItem>(out var persistent))
        {
            persistent.ItemData.Value = true;
            PersistentBoolItemHook.SaveStateNoConditionPostfix(persistent, false);
        }
    }
}

class PersistentHook : FsmStateAction
{
    public override void OnEnter()
    {
        var obj = Fsm.Owner.gameObject;
        if (!obj) return;

        var persistent = obj.GetComponent<PersistentBoolItem>();
        if (!persistent) return;

        persistent.SaveState();
        Finish();
    }
}