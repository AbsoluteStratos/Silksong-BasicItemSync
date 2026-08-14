using TeamCherry.Localization;
using UnityEngine;

namespace BasicItemSync.Modules
{
    internal class UI
    {
        public static void ShowPopup(ICollectableUIMsgItem item)
        {
            CollectableUIMsg.Spawn(item, Color.white, null, false);
        }

        public static void ShowPopup(FlagType flagType, string key, string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var icon = flagType switch
            {
                FlagType.Ability => GetAbilityIcon(key),
                FlagType.Map => GetMap(name),
                FlagType.Pin => GetPin(key, name),
                // TODO: Finish
                _ => null
            };

            if (icon != null) ShowPopup(icon);
        }

        static PopupItem? GetAbilityIcon(string key)
        {
            var inv = GetInventory();
            var layout = inv.GetChild(1).GetChild(0).GetChild(1).GetChild(2).GetChild(2);

            PopupItem GetIcon(int index, string text)
            {
                var renderer = layout.GetChild(index).GetComponent<SpriteRenderer>();
                return new PopupItem(text, renderer.sprite);
            }

            static PopupItem? GetCloakIcon(int state)
            {
                var d = CollectableItemManager.Instance.masterList.GetByName("Dresses");
                if (d is not CollectableItemStates dresses) return null;

                var dress = dresses.states[state];
                return new PopupItem(Language.GetLocal(dress.DisplayName), dress.Icon);
            }

            PopupItem GetNeedleStrike()
            {
                var renderer = inv.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
                return new PopupItem("Needle Strike", renderer.sprite);
            }

            return key switch
            {
                nameof(PlayerData.hasDash) => GetIcon(0, "Swift Step"),
                nameof(PlayerData.hasHarpoonDash) => GetIcon(1, "Clawline"),
                nameof(PlayerData.HasBoundCrestUpgrader) => GetIcon(2, "Sylphsong"),
                nameof(PlayerData.hasSuperJump) => GetIcon(3, "Sillk Soar"),
                nameof(PlayerData.hasWalljump) => GetIcon(4, "Cling Grip"),
                nameof(PlayerData.hasNeedolin) => GetIcon(5, "Needolin"),
                nameof(PlayerData.UnlockedFastTravelTeleport) => GetIcon(5, "Beastling Call"),
                nameof(PlayerData.hasBrolly) => GetCloakIcon(1),
                nameof(PlayerData.hasDoubleJump) => GetCloakIcon(2),
                nameof(PlayerData.hasChargeSlash) => GetNeedleStrike(),
                _ => null
            };
        }

        static PopupItem GetMap(string name)
        {
            var inv = GetInventory();
            var map = inv.GetChild(2).GetComponent<InventoryPane>();
            return new PopupItem(name + " Map", map.ListIcon);
        }

        static PopupItem? GetPin(string key, string name)
        {
            var inv = GetInventory();
            var map = inv.GetChild(2);

            PopupItem GetSpecialPin(int index)
            {
                var key = map.GetChild(3).GetChild(0).GetChild(index);
                var pin = key.GetComponentInChildren<SpriteRenderer>(true);
                
                return new PopupItem(name, pin.sprite);
            }

            PopupItem GetMarkerPin(int index)
            {
                var marker = map.GetChild(1).GetChild(5).GetChild(6).GetChild(index);
                var pin = marker.GetComponentInChildren<SpriteRenderer>(true);

                return new PopupItem(name, pin.sprite);
            }

            return key switch
            {
                nameof(PlayerData.hasPinBench) => GetSpecialPin(0),
                nameof(PlayerData.hasPinCocoon) => GetSpecialPin(1),
                nameof(PlayerData.hasPinShop) => GetSpecialPin(4),
                nameof(PlayerData.hasPinSpa) => GetSpecialPin(1),
                nameof(PlayerData.hasPinStag) => GetSpecialPin(1),
                nameof(PlayerData.hasPinTube) => GetSpecialPin(5),
                nameof(PlayerData.hasPinFleaMarrowlands) => GetSpecialPin(3),
                nameof(PlayerData.hasPinFleaMidlands) => GetSpecialPin(3),
                nameof(PlayerData.hasPinFleaBlastedlands) => GetSpecialPin(3),
                nameof(PlayerData.hasPinFleaCitadel) => GetSpecialPin(3),
                nameof(PlayerData.hasPinFleaPeaklands) => GetSpecialPin(3),
                nameof(PlayerData.hasPinFleaMucklands) => GetSpecialPin(3),
                nameof(PlayerData.hasMarker_a) => GetMarkerPin(0),
                nameof(PlayerData.hasMarker_b) => GetMarkerPin(1),
                nameof(PlayerData.hasMarker_c) => GetMarkerPin(2),
                nameof(PlayerData.hasMarker_d) => GetMarkerPin(3),
                nameof(PlayerData.hasMarker_e) => GetMarkerPin(4),
                _ => null
            };
        }



        static Transform GetInventory()
        {
            var cam = GameCameras.instance.hudCamera.gameObject;
            return cam.transform.GetChild(0).GetChild(4);
        }
    }

    internal class PopupItem : ICollectableUIMsgItem
    {
        public string Name;
        public Sprite Sprite;
        public Object Object;

        public PopupItem(FullQuestBase quest)
        {
            Object = quest;
            Name = Language.Get(quest.DisplayName.Key, quest.DisplayName.Sheet);
            Sprite = quest.QuestType.Icon;
        }

        public PopupItem(string name, Sprite sprite)
        {
            Name = name;
            Sprite = sprite;
            Object = sprite;
        }

        public Object GetRepresentingObject() => Object;

        public float GetUIMsgIconScale() => 1;

        public string GetUIMsgName() => Name;

        public Sprite GetUIMsgSprite() => Sprite;

        public bool HasUpgradeIcon() => false;
    }
}
