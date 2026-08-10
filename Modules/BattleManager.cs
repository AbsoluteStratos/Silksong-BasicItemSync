
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
            if (!battle) return;

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
                battle.gameObject.SetActive(false);
            }
        }

        static void OnBellBeast()
        {
            var scene = SceneManager.GetActiveScene();
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
            }
            else if (scene.name == "Memory_Silk_Heart_BellBeast")
            {
                var loadInfo = new GameManager.SceneLoadInfo
                {
                    SceneName = "Bone_05",
                    EntryGateName = "bot1",
                    PreventCameraFadeOut = true,
                    WaitForSceneTransitionCameraFade = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = true,
                    IsFirstLevelForPlayer = false
                };

                HeroController.instance.SetSilkRegenBlocked(false);
                PlayerData.instance.disableInventory = false;
                ToolItemManager.SetIsInCutscene(false);

                GameManager.instance.BeginSceneTransition(loadInfo);
            }
        }
    }
}
