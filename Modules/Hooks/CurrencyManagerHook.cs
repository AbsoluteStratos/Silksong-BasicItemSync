using BasicItemSync.Modules.Network.Client;
using HarmonyLib;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(CurrencyManager))]
internal class CurrencyManagerHook
{
    [HarmonyPatch(nameof(CurrencyManager.ChangeCurrency))]
    [HarmonyPostfix]
    public static void ChangeCurrency(int amount, CurrencyType type)
    {
        Log.LogDebug($"[CLI: CURRENCY] {amount} {type}s added");

        if (amount < 1)
        {
            if (!ClientAddon.Settings.SyncSpendingCurrency) return;
            if (PlayerData.instance.health == 0) return;
            if (PlayerData.instance.atBench) return;
        }

        if (!ClientState.WasCurrencyReceived(amount))
        {
            var internalType = type == CurrencyType.Money ? InternalCurrencyType.Rosary : InternalCurrencyType.ShellShard;
            NetworkSender.AddCurrency(internalType, amount);
        }
    }
}