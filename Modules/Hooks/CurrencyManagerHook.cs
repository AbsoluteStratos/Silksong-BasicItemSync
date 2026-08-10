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
#if DEBUG
        //Log.LogInfo($"[CLI: CURRENCY] {amount} {type}s added");
#endif
        if (amount < 1) return;
        if (!ClientState.WasCurrencyReceived(amount))
        {
            var internalType = type == CurrencyType.Money ? InternalCurrencyType.Rosary : InternalCurrencyType.ShellShard;
            NetworkSender.AddCurrency(internalType, amount);
        }
    }
}