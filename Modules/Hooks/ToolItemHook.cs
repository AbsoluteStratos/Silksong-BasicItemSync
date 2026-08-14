using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using TeamCherry.Localization;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(ToolItem))]
internal static class ToolItemHook
{
    [HarmonyPatch(nameof(ToolItem.Unlock))]
    [HarmonyPostfix]
    public static void Unlock(ToolItem __instance)
    {
        if (__instance.IsUnlocked && !ClientState.WasItemReceived(__instance.name))
        {
            NetworkSender.SendTool(__instance.name, __instance.GetPopupName(), true, false);
        }
    }

    [HarmonyPatch(nameof(ToolItem.Lock))]
    [HarmonyPostfix]
    public static void Lock(ToolItem __instance)
    {
        if (!__instance.IsUnlocked && !ClientState.WasItemReceived(__instance.name))
        {
            NetworkSender.SendTool(__instance.name, __instance.GetPopupName(), false, false);
        }
    }

    [HarmonyPatch(nameof(ToolItem.SetUnlockedTestsComplete))]
    [HarmonyPrefix]
    public static void SetUnlockedTestsComplete(ToolItem __instance)
    {
        foreach (var group in __instance.alternateUnlockedTest.TestGroups)
        {
            foreach (var test in group.Tests)
            {
                if (test.Type == PlayerDataTest.TestType.Bool) PlayerDataHook.BoolUpdated(test.FieldName, test.BoolValue);
            }
        }
    }
}

[HarmonyPatch(typeof(ToolCrest))]
internal static class ToolCrestHook
{
    [HarmonyPatch(nameof(ToolCrest.Unlock))]
    [HarmonyPostfix]
    public static void Unlock(ToolCrest __instance)
    {
        if (__instance.IsUnlocked && !ClientState.WasItemReceived(__instance.name))
        {
            if (__instance.name == "Cursed" || __instance.name == "Cloakless") return;

            var name = Language.GetLocal(__instance.DisplayName);
            NetworkSender.SendTool(__instance.name, name, true, true);
        }
    }
}