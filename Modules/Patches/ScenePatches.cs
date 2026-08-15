using BasicItemSync.Data;
using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using System.Linq;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules.Patches;

[HarmonyPatch(typeof(SceneLoad))]
internal class ScenePatches
{
    [HarmonyPatch(nameof(SceneLoad.Begin))]
    [HarmonyPrefix]
    static void Begin(SceneLoad __instance)
    {
        __instance.Finish += () => OnFinish(__instance);
    }

    private static void OnFinish(SceneLoad __instance)
    {
        var scene = __instance.OperationHandle.Result.Scene;
        if (!scene.IsValid()) return;

        if (scene.name == "Bone_05") OnBellBeast(scene);
    }

    /// <summary>
    /// If Silk Hearts are off but bosses are on, enable the silk heart object when they enter the scene.
    /// </summary>
    static void OnBellBeast(Scene scene)
    {
        var canHeart = ClientAddon.Settings.FlagAllowed(FlagType.SilkHeart);
        var canBoss = ClientAddon.Settings.FlagAllowed(FlagType.Boss);
        
        if (!canHeart && canBoss)
        {
            if (!SceneData.instance.PersistentBools.TryGetValue(scene.name, ItemNames.SilkHeart, out var persistent)) return;
            if (persistent.Value) return;
            
            var heart = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Silk Heart");
            if (!heart) return;

            heart.SetActive(true);
        }
    }
}

/// Bell Beast 
///
/// No Sync:
/// Once bell beast is defeated, store a PersistentBool for that silk heart, value is false
/// When a silk heart is collected, set the PersistentBool for that silk heart to true
/// 
/// All Sync:
/// Once bell beast is defeated, disable the fight for all other players
/// Once bell beast is defeated, store a PersistentBool for that silk heart, value is false
/// The player that defeated bell beast must collect the silk heart (may cause problems if they don't)
/// When a silk heart is collected, set the PersistentBool for that silk heart to true
/// 
/// Sync Bosses, No Silk Hearts
/// Once bell beast is defeated, disable the fight for all other players
/// Once bell beast is defeated, store a PersistentBool for that silk heart, value of false
/// For players currently in the scene, enable the original silk heart object
/// For players entering the scene, enable the other silk heart object if the PersistentBool is still false
/// When a silk heart is collected, set the PersistentBool for that silk heart to true
/// 
/// No Bosses, Sync Silk Hearts
/// When a silk heart is collected, set the PersistentBool for that silk heart to true
/// Once bell beast is defeated:
///     a) if the PersistentBool is false or non-existent, go to the memory scene
///     b) if the PersistentBool is true, disable the entire boss scene
