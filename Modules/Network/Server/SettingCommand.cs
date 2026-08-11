using GenericVariableExtension;
using SSMP.Api.Command.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicItemSync.Modules.Network.Server
{
    internal class SettingCommand : IServerCommand
    {
        public SettingCommand(SyncServerSettings settings)
        {
            Settings = settings;
        }

        readonly SyncServerSettings Settings;

        public bool AuthorizedOnly => true;

        public string Trigger => "/sync";

        public string[] Aliases => [];

        public void Execute(ICommandSender commandSender, string[] arguments)
        {
            void SendUsage()
            {
                commandSender.SendMessage($"Invalid usage: {Trigger} <setting> <true/false>");
            }

            if (arguments.Length < 2)
            {
                SendUsage();
                return;
            }

            var settingName = "Sync" + arguments[1];
            var settings = typeof(SyncServerSettings).GetAllFields(System.Reflection.BindingFlags.Public);
            var setting = settings.FirstOrDefault(s => s.Name.Equals(settingName, StringComparison.CurrentCultureIgnoreCase));
            if (setting == null)
            {
                commandSender.SendMessage($"Unknown setting '{settingName}");
                return;
            }

            if (arguments.Length == 2)
            {
                var value = Settings.GetVariable<bool>(setting.Name);
                commandSender.SendMessage($"Setting '{setting.Name}' is currently {value}");
            }
            else
            {
                bool value;
                if (arguments[2] == "true") value = true;
                else if (arguments[2] == "false") value = false;
                else
                {
                    SendUsage();
                    return;
                }
                Settings.SetVariable(setting.Name, value);
                NetworkForwarder.SendSettingsUpdate();
            }
        }
    }
}
