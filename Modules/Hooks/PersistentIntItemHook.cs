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

        var commonId = id.ToLower(); //Regex.Replace(id.ToLower(), " ?\\((\\d+|Clone)\\)$", "");

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
            if (id.StartsWith("bellshrine")) flagType = FlagType.Bellshrine;
            else if (id.StartsWith("library sliding")) flagType = FlagType.Shortcut;
        }

        if (flagType == FlagType.DoNotSync)
        {
            Log.LogDebug($"persistent '{__instance.ItemData.ID}' value was not sent");
            return;
        }

        Log.LogDebug($"[CLI: PERSISTENT INT] {commonId}, {flagType}");

        NetworkSender.AddPersistentIntData(id, scene, value, flagType);
    }
}
