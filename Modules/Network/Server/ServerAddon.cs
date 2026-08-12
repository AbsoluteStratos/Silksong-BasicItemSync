using SSMP.Api.Server;

namespace BasicItemSync.Modules.Network.Server;

internal class ServerAddon : SSMP.Api.Server.ServerAddon
{
    public override bool NeedsNetwork => true;
    public override uint ApiVersion => Common.AddonApiVersion;
    protected override string Name => Common.AddonName;
    protected override string Version => Common.AddonVersion;

    public static IServerApi api;

    public static ServerAddon Instance;

    public static SyncServerSettings Settings;

    public override void Initialize(IServerApi serverApi)
    {
        Log.SetLogger(Logger);
        Log.LogInfo("Item Sync Server Addon enabled");
        Settings = SyncServerSettings.ReadFromFile();
        Instance = this;
        api = serverApi;

        NetworkForwarder.Initialize();
        api.ServerManager.PlayerConnectEvent += OnPlayerConnect;
    }

    void OnPlayerConnect(IServerPlayer player)
    {
        NetworkForwarder.SendSettingsUpdate(player.Id);
    }
}
