using BasicItemSync.Modules.Network.Client;
using HarmonyLib;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(ShopItem))]
internal static class ShopItemHook
{
    [HarmonyPatch(nameof(ShopItem.SetPurchased))]
    [HarmonyPrefix]
    static void SetPurchasedPrefix(ShopItem __instance, int subItemIndex, out string __state)
    {
        __state = ClientState.LastItem;
        var boolName = __instance.playerDataBoolName;
        PlayerDataHook.BoolUpdated(boolName, true);

        foreach (var extraBool in __instance.setExtraPlayerDataBools)
        {
            PlayerDataHook.BoolUpdated(extraBool, true);
        }

        if (subItemIndex >= 0 && subItemIndex < __instance.SubItemsCount)
        {
            var intName = __instance.playerDataIntName;
            var value = (int)__instance.GetSubItem(subItemIndex).Value;
            PlayerDataHook.IntUpdated(intName, value);
        }

        var item = __instance.Item;
        if (item == null)
        {
            Log.LogError($"[CLI: ShopItemHook] Unknown item for shop {__instance.name}");
            return;
        }

        if (item is ToolBase) return;

        ClientState.LastItem = item.name;
        if (item.name == "Heart Piece") return; //NetworkSender.SendUpgrade("", FlagType.Mask);
        else if (item.name == "Silk Spool") return; // NetworkSender.SendUpgrade("", FlagType.Spool);
        else if (item.name == "Tool Pouch Pickup") NetworkSender.SendUpgrade("", FlagType.Pouch);
        else if (item.name == "Took Kit Pickup") NetworkSender.SendUpgrade("", FlagType.CraftingKit);
        else NetworkSender.SendCollectable(item.name, item.GetPopupName(), 1);
    }

    [HarmonyPatch(nameof(ShopItem.SetPurchased))]
    [HarmonyPostfix]
    static void SetPurchasedPostfix(string __state)
    {
        ClientState.LastItem = __state;
    }
}
