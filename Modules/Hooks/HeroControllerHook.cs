using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(HeroController))]
internal class HeroControllerHook
{
    [HarmonyPatch(nameof(HeroController.CocoonBroken), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static void CocoonBrokenPrefix(out int __state)
    {
        __state = ClientState.LastCurrency;

        var corpseMoney = PlayerData.instance.HeroCorpseMoneyPool;
        if (corpseMoney == 0) return;

        ClientState.LastCurrency = corpseMoney;
    }

    [HarmonyPatch(nameof(HeroController.CocoonBroken), typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    public static void CocoonBrokenPostfix(int __state)
    {
        ClientState.LastCurrency = __state;
    }

    public static bool CollectedBeast;
    [HarmonyPatch(nameof(HeroController.AddToMaxSilkRegen))]
    [HarmonyPrefix]
    public static void AddToMaxSilkRegen()
    {
        if (ClientState.WasUpgradeReceived(FlagType.SilkHeart)) return;

        if (SceneManager.GetActiveScene().name == "Bone_05") CollectedBeast = true;
        NetworkSender.SendUpgrade(SceneManager.GetActiveScene().name, FlagType.SilkHeart);
    }
}
