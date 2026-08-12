using BasicItemSync.Modules.Network.Client;
using BasicItemSync.Modules.Network.Server;
using BepInEx;
using HarmonyLib;
using Silksong.AssetHelper.ManagedAssets;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BasicItemSync
{
    [BepInDependency("ssmp")]
    [BepInDependency("org.silksong-modding.assethelper")]
    [BepInAutoPlugin(id: "io.github.bobbythecatfish.basicitemsync")]
    public partial class SyncPlugin : BaseUnityPlugin
    {
        private static List<Action> CurrentFrameActions = [];
        private static List<Action> NextFrameActions = [];

        internal static SyncPlugin Instance;
        internal static Dictionary<string, ManagedAsset<FakeCollectable>> Collectables = [];

        private void Awake()
        {
            // Put your initialization logic here
            Instance = this;

            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
            SSMP.Api.Client.ClientAddon.RegisterAddon(new ClientAddon());
            SSMP.Api.Server.ServerAddon.RegisterAddon(new ServerAddon());

            const string bundle = "dataassets_assets_assets/dataassets/collectables/fakecollectables";
            const string dir = "Assets/Data Assets/Collectables/Fake Collectables/";
            Collectables["Mask"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Heart Piece.asset", bundle);
            Collectables["Spool"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Silk Spool.asset", bundle);
            Collectables["Pouch"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Tool Pouch Pickup.asset", bundle);
            Collectables["CraftKit"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Tool Kit Pickup.asset", bundle);
            Collectables["SilkHeart"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Silk Heart.asset", bundle);
            Collectables["Needle"] = ManagedAsset<FakeCollectable>.FromNonSceneAsset(dir + "Needle Upgrade.asset", bundle);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void AddNextFrameAction(Action action)
        {
            NextFrameActions.Add(action);
        }

        void Update()
        {
            if (CurrentFrameActions.Count > 0)
            {
                var actions = CurrentFrameActions.ToArray();
                CurrentFrameActions.Clear();
                foreach (var action in actions)
                {
                    action.Invoke();
                }
            }

#if DEBUG
            //if (Input.GetKeyDown(KeyCode.Alpha0))
            //{
            //    UnityExplorer.InspectorManager.Inspect(PlayerData.instance);
            //    UnityExplorer.UI.UIManager.GetPanel(UnityExplorer.UI.UIManager.Panels.ObjectExplorer).SetActive(false);
            //    UnityExplorer.UI.UIManager.GetPanel(UnityExplorer.UI.UIManager.Panels.Clipboard).SetActive(false);
            //    UnityExplorer.UI.UIManager.GetPanel(UnityExplorer.UI.UIManager.Panels.ConsoleLog).SetActive(false);
            //}
#endif
        }

        void LateUpdate()
        {
            if (NextFrameActions.Count > 0)
            {
                CurrentFrameActions = [.. NextFrameActions];
                NextFrameActions.Clear();
            }
        }
    }
}
