using Silksong.AssetHelper.ManagedAssets;
using System.Collections;
using UnityEngine;

namespace BasicItemSync.Modules
{
    internal class Upgrader
    {
        static FakeCollectable? MaskCollectable;
        static FakeCollectable? SpoolCollectable;
        static FakeCollectable? PouchCollectable;
        static FakeCollectable? CraftingKitCollectable;
        static FakeCollectable? SilkHeartCollectable;
        static FakeCollectable? NeedleUpgrade;
        public static bool UpgradeMask(string sceneName)
        {
            var objName = "Heart Piece";
            if (sceneName == "Bone_East_LavaChallenge") objName += " (1)";

            PersistentHandler.SetPersistentBoolData(sceneName, objName, true, true);
            PlayerData.instance.heartPieces++;

            if (PlayerData.instance.heartPieces >= 4)
            {
                PlayerData.instance.heartPieces = 0;
                HeroController.instance.AddToMaxHealth(1);
                HeroController.instance.MaxHealth();
                EventRegister.SendEvent("MAX HP UP");
            }

            LoadAndDisplay(ref MaskCollectable, "Mask", "UI", "SHOP_SHELLFRAG_NAME");

            return Save();
        }

        public static bool UpgradeSpool(string sceneName) 
        {
            PersistentHandler.SetPersistentBoolData(sceneName, "Silk Spool", true, true);
            PlayerData.instance.silkSpoolParts++;

            if (PlayerData.instance.silkSpoolParts >= 2)
            {
                PlayerData.instance.silkSpoolParts = 0;
                HeroController.instance.AddToMaxSilk(1);
                EventRegister.SendEvent("SPOOL MAX UP");
                //EventRegister.SendEvent("SPOOL UNBROKEN");
            }

            LoadAndDisplay(ref SpoolCollectable, "Spool", "UI", "SHOP_SPOOL_SEGMENT_NAME");

            return Save();
        }

        public static bool GiveCollectable(string persistentScene, string persistentKey, string itemKey)
        {
            //var objName = "Collectable Item Pickup";

            //if (key == "Crest Socket Unlocker")
            //{
            //    if (sceneName == "Bone_10") objName += " Locket";
            //    else if (sceneName == "Coral_02") objName += " (1)";
            //    else if (sceneName == "Shadow_27") objName = "Sack Corpse Pickup";
            //}

            if (!string.IsNullOrEmpty(persistentScene) && !string.IsNullOrEmpty(persistentKey))
            {
                PersistentHandler.SetPersistentBoolData(persistentScene, persistentKey, true, true);
            }

            var collectable = CollectableItemManager.GetItemByName(itemKey);
            collectable.AddAmount(1);
            UI.ShowPopup(collectable);

            return Save();
        }

        public static bool UpgradePouch() 
        {
            PlayerData.instance.ToolPouchUpgrades++;

            LoadAndDisplay(ref PouchCollectable, "Pouch", "UI", "INV_NAME_TOOLPOUCH");

            return Save();
        }
        public static bool UpgradeCraftingKit() 
        {
            PlayerData.instance.ToolKitUpgrades++;

            LoadAndDisplay(ref CraftingKitCollectable, "CraftKit", "UI", "INV_MSG_TOOLKIT");

            return Save();
        }
        public static bool UpgradeSilkHeart()
        {
            PlayerData.instance.silkRegenMax++;

            LoadAndDisplay(ref SilkHeartCollectable, "SilkHeart", "UI", "INV_MSG_THREAD_HEART");

            return Save();
        }

        public static bool UpgradeNeedle()
        {
            PlayerData.instance.nailUpgrades++;

            LoadAndDisplay(ref NeedleUpgrade, "Needle", "UI", "INV_MSG_NEEDLE_UPGRADE");

            return Save();
        }
        static void LoadAndDisplay(ref FakeCollectable? assetRef, string dictKey, string langSheet, string langKey)
        {
            if (assetRef == null)
            {
                IEnumerator Coroutine(ManagedAsset<FakeCollectable> asset)
                {
                    if (asset == null) yield break;

                    asset.Load();
                    yield return asset.Handle;

                    if (asset.Handle.OperationException != null)
                    {
                        Debug.LogError($"Error loading asset: {asset.Handle.OperationException}");
                        yield break;
                    }

                    var result = asset.Handle.Result;

                    result.uiMsgName = new TeamCherry.Localization.LocalisedString
                    {
                        Sheet = langSheet,
                        Key = langKey
                    };

                    UI.ShowPopup(result);
                }

                if (!SyncPlugin.Collectables.TryGetValue(dictKey, out var loader)) return;
                
                if (!loader.IsLoaded)
                {
                    SyncPlugin.Instance.StartCoroutine(Coroutine(loader));
                    return;
                }
                else
                {
                    assetRef = loader.Handle.Result;                
                }
            }

            UI.ShowPopup(assetRef);
        }

        public static bool Save()
        {
            if (!HeroController.SilentInstance)
            {
                Log.LogError($"[CLI: SAVE] Not in save file. Cannot save data.");
                return false;
            }

            GameManager.instance.SaveGame((a) => { });
            return true;
        }

        public static bool NoOp()
        {
            return false;
        }
    }
}
