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
    static List<EventRegister> Hooks = [];
    static bool CollectedRecently = false;
    public static void Initialize()
    {
        AddHook("HEART PIECE COLLECTED", OnMask);
        AddHook("SILK SPOOL SAVE", OnSilkSpool);
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

    //static void OnSilkHeart()
    //{
    //    SendUpgrade(FlagType.SilkHeart);
    //}

    static void OnPouch()
    {
        SendUpgrade(FlagType.Pouch);
    }

    static void OnCraftingKit()
    {
        SendUpgrade(FlagType.CraftingKit);
    }
}
