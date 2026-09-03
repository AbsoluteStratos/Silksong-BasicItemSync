using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using System.Text.RegularExpressions;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(PersistentItem<int>))]
internal class PersistentIntItemHook
{
    static readonly string[] CurrencyItems = [
        "rosary",
        "shell",
        "song",
        "ant_",
    ];
    [HarmonyPatch(nameof(PersistentIntItem.SaveStateNoCondition))]
    [HarmonyPrefix]
    static void SaveStateNoCondition_Prefix(PersistentItem<int> __instance, out int __state)
    {
        __state = __instance.ItemData.Value;
    }

    [HarmonyPatch(nameof(PersistentBoolItem.SaveStateNoCondition))]
    [HarmonyPostfix]
    public static void SaveStateNoConditionPostfix(PersistentItem<int> __instance, int __state)
    {
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

        FlagType flagType = FlagType.DoNotSync;

        var commonId = id.ToLower();

        foreach (var item in CurrencyItems)
        {
            if (commonId.StartsWith(item))
            {
                flagType = FlagType.Currency;
                break;
            }
        }

        if (flagType == FlagType.DoNotSync)
        {
            if (commonId.StartsWith("bellshrine")) flagType = FlagType.Bellshrine;
            else if (commonId.StartsWith("library sliding")) flagType = FlagType.Shortcut;
        }

        if (flagType == FlagType.DoNotSync)
        {
            Log.LogDebug($"persistent '{__instance.ItemData.ID}' value was not sent");
            return;
        }

        Log.LogDebug($"[CLI: PERSISTENT INT] {commonId}, {flagType}");

        NetworkSender.AddPersistentIntData(id, scene, value, flagType);
    }

    public static void UpdateValue(PersistentIntItem persistent, int value)
    {
        if (persistent == null) return;
        persistent.SaveState();
        //var preValue = persistent.ItemData.Value;
        //persistent.ItemData.Value = value;
        //SaveStateNoConditionPostfix(persistent, preValue);
    }
}

[HarmonyPatch(typeof(HitSlidePlatform))]
internal static class HitSlidePlatformHook
{
    [HarmonyPatch(nameof(HitSlidePlatform.OnHit))]
    [HarmonyPrefix]
    public static void OnHitPrefix(HitSlidePlatform __instance, out int __state)
    {
        __state = __instance.currentNodeIndex;
    }

    [HarmonyPatch(nameof(HitSlidePlatform.OnHit))]
    [HarmonyPostfix]
    public static void OnHit(HitSlidePlatform __instance, int __state)
    {
        if (__state == __instance.currentNodeIndex)
        {
            Log.LogDebug($"Platform {__instance.name} state was the same ({__state})");
            return;
        }

        PersistentIntItemHook.UpdateValue(__instance.persistent, __instance.currentNodeIndex);
    }
}
