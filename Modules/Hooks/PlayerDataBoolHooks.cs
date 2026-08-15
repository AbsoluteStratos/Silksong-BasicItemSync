using BasicItemSync.Data;
using BasicItemSync.Modules.Network.Client;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Reflection;


namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(PlayerData))]
internal static class PlayerDataHook
{
    [HarmonyPatch(nameof(PlayerData.SetBool))]
    [HarmonyPrefix]
    static void SetBool(string boolName, bool value)
    {
        BoolUpdated(boolName, value);
    }

    [HarmonyPatch(nameof(PlayerData.SetInt))]
    [HarmonyPrefix]
    static void SetInt(string intName, int value)
    {
        IntUpdated(intName, value);
    }


    public static void BoolUpdated(string boolName, bool value, PlayerDataBoolOperation.Operation operation = PlayerDataBoolOperation.Operation.Set)
    {
        if (string.IsNullOrEmpty(boolName)) return;
        Log.LogDebug($"[CLI: PD BOOL] {boolName}");

        if (ItemNames.BoolKeys.TryGetValue(boolName, out var key))
        {
            if (ClientState.WasItemReceived(boolName)) return;
            var boolValue = value;

            var existing = PlayerData.instance.GetBool(boolName);
            if (operation == PlayerDataBoolOperation.Operation.Set && value == existing) return;
            if (operation == PlayerDataBoolOperation.Operation.Flip) boolValue = !existing;

            NetworkSender.SendFlag(boolName, key.Type, key.Name, boolValue);
        }
    }
    public static void IntUpdated(string intName, int value, PlayerDataIntOperation.Operation operation = PlayerDataIntOperation.Operation.Set)
    {
        if (string.IsNullOrEmpty(intName)) return;

        Log.LogDebug($"[CLI: PD INT] {intName}");
        if (ItemNames.IntKeys.TryGetValue(intName, out var key))
        {
            var existing = PlayerData.instance.GetInt(intName);
            var newValue = existing;

            if (operation == PlayerDataIntOperation.Operation.Set && value == existing) return;
            
            else if (operation == PlayerDataIntOperation.Operation.Add) newValue += value;
            else if (operation == PlayerDataIntOperation.Operation.Subtract) newValue -= value;
            else if (operation == PlayerDataIntOperation.Operation.Multiply) newValue *= value;
            else return;

            NetworkSender.SendInt(intName, key.Type, key.Name, newValue);
        }
    }
}


[HarmonyPatch(typeof(SetPlayerDataBool))]
internal static class SetPlayerDataBoolHook
{
    [HarmonyPatch(nameof(SetPlayerDataBool.SetBool))]
    [HarmonyPrefix]
    public static void SetBool(SetPlayerDataBool __instance)
    {
        var boolName = __instance.boolName;
        var value = __instance.value;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(SetPlayerDataVariable))]
internal static class SetPlayerDataVariableHook
{
    [HarmonyPatch(nameof(SetPlayerDataVariable.OnEnter))]
    [HarmonyPrefix]
    public static void OnEnter(SetPlayerDataVariable __instance)
    {
        var varName = __instance.VariableName.Value;
        var value = __instance.SetValue.GetValue();

        if (value is bool bValue)
        {
            PlayerDataHook.BoolUpdated(varName, bValue);
        }
        else if (value is int iValue)
        {
            PlayerDataHook.IntUpdated(varName, iValue);
        }
    }
}


[HarmonyPatch(typeof(StateChangeSequence))]
internal static class StateChangeSequenceHook
{
    [HarmonyPatch(nameof(StateChangeSequence.SetIsCompleteBool))]
    [HarmonyPrefix]
    public static void OnEnter(StateChangeSequence __instance)
    {
        var boolName = __instance.isCompleteBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(PlayerDataCollectable))]
internal static class PlayerDataCollectableHook
{
    [HarmonyPatch(nameof(PlayerDataCollectable.Get))]
    [HarmonyPrefix]
    public static void Get(PlayerDataCollectable __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.linkedPDBool, true);
        PlayerDataHook.IntUpdated(__instance.linkedPDInt, 1, PlayerDataIntOperation.Operation.Add);
    }
}


[HarmonyPatch(typeof(PlayerDataBoolCollectable))]
internal static class PlayerDataBoolCollectableHook
{
    [HarmonyPatch(nameof(PlayerDataBoolCollectable.Get))]
    [HarmonyPostfix]
    public static void UnlockSequence(PlayerDataBoolCollectable __instance)
    {
        var boolName = __instance.boolName;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(CollectableItemBasic))]
internal static class CollectableItemBasicHook
{
    [HarmonyPatch(nameof(CollectableItemBasic.SetUniqueBool))]
    [HarmonyPrefix]
    static void SetUniqueBool(CollectableItemBasic __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.uniqueCollectBool, true);
    }
}


[HarmonyPatch(typeof(CaravanTroupeHunter))]
internal static class CaravanTroupeHunterHook
{
    [HarmonyPatch(nameof(CaravanTroupeHunter.OnPurchasedItem))]
    [HarmonyPrefix]
    public static void OnPurchasedItem(CaravanTroupeHunter __instance, int itemIndex)
    {

        var boolName = CaravanTroupeHunter.PdBools[__instance.currentListGroups[itemIndex]];
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(ControlReminder.ConfigBase))]
internal static class ControlReminderHook
{
    [HarmonyPatch(nameof(ControlReminder.ConfigBase.DoAppear))]
    [HarmonyPrefix]
    public static void DoAppear(ControlReminder.ConfigBase __instance)
    {
        var boolName = __instance.PlayerDataBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(CurrencyObjectBase))]
internal static class CurrencyObjectBaseHook
{
    [HarmonyPatch(nameof(CurrencyObjectBase.CollectPopup))]
    [HarmonyPrefix]
    static void CollectPopup(CurrencyObjectBase __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.firstGetPDBool, true);
        PlayerDataHook.BoolUpdated(__instance.popupPDBool, true);
    }
}


[HarmonyPatch]
internal static class GameManagerHook
{
    static MethodInfo TargetMethod()
    {
        var method = typeof(GameManager).GetMethod(nameof(GameManager.SetPlayerDataVariable), BindingFlags.Public | BindingFlags.Instance);
        return method.MakeGenericMethod(typeof(bool));
    }

    static void Prefix(string fieldName, bool value)
    {
        PlayerDataHook.BoolUpdated(fieldName, value);
    }
}


[HarmonyPatch(typeof(ItemReceptacle))]
internal static class ItemReceptacleHook
{
    [HarmonyPatch(nameof(ItemReceptacle.UnlockSequence))]
    [HarmonyPrefix]
    public static void UnlockSequence(ItemReceptacle __instance)
    {
        var boolName = __instance.playerDataBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
        PersistentBoolItemHook.UpdateValue(__instance.persistent, true);
    }
}


[HarmonyPatch(typeof(PersistentPressurePlate))]
internal static class PersistentPressurePlateHook
{
    [HarmonyPatch(nameof(PersistentPressurePlate.Activate))]
    [HarmonyPrefix]
    public static void Activate(PersistentPressurePlate __instance)
    {
        var boolName = __instance.playerDataBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(PlayerDataBoolOperation))]
internal static class PlayerDataBoolOperationHook
{
    [HarmonyPatch(nameof(PlayerDataBoolOperation.Execute))]
    [HarmonyPrefix]
    public static void Execute(PlayerDataBoolOperation __instance)
    {
        var op = __instance;
        PlayerDataHook.BoolUpdated(op.variableName, op.value, op.operation);
    }
}


[HarmonyPatch(typeof(PlayerDataIntOperation))]
internal static class PlayerDataIntOperationHook
{
    [HarmonyPatch(nameof(PlayerDataBoolOperation.Execute))]
    [HarmonyPrefix]
    public static void Execute(PlayerDataIntOperation __instance)
    {
        var op = __instance;
        PlayerDataHook.IntUpdated(op.variableName, op.number, op.operation);
    }
}


[HarmonyPatch(typeof(SceneAdditiveLoadConditional))]
internal static class SceneAdditiveLoadConditionalHook
{
    [HarmonyPatch(nameof(SceneAdditiveLoadConditional.OnWasLoaded))]
    [HarmonyPrefix]
    public static void OnWasLoaded(SceneAdditiveLoadConditional __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.setPdBoolOnLoad, true);
    }
}


[HarmonyPatch(typeof(TempGate))]
internal static class TempGateHook
{
    [HarmonyPatch(nameof(TempGate.SetBroken))]
    [HarmonyPrefix]
    public static void SetBroken(TempGate __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.brokenPDBool, true);
    }
}


[HarmonyPatch(typeof(ToolItemToggleState))]
internal static class ToolItemToggleStateHook
{
    [HarmonyPatch(nameof(ToolItemToggleState.DoToggle))]
    [HarmonyPrefix]
    public static void DoToggle(ToolItemToggleState __instance)
    {
        PlayerDataHook.BoolUpdated(__instance.statePdBool, true, PlayerDataBoolOperation.Operation.Flip);
    }
}


[HarmonyPatch(typeof(QuestTargetPlayerDataBools))]
internal static class QuestTargetPlayerDataBoolsHook
{
    [HarmonyPatch(nameof(QuestTargetPlayerDataBools.Get))]
    [HarmonyPrefix]
    public static void Get(QuestTargetPlayerDataBools __instance)
    {
        if (!__instance.UsesSceneBools()) return;

        var boolName = __instance.pdFieldTemplate + GameManager.instance.GetSceneNameString();
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(SubQuest))]
internal static class SubQuestHook
{
    [HarmonyPatch(nameof(SubQuest.Get))]
    [HarmonyPrefix]
    public static void Get(SubQuest __instance)
    {
        var boolName = __instance.linkedBool;
        var value = true;

        PlayerDataHook.BoolUpdated(boolName, value);
    }

    [HarmonyPatch(nameof(SubQuest.HasBeenSeen), MethodType.Setter)]
    [HarmonyPrefix]
    public static void HasBeenSeen(SubQuest __instance, bool value)
    {
        var boolName = __instance.seenBool;
        PlayerDataHook.BoolUpdated(boolName, value);
    }
}


[HarmonyPatch(typeof(QuestRewardHolder))]
internal class QuestRewardHolderHook
{
    [HarmonyPatch(nameof(QuestRewardHolder.OnItemPickup))]
    [HarmonyPostfix]
    static void OnItemPickup(QuestRewardHolder __instance, ref bool __result)
    {
        if (__result == false) return;

        PlayerDataHook.BoolUpdated(__instance.pickupPdBool, true);
    }
}
