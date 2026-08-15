using BasicItemSync.Data;
using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using TeamCherry.Localization;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(QuestManager))]
internal static class QuestHook
{
    [HarmonyPatch(nameof(QuestManager.ShowQuestCompleted))]
    [HarmonyPrefix]
    static void ShowQuestCompleted(FullQuestBase quest)
    {
        var key = quest.name;
        if (ClientState.WasItemReceived(key)) return;

        if (quest.RewardItem)
        {
            var item = quest.RewardItem;
            if (quest.RewardItem.name == ItemNames.MaskShard) { }
            else if (quest.RewardItem.name == ItemNames.SpoolShard) { }
            else if (item.name == ItemNames.ToolPouch) NetworkSender.SendUpgrade("", FlagType.Pouch);
            else if (item.name == ItemNames.CraftingKit) NetworkSender.SendUpgrade("", FlagType.CraftingKit);
            else if (item.name == ItemNames.NeedleUpgrade) NetworkSender.SendUpgrade("", FlagType.Needle);
            else NetworkSender.SendCollectable(item.name, item.GetPopupName(), quest.RewardCount, FlagType.Collectable);
        }

        var displayName = Language.GetLocal(quest.displayName);
        NetworkSender.SendQuestComplete(quest.name, displayName);
    }
}