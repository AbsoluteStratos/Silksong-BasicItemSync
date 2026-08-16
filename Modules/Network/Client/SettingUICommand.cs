using BasicItemSync.Modules.Network.Server;
using SSMP.Api.Command.Client;
using UnityEngine;

namespace BasicItemSync.Modules.Network.Client
{
    internal class SettingUICommand : IClientCommand
    {
        readonly SyncServerSettings Settings;
        static SettingsUI? UI;
        public SettingUICommand(SyncServerSettings settings)
        {
            Settings = settings;
        }

        public string Trigger => "/sync-ui";

        public string[] Aliases => [];

        public void Execute(string[] arguments)
        {
            if (UI != null) return;

            var uiObj = new GameObject("Sync Settings UI");
            UI = uiObj.AddComponent<SettingsUI>();
            UI.Settings = Settings;
            UI.SetProps();
            UI.ShowSettings = true;
        }
    }
}
