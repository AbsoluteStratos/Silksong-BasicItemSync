namespace BasicItemSync.Modules;

internal class QuestHandler
{
    public static void EndQuest(string name)
    {
        var quest = QuestManager.GetQuest(name);
        if (!quest) return;

        quest.SilentlyComplete();
        quest.ConsumeTarget();
        
        UI.ShowPopup(new QuestPopupItem(quest));
    }
    public static void EndQuestSilent(string name)
    {
        var quest = QuestManager.GetQuest(name);
        if (!quest) return;

        quest.SilentlyComplete();
    }
}
