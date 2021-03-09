using VEDrivers.Economy.Wallets.Handlers;
using VEDrivers.Nodes.Handlers;

namespace VEconomy
{
    public static class MainDataContext
    {
        public static BasicAccountHandler AccountHandler { get; set; } = new BasicAccountHandler();
        public static BasicWalletHandler WalletHandler { get; set; } = new BasicWalletHandler();
        public static BasicNodeHandler NodeHandler { get; set; } = new BasicNodeHandler();
    }
}
