using BasicItemSync.Modules.Network.Client;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BasicItemSync.Modules.Hooks;

internal class EventHooks
{
    static GameObject? HookObj;
    static readonly List<EventRegister> Hooks = [];
    static bool CollectedRecently = false;
    public static void Initialize()
    {
        AddHook("HEART PIECE COLLECTED", OnMask);
        AddHook("SILK SPOOL SAVE", OnSilkSpool);
        AddHook("BEAST DEFEATED", OnBellBeastDefeated);
    }

    public static void Uninitialize()
    {
        foreach (var hook in Hooks)
        {
            EventRegister.UnsubscribeEvent(hook);
        }

        if (HookObj) Object.Destroy(HookObj);
    }

    static void AddHook(string eventName, Action action)
    {
        if (!HookObj)
        {
            HookObj = new GameObject("BasicItemSync Event Hooks");
            Object.DontDestroyOnLoad(HookObj);
        }

        var hook = HookObj.AddComponent<EventRegister>();
        hook.SubscribedEvent = eventName;
        hook.ReceivedEvent += action;
        
        EventRegister.SubscribeEvent(hook);
        Hooks.Add(hook);
    }

    static void SendUpgrade(FlagType upgradeType)
    {
        if (CollectedRecently) return;
        if (ClientState.WasUpgradeReceived(upgradeType)) return;
        
        CollectedRecently = true;
        var scene = SceneManager.GetActiveScene();
        NetworkSender.SendUpgrade(scene.name, upgradeType);

        SyncPlugin.AddNextFrameAction(() =>
        {
            CollectedRecently = false;
        });
    }

    static void OnMask()
    {
        SendUpgrade(FlagType.Mask);
    }

    static void OnSilkSpool()
    {
        SendUpgrade(FlagType.Spool);
    }

    static void OnBellBeastDefeated()
    {
        PlayerData.instance.SetBool(nameof(PlayerData.defeatedBellBeast), true);

        // If silk hearts are on but bosses are off, skip the silk heart once the boss is defeated
        var canHeart = ClientAddon.Settings.FlagAllowed(FlagType.SilkHeart);
        var canBoss = ClientAddon.Settings.FlagAllowed(FlagType.Boss);

        if (canHeart && !canBoss)
        {
            if (!SceneData.instance.PersistentBools.TryGetValue("Bone_05", "Silk Heart", out var persistent))
            {
                Log.LogDebug($"[CLI] No silk heart persistent for Bone_05");
                return;
            }
            
            if (!persistent.Value)
            {
                Log.LogDebug($"[CLI] Haven't accepted silk heart for Bone_05");
                return;
            }

            var bossScene = SceneManager.GetSceneByName("Bone_05_boss");
            if (!bossScene.IsValid()) return;

            Log.LogDebug($"[CLI] Deactivating bell beast");

            foreach (var obj in bossScene.GetRootGameObjects())
            {
                obj.SetActive(false);
            }

            PlayerData.instance.UnlockedFastTravel = true;
        }
    }
}
