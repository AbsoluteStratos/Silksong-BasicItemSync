using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicItemSync.Modules
{
    internal static class ModSettings
    {
        public static bool DebugPlayerData => _debugPlayerData?.Value ?? false;
        static ConfigEntry<bool> _debugPlayerData;

        public static bool DebugLogs => _debugLogs?.Value ?? false;
        static ConfigEntry<bool> _debugLogs;

        public static void Init(ConfigFile config)
        {
            _debugLogs = config.Bind("Debug", "Enable debug logs", true);
            _debugPlayerData = config.Bind("Debug", "Enable PlayerData logs", false);
        }
    }
}
