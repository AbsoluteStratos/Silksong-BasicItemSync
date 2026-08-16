using GenericVariableExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BasicItemSync.Modules.Network.Server
{
    internal class SyncServerSettings
    {
        public bool KillSwitch = false;
        public bool SyncAbilities = true;
        public bool SyncMaps = true;
        public bool SyncPins = true;
        public bool SyncCurrency = true;
        public bool SyncSpendingCurrency = false;
        public bool SyncQuests = true;
        public bool SyncQuestItems = false;
        public bool SyncBosses = true;
        public bool SyncArenas = true;
        public bool SyncProgression = true;
        public bool SyncFleas = true;
        public bool SyncCollectables = true;
        public bool SyncUpgrades = true;
        public bool SyncTools = true;
        public bool SyncCrests = true;
        public bool SyncTransit = true;
        public bool SyncShortcuts = true;

        public bool FlagAllowed(FlagType flag)
        {
            if (ServerAddon.Settings.KillSwitch) return false;

            return flag switch
            {
                FlagType.Ability => SyncAbilities,
                FlagType.Map => SyncMaps,
                FlagType.Pin => SyncPins,
                FlagType.Bellshrine => SyncProgression,
                FlagType.Boss => SyncBosses,
                FlagType.Arena => SyncArenas,
                FlagType.Progression => SyncProgression,
                FlagType.Flea => SyncFleas,
                FlagType.Collectable => SyncCollectables,
                FlagType.Bellway => SyncTransit,
                FlagType.Ventrica => SyncTransit,
                FlagType.Currency => SyncCurrency,

                FlagType.Mask => SyncUpgrades,
                FlagType.Spool => SyncUpgrades,
                FlagType.Pouch => SyncUpgrades,
                FlagType.CraftingKit => SyncUpgrades,
                FlagType.Quest => SyncQuests,
                FlagType.QuestStart => SyncQuestItems,
                FlagType.QuestItem => SyncQuestItems,
                FlagType.Tool => SyncTools,
                FlagType.Crest => SyncCrests,
                FlagType.SilkHeart => SyncUpgrades,
                FlagType.Needle => SyncUpgrades,
                FlagType.Bench => SyncProgression,
                FlagType.Shortcut => SyncShortcuts,
                FlagType.DoNotSync => false,
                _ => false,
            };
        }

        public List<bool> ToValues()
        {
            var props = GetProperties();
            var output = new List<bool>();

            foreach (var prop in props)
            {
                output.Add(this.GetVariable<bool>(prop.Name));
            }

            return output;
        }

        public static List<FieldInfo> GetProperties()
        {
            var props = typeof(SyncServerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();
            props.Sort((a, b) => a.Name.CompareTo(b.Name));

            return props;
        }

        public void CopyFrom(SyncServerSettings settings)
        {
            var props = GetProperties();
            foreach (var prop in props)
            {
                this.SetVariable(prop.Name, settings.GetVariable<bool>(prop.Name));
            }
        }

        public static SyncServerSettings PopulateFromValues(List<bool> values)
        {
            var instance = new SyncServerSettings();
            var props = GetProperties();

            if (props.Count != values.Count)
            {
                Log.LogWarning("Mismatched server setting property count");
            }

            for (var i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                var value = values[i];
                instance.SetVariable(prop.Name, value);
            }

            return instance;
        }

        public static SyncServerSettings ReadFromFile()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "ServerSettings.json");

            if (!File.Exists(path))
            {
                Log.LogWarning($"[SERVER: SETTINGS] {path} doesn't exist");
                var instance = new SyncServerSettings();
                instance.SaveToFile();
                return instance;
            }

            try
            {
                var fileContents = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<SyncServerSettings>(fileContents);
                return settings ?? new SyncServerSettings();
            }
            catch (Exception e)
            {
                Log.LogError($"[SERVER: SETTINGS] Could not load server settings from file:\n{e}");
                return new SyncServerSettings();
            }
        }

        public void SaveToFile()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "ServerSettings.json");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Log.LogWarning($"[SERVER: SETTINGS] {path} directory doesn't exist");
                return;
            }

            var settings = JsonConvert.SerializeObject(this, Formatting.Indented);
            if (settings == null) return;

            try
            {
                File.WriteAllText(path, settings);
            }
            catch (Exception e)
            {
                Log.LogError($"[SERVER: SETTINGS] Could not write server settings to file:\n{e}");
            }
        }
    }
}
