
using BasicItemSync.Data;
using BasicItemSync.Modules.Network.Client;
using GenericVariableExtension;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules
{
    internal class BattleManager
    {
        static BattleScene? GetBattleScene(string keyName)
        {
            var scene = SceneManager.GetActiveScene();
            var objs = scene.GetRootGameObjects();
            foreach (var obj in objs)
            {
                var scenes = obj.GetComponentsInChildren<BattleScene>();
                foreach (var battle in scenes)
                {
                    if (battle.TryGetComponent<DeactivateIfPlayerdataTrue>(out var deactivator))
                    {
                        if (deactivator.boolName == keyName) return battle;
                    }
                }
            }

            return null;
        }

        public static void DefeatBattleScene(string keyName)
        {
            if (keyName == nameof(PlayerData.defeatedBellBeast))
            {
                OnBellBeast();
                return;
            }
            var battle = GetBattleScene(keyName);
            if (battle) DefeatBattleScene(battle);
        }

        public static void DefeatBattleScene(BattleScene battle)
        {
            if (battle.started)
            {
                var wave = battle.currentWave;
                battle.DoEndBattle();

                foreach (var health in battle.waves[wave].gameObject.GetComponentsInChildren<HealthManager>())
                {
                    if (health.isDead) continue;

                    health.Die(0, AttackTypes.Nail, true);
                }
            }
            else
            {
                battle.BattleCompleted();
                battle.completed = true;

                // Do a bunch of logic from DoEndBattle
                if (!string.IsNullOrEmpty(battle.setPDBoolOnEnd)) PlayerData.instance.SetVariable(battle.setPDBoolOnEnd, true);
                if (!string.IsNullOrEmpty(battle.setExtraPDBoolOnEnd)) PlayerData.instance.SetVariable(battle.setExtraPDBoolOnEnd, true);
                if (battle.camLocks && !battle.dontDisableCamlocksOnEnd) battle.camLocks.SetActive(false);
                if (battle.openGatesOnEnd) battle.SendEventToChildren("BG OPEN");
                if (battle.endScene) battle.SendEventToChildren("BATTLE END");
                if (battle.activeDuringBattle) battle.activeDuringBattle.SetActive(false);
                if (battle.disableActiveBeforeBattleAtEnd) battle.activeBeforeBattle.SetActive(false);
                if (!string.IsNullOrEmpty(battle.battleEndEventRegister)) EventRegister.SendEvent(battle.battleEndEventRegister, null);
                //battle.gameObject.SetActive(false);
            }
        }

        static void OnBellBeast()
        {
            SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
            {
                ID = ItemNames.SilkHeart,
                SceneName = "Bone_05",
                IsSemiPersistent = false,
                Value = false,
                Mutator = SceneData.PersistentMutatorTypes.None
            });

            var scene = SceneManager.GetActiveScene();
            var doHearts = ClientAddon.Settings.FlagAllowed(FlagType.SilkHeart);

            if (scene.name == "Bone_05")
            {
                var bossScene = SceneManager.GetSceneByName("Bone_05_boss");
                if (bossScene.IsValid())
                {
                    foreach (var obj in bossScene.GetRootGameObjects())
                    {
                        obj.SetActive(false);
                    }
                }

                if (!doHearts)
                {
                    var boss = bossScene.GetRootGameObjects()[0];
                    for (var i = 0; i < boss.transform.childCount; i++)
                    {
                        var obj = boss.transform.GetChild(i);
                        if (obj.name == "Silk Heart")
                        {
                            obj.gameObject.SetActive(true);
                            return;
                        }
                    }
                }
            }
            //else if (scene.name == "Memory_Silk_Heart_BellBeast" && doHearts)
            //{
            //    var loadInfo = new GameManager.SceneLoadInfo
            //    {
            //        SceneName = "Bone_05",
            //        EntryGateName = "bot1",
            //        PreventCameraFadeOut = true,
            //        WaitForSceneTransitionCameraFade = false,
            //        Visualization = GameManager.SceneLoadVisualizations.Default,
            //        AlwaysUnloadUnusedAssets = true,
            //        IsFirstLevelForPlayer = false
            //    };

            //    HeroController.instance.SetSilkRegenBlocked(false);
            //    PlayerData.instance.disableInventory = false;
            //    ToolItemManager.SetIsInCutscene(false);

            //    CameraBlurPlane.Spacing = 0;
            //    CameraBlurPlane.Vibrancy = 0;
            //    CameraBlurPlane.MaskLerp = 0;

            //    GameManager.instance.BeginSceneTransition(loadInfo);
            //}
        }
    }
}
