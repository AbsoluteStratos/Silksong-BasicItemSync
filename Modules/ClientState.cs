namespace BasicItemSync.Modules
{
    internal class ClientState
    {
        public static string LastItem = "";
        public static string LastPlayerData = "";
        public static string LastPersistent = "";
        public static int LastCurrency = 0;
        public static FlagType LastUpgrade = FlagType.Currency;

        public static bool WasItemReceived(string item)
        {
            return LastItem == item;
        }

        public static bool WasCurrencyReceived(int amount)
        {
            return LastCurrency == amount;
        }

        public static bool WasUpgradeReceived(FlagType flagType)
        {
            return LastUpgrade == flagType;
        }

        public static bool WasPersistentReceived(string key)
        {
            return LastPersistent == key;
        }
    }
}
