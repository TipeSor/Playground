using System.Text;
#pragma warning disable IDE0011, IDE0022
namespace Playground.Inventory.Shop
{
    public class BasicShop : IShop
    {
        private readonly ShopSafeInventory _stock = new();
        private readonly Dictionary<Item, CostEntry> _trades = [];

        public IReadOnlyDictionary<Item, CostEntry> Trades => _trades.AsReadOnly();

        public AddResult Add(IItemStack stack)
        {
            return _stock.Add(stack);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            return _stock.Subtract(stack);
        }

        public TradeResult CanTrade(ISafeInventory buyer, Item item, uint amount)
        {
            ArgumentNullException.ThrowIfNull(buyer);
            if (_stock == buyer)
                return new TradeResult(false, "Can't trade with self");

            if (_stock.InTransaction)
                return new TradeResult(false, "_stock in transaction");

            if (buyer.InTransaction)
                return new TradeResult(false, "buyer in transaction");

            if (!_trades.TryGetValue(item, out CostEntry? trade))
                return new TradeResult(false, $"No {item.Name} trade found.");

            Item currencyAmount = trade.Currency;
            uint cost;

            try { cost = checked(trade.Amount * amount); }
            catch (OverflowException) { return new TradeResult(false, "Trade cost too high."); }

            uint itemAvalable = GetCount(item);
            if (itemAvalable < amount)
                return new TradeResult(false, $"Not enough stock ({itemAvalable}/{amount}).");

            uint _currency = buyer.GetCount(currencyAmount);
            return _currency < cost
                ? new TradeResult(false, $"not enough funds. ({_currency}/{cost})")
                : new TradeResult(true, "");
        }

        public uint GetCount(Item item)
        {
            return _stock.GetCount(item);
        }

        public TradeResult Trade(ISafeInventory buyer, Item item, uint amount)
        {
            TradeResult result = CanTrade(buyer, item, amount);
            if (!result.Success)
                return result;

            Item currency = _trades[item].Currency;
            uint cost = _trades[item].Amount * amount;

            OverflowStack temp1 = new OverflowStack(item, amount);
            OverflowStack temp2 = new OverflowStack(currency, cost);

            _stock.BeginTransaction();
            buyer.BeginTransaction();

            if (_stock.Subtract(temp1).Remaining != 0)
                return Fail("Shop out of stock");
            if (buyer.Subtract(temp2).Remaining != 0)
                return Fail("Buyer has insufficient funds");
            if (_stock.Add(temp2).Remaining != 0)
                return Fail("Shop can't receive funds");
            if (buyer.Add(temp1).Remaining != 0)
                return Fail("Buyer can't receive item");

            _stock.Commit();
            buyer.Commit();
            return new TradeResult(true, "");

            TradeResult Fail(string msg)
            {
                _stock.Rollback();
                buyer.Rollback();
                return new TradeResult(false, msg);
            }
        }

        public bool AddTrade(Item item, CostEntry cost)
        {
            return _trades.TryAdd(item, cost);
        }

        public bool RemoveTrade(Item item)
        {
            return _trades.Remove(item);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Shop:");

            if (_stock.Count == 0) { sb.AppendLine("- no stock"); return sb.ToString(); }

            foreach ((Item item, IItemStack stock) in _stock.Stocks)
            {
                sb.AppendLine($"{item.Name}");
                if (stock is UnlimitedStack)
                    sb.AppendLine($"- stock: unlimited");
                else
                    sb.AppendLine($"- stock: {stock.Amount}");
                if (_trades.TryGetValue(item, out var costEntry))
                    sb.AppendLine($"- price: {costEntry.Amount}x {costEntry.Currency.Name}");
                else
                    sb.AppendLine($"- price: none");
            }
            return sb.ToString();
        }
    }

    public interface IShop
    {
        TradeResult Trade(ISafeInventory buyer, Item item, uint amount);
        TradeResult CanTrade(ISafeInventory buyer, Item item, uint amount);
        IReadOnlyDictionary<Item, CostEntry> Trades { get; }
    }

    public record TradeResult(bool Success, string Message);
    public record CostEntry(Item Currency, uint Amount);
}
