using BasicItemSync.Modules.Network.Client;
using HarmonyLib;

namespace BasicItemSync.Modules.Hooks;

[HarmonyPatch(typeof(ShopItem))]
internal static class ShopItemHook
{
    [HarmonyPatch(nameof(ShopItem.SetPurchased))]
    [HarmonyPrefix]
    static void SetPurchased(ShopItem __instance, int subItemIndex)
    {
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
        if (item != null)
        {
            if (item.name == "Heart Piece") NetworkSender.SendUpgrade("", FlagType.Mask);
            else if (item.name == "Silk Spool") NetworkSender.SendUpgrade("", FlagType.Spool);
            else if (item.name == "Tool Pouch Pickup") NetworkSender.SendUpgrade("", FlagType.Pouch);
            else if (item.name == "Took Kit Pickup") NetworkSender.SendUpgrade("", FlagType.CraftingKit);
            else if (item is ToolBase) return;
            else NetworkSender.SendCollectable(item.name, item.GetPopupName(), "", "");
        }
        else
        {
            Log.LogError($"[CLI: ShopItemHook] Unknown item for shop {__instance.name}");
        }
    }
}
