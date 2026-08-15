using BasicItemSync.Data;
using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(HeroController))]
internal class HeroControllerHook
{
    // Ensure breaking the cocoon doesn't send other players that money
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

    // Silk Heart
    [HarmonyPatch(nameof(HeroController.AddToMaxSilkRegen))]
    [HarmonyPrefix]
    public static void AddToMaxSilkRegen()
    {
        if (ClientState.WasUpgradeReceived(FlagType.SilkHeart)) return;

        var scene = SceneManager.GetActiveScene().name;
        if (scene == "Memory_Silk_Heart_BellBeast") scene = "Bone_05";

        SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
        {
            ID = ItemNames.SilkHeart,
            SceneName = scene,
            IsSemiPersistent = false,
            Value = true,
            Mutator = SceneData.PersistentMutatorTypes.None
        });

        NetworkSender.SendUpgrade(scene, FlagType.SilkHeart);
    }
}
