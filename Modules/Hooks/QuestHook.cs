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

        if (quest.RewardItem && !ClientAddon.Settings.FlagAllowed(FlagType.Quest))
        {
            //if (quest.RewardItem.name == "Heart Piece") NetworkSender.SendUpgrade("", FlagType.Mask);
            //else if (quest.RewardItem.name == "Silk Spool") NetworkSender.SendUpgrade("", FlagType.Spool);
            if (quest.RewardItem.name == "Tool Pouch Pickup") NetworkSender.SendUpgrade("", FlagType.Pouch);
            else if (quest.RewardItem.name == "Took Kit Pickup") NetworkSender.SendUpgrade("", FlagType.CraftingKit);
            else if (quest.RewardItem.name == "Needle Upgrade") NetworkSender.SendUpgrade("", FlagType.Needle);
        }

        var displayName = Language.GetLocal(quest.displayName);
        NetworkSender.SendQuestComplete(quest.name, displayName);
    }
}