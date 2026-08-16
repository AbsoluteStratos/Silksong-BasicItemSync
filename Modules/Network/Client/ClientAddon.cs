using BasicItemSync.Modules.Hooks;
using BasicItemSync.Modules.Network.Server;
using SSMP.Api.Client;
using System;
using System.Collections;
using UnityEngine;

namespace BasicItemSync.Modules.Network.Client;

internal class ClientAddon : SSMP.Api.Client.ClientAddon
{
    public override bool NeedsNetwork => true;
    public override uint ApiVersion => Common.AddonApiVersion;
    protected override string Name => Common.AddonName;
    protected override string Version => Common.AddonVersion;

    public static IClientApi api;

    public static ClientAddon Instance;

    public static SyncServerSettings Settings = new();

    static readonly WaitForSeconds CurrencyTimer = new(4);
    static Coroutine? CurrencyRoutine;
    public override void Initialize(IClientApi clientApi)
    {
        Log.SetLogger(Logger);
        Log.LogInfo("Sync Client Addon enabled");
        Settings = new();
        Instance = this;
        api = clientApi;
        NetworkSender.Initialize();
        NetworkReceiver.Initialize();

        api.CommandManager.RegisterCommand(new SettingUICommand(Settings));
        api.ClientManager.ConnectEvent += OnConnect;
        api.ClientManager.DisconnectEvent += OnDisconnect;
    }

    void OnConnect()
    {
        EventHooks.Initialize();
        Settings.CopyFrom(new());

        api.UiManager.ChatBox.AddMessage("BasicItemSync is in beta. Please report any bugs to the link on the mod page.");

        CurrencyRoutine = SyncPlugin.Instance.StartCoroutine(CurrencySender());
    }

    void OnDisconnect()
    {
        EventHooks.Uninitialize();
        if (CurrencyRoutine != null)
        {
            NetworkSender.SendPendingCurrency();
            SyncPlugin.Instance.StopCoroutine(CurrencyRoutine);
        }
    }

    public static void LocalChat(string message)
    {
        api.UiManager.ChatBox.AddMessage(message);
    }

    static IEnumerator CurrencySender()
    {
        while (true)
        {
            NetworkSender.SendPendingCurrency();

            yield return CurrencyTimer;
        }
    }
}
