using BasicItemSync.Data;
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

        Log.LogInfo($"[CLI: ShopItemHook] Item: {item.name}");

        ClientState.LastItem = item.name;
        if (item.name == ItemNames.MaskShard) return; //NetworkSender.SendUpgrade("", FlagType.Mask);
        else if (item.name == ItemNames.SpoolShard) return; // NetworkSender.SendUpgrade("", FlagType.Spool);
        else if (item.name == ItemNames.ToolPouch) NetworkSender.SendUpgrade("", FlagType.Pouch);
        else if (item.name == ItemNames.CraftingKit) NetworkSender.SendUpgrade("", FlagType.CraftingKit);
        else if (item.name == ItemNames.NeedleUpgrade) NetworkSender.SendUpgrade("", FlagType.Needle);
        else NetworkSender.SendCollectable(item.name, item.GetPopupName(), 1);
    }

    [HarmonyPatch(nameof(ShopItem.SetPurchased))]
    [HarmonyPostfix]
    static void SetPurchasedPostfix(string __state)
    {
        ClientState.LastItem = __state;
    }
}
