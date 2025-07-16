using System.Text;
using Playground.Inventory.Core;
#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory.Shop
{
    #region Interfaces and Records
    public interface IShop
    {
        TradeResult Trade(IInventory buyer, Item item, uint amount);
        IReadOnlyDictionary<Item, CostEntry> Trades { get; }
    }

    public record CostEntry(Item Currency, uint Amount);
    #endregion

    public class BasicShop : IShop
    {
        #region Fields and Properties
        private readonly ShopInventory _stock = new();
        private readonly Dictionary<Item, CostEntry> _trades = [];

        public IReadOnlyDictionary<Item, CostEntry> Trades => _trades.AsReadOnly();
        #endregion

        #region Inventory Operations
        public AddResult Add(IItemStack stack) => _stock.Add(stack);
        public SubtractResult Subtract(IItemStack stack) => _stock.Subtract(stack);
        public uint GetCount(Item item) => _stock.GetCount(item);
        #endregion

        #region Trade Management
        public bool AddTrade(Item item, CostEntry cost) => _trades.TryAdd(item, cost);
        public bool RemoveTrade(Item item) => _trades.Remove(item);
        #endregion

        #region Trade Validation
        private static bool TryCalculateCost(CostEntry trade, uint amount, out uint totalCost)
        {
            try
            {
                totalCost = checked(trade.Amount * amount);
                return true;
            }
            catch (OverflowException)
            {
                totalCost = 0;
                return false;
            }
        }
        #endregion

        #region Trade Execution
        public TradeResult Trade(IInventory buyer, Item item, uint amount)
        {
            if (!_trades.TryGetValue(item, out CostEntry? trade))
                return new TradeResult(false, "Item not available for trade");

            OverflowStack itemStack = new(item, amount);

            if (!TryCalculateCost(trade, amount, out uint totalCost))
                return new TradeResult(false, "Failed to calculate cost");

            OverflowStack currencyStack = new(trade.Currency, totalCost);

            return InventoryUtil.Trade(buyer, _stock, itemStack, currencyStack);
        }
        #endregion

        #region String Representation
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shop Inventory:");

            if (_stock.Count == 0) { sb.AppendLine("- No stock available"); return sb.ToString(); }

            AppendStockInformation(sb);
            return sb.ToString();
        }

        private void AppendStockInformation(StringBuilder sb)
        {
            foreach ((Item item, IItemStack stock) in _stock.Stocks)
            {
                sb.AppendLine($"- {item.Name}");
                sb.AppendLine($"  Stock: {(stock is UnlimitedStack ? "unlimited" : stock.Amount.ToString())}");
                sb.AppendLine($"  Price: {GetPriceInformation(item)}");
            }
        }

        private string GetPriceInformation(Item item)
        {
            return _trades.TryGetValue(item, out CostEntry? costEntry)
                ? $"{costEntry.Amount}x {costEntry.Currency.Name}"
                : "not for sale";
        }
        #endregion
    }
}
