using UnityEngine.SceneManagement;

namespace BasicItemSync.Modules
{
    internal class PersistentHandler
    {
        public static void SetPersistentIntData(string sceneName, string keyName, int status, FlagType flagType)
        {
            var value = new PersistentItemData<int>
            {
                SceneName = sceneName,
                ID = keyName,
                IsSemiPersistent = false,
                Value = status
            };

            if (SceneData.instance.PersistentInts.TryGetValue(sceneName, keyName, out var persistent))
            {
                persistent.Value = status;
            }
            else
            {
                SceneData.instance.PersistentInts.SetValue(value);
            }

            var scene = SceneManager.GetActiveScene().name;
            if (flagType == FlagType.Currency && scene != sceneName) return;
            
            var rawItem = ObjectHelper.FindPersistent<int>(sceneName, keyName);
            if (rawItem == null || rawItem is not PersistentIntItem item) return;

            item.ItemData.Value = status;
            item.Start();
        }

        public static void SetPersistentBoolData(string sceneName, string keyName, bool status, bool forceDisable = false)
        {
            var value = new PersistentItemData<bool>
            {
                SceneName = sceneName,
                ID = keyName,
                IsSemiPersistent = false,
                Value = status
            };

            //Log.LogInfo(sceneName, keyName, status);

            if (SceneData.instance.PersistentBools.TryGetValue(sceneName, keyName, out var persistent))
            {
                persistent.Value = status;
            }
            else
            {
                SceneData.instance.PersistentBools.SetValue(value);
            }

            var rawItem = ObjectHelper.FindPersistent<bool>(sceneName, keyName);
            if (rawItem == null || rawItem is not PersistentBoolItem item) return;

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

            if (item.disablePrefabIfActivated) item.disablePrefabIfActivated.SetActive(false);
            if (item.disableIfActivated || forceDisable)
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}
