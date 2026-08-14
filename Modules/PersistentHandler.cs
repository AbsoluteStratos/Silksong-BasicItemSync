using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules
{
    internal class PersistentHandler
    {
        public static void SetPersistentIntData(string sceneName, string id, int status, FlagType flagType)
        {

            if (SceneData.instance.PersistentInts.TryGetValue(sceneName, id, out var persistent))
            {
                persistent.Value = status;
            }
            else
            {
                var value = new PersistentItemData<int>
                {
                    SceneName = sceneName,
                    ID = id,
                    IsSemiPersistent = false,
                    Value = status
                };
                SceneData.instance.PersistentInts.SetValue(value);
            }

            if (flagType == FlagType.Currency && SceneManager.GetActiveScene().name != sceneName) return;
            
            var rawItem = ObjectHelper.FindPersistent<int>(sceneName, id);
            if (rawItem == null || rawItem is not PersistentIntItem item) return;

            item.ItemData.Value = status;
            item.Start();

            // Needle hitbox already does this for us
            //if (item.TryGetComponent<HitSlidePlatform>(out var plat))
            //{
            //    plat.SetAtNode(plat.nodes[status]);
            //}
        }

        public static void SetPersistentBoolData(string sceneName, string id, bool status, bool forceDisable = false)
        {

            if (SceneData.instance.PersistentBools.TryGetValue(sceneName, id, out var persistent))
            {
                persistent.Value = status;
            }
            else
            {
                var value = new PersistentItemData<bool>
                {
                    SceneName = sceneName,
                    ID = id,
                    IsSemiPersistent = false,
                    Value = status
                };
                SceneData.instance.PersistentBools.SetValue(value);
            }

            var rawItem = ObjectHelper.FindPersistent<bool>(sceneName, id);
            if (rawItem == null || rawItem is not PersistentBoolItem item)
            {
                if (sceneName == SceneManager.GetActiveScene().name) Log.LogWarning($"Unable to find persistent {sceneName} {id}");
                return;
            }

            item.ItemData.Value = status;

            if (item.TryGetComponent<Lever>(out var lever)) // activate levers
            {
                var trigger = lever.canHitTrigger;
                lever.canHitTrigger = null;
                lever.Hit(new HitInstance
                {
                    IsFirstHit = true,
                    IsNailTag = true,
                    Direction = 0,
                });
                lever.canHitTrigger = trigger;
            }
            else if (item.TryGetComponent<Lever_tk2d>(out var tkLever))
            {
                var trigger = tkLever.canHitTrigger;
                tkLever.canHitTrigger = null;
                tkLever.Hit(new HitInstance
                {
                    IsFirstHit = true,
                    IsNailTag = true,
                    Direction = 0,
                });
                tkLever.canHitTrigger = trigger;
            }
            else if (item.fsm)
            {
                item.SetValueOnFSM(item.fsm, status);
                item.fsm.SendEvent("QUICK BREAK");
                item.fsm.SendEvent("ACTIVATE");
                item.fsm.SendEvent("DESTROY");
            }
            else if (item.TryGetComponent<PersistentBoolItemResponder>(out var responder))
            {
                responder.InvokeEvents(status);
            }
            else if (item.TryGetComponent<CollectableItemPickup>(out var _))
            {
                forceDisable = true;
            }
            else if (item.TryGetComponent<BattleScene>(out var scene))
            {
                BattleManager.DefeatBattleScene(scene);
            }

            if (item.disablePrefabIfActivated) item.disablePrefabIfActivated.SetActive(false);
            if (item.disableIfActivated || forceDisable)
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}
