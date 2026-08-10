using HarmonyLib;
using System.Reflection;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch]
internal static class GameManagerIntHook
{
    static MethodInfo TargetMethod()
    {
        var method = typeof(GameManager).GetMethod(nameof(GameManager.SetPlayerDataVariable), BindingFlags.Public | BindingFlags.Instance);
        return method.MakeGenericMethod(typeof(int));
    }

    static void Prefix(string fieldName, int value)
    {
        PlayerDataHook.IntUpdated(fieldName, value);
    }
}


[HarmonyPatch(typeof(QuestTargetPlayerDataInt))]
internal static class QuestTargetPlayerDataIntHook
{
    [HarmonyPatch(nameof(QuestTargetPlayerDataInt.Get))]
    [HarmonyPrefix]
    public static void Get(QuestTargetPlayerDataInt __instance)
    {
        var boolName = __instance.playerDataInt;
        var value = 1;

        PlayerDataHook.IntUpdated(boolName, value, PlayerDataIntOperation.Operation.Add);
    }
}