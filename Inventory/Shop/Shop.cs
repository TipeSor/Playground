using System.Text;
using Playground.Inventory.Core;
#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0046, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory.Shop
{
    #region Interfaces and Records
    public interface IShop
    {
        void AddTrade(TradeEntry trade);
        bool RemoveTrade(TradeEntry trade);

        bool TryCalculateCost(TradeEntry trade, uint multipler, out IItemStack[] cost);
        bool TryCalculateReward(TradeEntry trade, uint multipler, out IItemStack[] reward);

        TradeResult Trade(IInventory buyer, TradeEntry trade, uint multipler);
        IReadOnlyList<TradeEntry> Trades { get; }
    }

    public record CostEntry(Item Currency, uint Amount);
    #endregion

    #region TradeEntry Implementation
    public record TradeEntry(
        string Id,
        (Item Item, uint Amount)[] Cost,
        (Item Item, uint Amount)[] Reward
    );
    #endregion

    public class BasicShop : IShop
    {
        #region Fields and Properties
        private readonly ShopInventory _stock = new();
        private readonly List<TradeEntry> _trades = [];

        public IReadOnlyList<TradeEntry> Trades => _trades.AsReadOnly();
        #endregion

        #region Inventory Operations
        public AddResult Add(IItemStack stack) => _stock.Add(stack);
        public SubtractResult Subtract(IItemStack stack) => _stock.Subtract(stack);
        public uint GetCount(Item item) => _stock.GetCount(item);
        #endregion

        #region Trade Management
        public void AddTrade(TradeEntry trade) => _trades.Add(trade);
        public bool RemoveTrade(TradeEntry trade) => _trades.Remove(trade);
        #endregion

        #region Trade Validation
        public bool TryCalculateCost(TradeEntry trade, uint multipler, out IItemStack[] cost)
        {
            cost = new IItemStack[trade.Cost.Length];
            for (int i = 0; i < trade.Cost.Length; i++)
                try
                {
                    cost[i] = new OverflowStack(trade.Cost[i].Item, trade.Cost[i].Amount * multipler);
                }
                catch (OverflowException)
                {
                    cost = [];
                    return false;
                }
            return true;
        }

        public bool TryCalculateReward(TradeEntry trade, uint multipler, out IItemStack[] reward)
        {
            reward = new IItemStack[trade.Reward.Length];
            for (int i = 0; i < trade.Reward.Length; i++)
                try
                {
                    reward[i] = new OverflowStack(trade.Reward[i].Item, trade.Reward[i].Amount * multipler);
                }
                catch (OverflowException)
                {
                    reward = [];
                    return false;
                }
            return true;
        }
        #endregion

        #region Trade Execution
        public TradeResult Trade(IInventory buyer, TradeEntry trade, uint multipler)
        {
            if (!_trades.Contains(trade))
                return new TradeResult(false, $"Trade ({trade.Id}) not found");

            if (!TryCalculateCost(trade, multipler, out IItemStack[] TotalCost))
                return new TradeResult(false, "Failed to calculate cost");

            if (!TryCalculateReward(trade, multipler, out IItemStack[] TotalReward))
                return new TradeResult(false, "Failed to calculate reward");

            return InventoryUtil.Trade(buyer, _stock, TotalCost, TotalReward);
        }
        #endregion

        #region String Representation
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shop:");

            if (_stock.Count == 0) { sb.AppendLine("- No trades available"); return sb.ToString(); }

            foreach (TradeEntry trade in _trades)
            {
                sb.AppendLine($"- Offer ({trade.Id})");
                sb.AppendLine("  - Cost:");
                foreach ((Item Item, uint Amount) in trade.Cost)
                {
                    sb.AppendLine($"    - {Amount}x {Item.Name}");
                }
                sb.AppendLine("  - Reward:");
                foreach ((Item Item, uint Amount) in trade.Reward)
                {
                    sb.AppendLine($"    - {Amount}x {Item.Name}");
                }
            }

            return sb.ToString();
        }
        #endregion
    }
}
