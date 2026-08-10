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
    }

    internal class QuestPopupItem : ICollectableUIMsgItem
    {
        public FullQuestBase Quest;

        public QuestPopupItem(FullQuestBase quest)
        {
            Quest = quest;
        }

        public Object GetRepresentingObject()
        {
            return Quest;
        }

        public float GetUIMsgIconScale()
        {
            return 1;
        }

        public string GetUIMsgName()
        {
            return Language.Get(Quest.DisplayName.Key, Quest.DisplayName.Sheet);
        }

        public Sprite GetUIMsgSprite()
        {
            return Quest.QuestType.Icon;
        }

        public bool HasUpgradeIcon() => false;
    }
}
