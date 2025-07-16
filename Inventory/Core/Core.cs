#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory.Core
{
    #region Records
    public record AddResult(uint Remaining, uint Added);
    public record SubtractResult(uint Remaining, uint Subtracted);
    public record TransferResult(uint Amount, bool Success, string Message);
    public record TradeResult(bool Success, string Message);
    #endregion

    #region Interfaces
    public interface IItemStack
    {
        Item Item { get; }
        uint Amount { get; }
        uint MaxAmount { get; }
        bool IsFull => Amount == MaxAmount;

        AddResult Add(uint amount);
        SubtractResult Subtract(uint amount);

        IItemStack Clone();
    }

    public interface IInventory
    {
        AddResult Add(IItemStack stack);
        SubtractResult Subtract(IItemStack stack);
        uint GetCount(Item item);

        void BeginTransaction();
        void Commit();
        void Rollback();

        bool InTransaction { get; }
    }
    #endregion

    #region Item Implementation
    public readonly struct Item(string id, string name, uint maxAmount = uint.MaxValue) : IEquatable<Item>
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public uint MaxAmount { get; } = maxAmount == 0 ? uint.MaxValue : maxAmount;

        public bool Equals(Item other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is Item other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Item left, Item right) => left.Equals(right);
        public static bool operator !=(Item left, Item right) => !left.Equals(right);

        public override string ToString() => $"{Name} ({Id}) - Max {MaxAmount}";
    }
    #endregion

    #region Item Stack Implementations
    public class ItemStack(Item item, uint amount) : IItemStack
    {
        public Item Item { get; } = item;
        public uint Amount { get; private set; } = Math.Min(amount, item.MaxAmount);

        public string Name => Item.Name;
        public uint MaxAmount => Item.MaxAmount;
        public bool IsFull => Amount == MaxAmount;

        public AddResult Add(uint amount)
        {
            uint availableSpace = MaxAmount - Amount;
            uint added = Math.Min(amount, availableSpace);
            Amount += added;
            return new AddResult(amount - added, added);
        }

        public SubtractResult Subtract(uint amount)
        {
            uint subtracted = Math.Min(amount, Amount);
            Amount -= subtracted;
            return new SubtractResult(amount - subtracted, subtracted);
        }

        public IItemStack Clone() => new ItemStack(Item, Amount);
        public override string ToString() => $"{Item.Name}: {Amount}/{Item.MaxAmount}";
    }

    public class OverflowStack(Item item, uint amount) : IItemStack
    {
        public Item Item { get; } = item;
        public uint Amount { get; private set; } = amount;

        public string Name => Item.Name;
        public uint MaxAmount => uint.MaxValue;
        public bool IsFull => false;

        public AddResult Add(uint amount)
        {
            uint availableSpace = MaxAmount - Amount;
            uint added = Math.Min(amount, availableSpace);
            Amount += added;
            return new AddResult(amount - added, added);
        }

        public SubtractResult Subtract(uint amount)
        {
            uint subtracted = Math.Min(amount, Amount);
            Amount -= subtracted;
            return new SubtractResult(amount - subtracted, subtracted);
        }

        public IItemStack Clone() => new OverflowStack(Item, Amount);
        public override string ToString() => $"{Item.Name}: {Amount}/{MaxAmount}";
    }

    public class UnlimitedStack(Item item) : IItemStack
    {
        public Item Item { get; } = item;
        public uint Amount => MaxAmount;
        public uint MaxAmount => uint.MaxValue;
        public bool IsFull => false;

        public AddResult Add(uint amount) => new(0, amount);
        public SubtractResult Subtract(uint amount) => new(0, amount);
        public IItemStack Clone() => new UnlimitedStack(Item);
    }
    #endregion

    #region Utility Class
    public static class InventoryUtil
    {
        public static TransferResult Transfer(
            IInventory source,
            IInventory target,
            IItemStack stack,
            bool exact = true)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(stack);

            if (source == target)
                return new TransferResult(0, true, "");

            if (source.InTransaction)
                return new TransferResult(0, false, "source in transaction");

            if (target.InTransaction)
                return new TransferResult(0, false, "target in transaction");

            uint available = source.GetCount(stack.Item);
            uint left = exact ? stack.Amount : Math.Min(available, stack.Amount);

            if (available < left)
                return new TransferResult(0, false, "Not enough items at source");

            source.BeginTransaction();
            target.BeginTransaction();

            OverflowStack temp = new OverflowStack(stack.Item, left);

            AddResult result = target.Add(temp);
            if (exact && result.Remaining != 0)
                return Fail($"target could not accept entire amount");

            SubtractResult result1 = source.Subtract(new OverflowStack(stack.Item, result.Added));
            if (exact && result1.Remaining != 0)
                return Fail($"source failed to subtract");

            source.Commit();
            target.Commit();
            return new TransferResult(result1.Subtracted, true, "");

            TransferResult Fail(string msg)
            {
                source.Rollback();
                target.Rollback();
                return new TransferResult(0, false, msg);
            }
        }

        public static TradeResult Trade(
            IInventory tradee,
            IInventory trader,
            IItemStack tradeeOffer,
            IItemStack traderOffer)
        {
            ArgumentNullException.ThrowIfNull(tradee);
            ArgumentNullException.ThrowIfNull(trader);
            ArgumentNullException.ThrowIfNull(tradeeOffer);
            ArgumentNullException.ThrowIfNull(traderOffer);

            if (tradee == trader)
                return new TradeResult(false, "Can't trade with yourself");

            if (tradee.InTransaction)
                return new TradeResult(false, "Tradee in transaction");

            if (trader.InTransaction)
                return new TradeResult(false, "Trader in transaction");

            uint temp1 = tradee.GetCount(traderOffer.Item);
            if (temp1 < traderOffer.Amount)
                return new TradeResult(false,
                    $"Tradee didn't have enough items. ({temp1})/({traderOffer.Amount}) {traderOffer.Item}");

            uint temp2 = trader.GetCount(tradeeOffer.Item);
            if (temp2 < tradeeOffer.Amount)
                return new TradeResult(false,
                    $"Trader didn't have enough items. ({temp2})/({tradeeOffer.Amount}) {tradeeOffer.Item}");

            tradee.BeginTransaction();
            trader.BeginTransaction();

            SubtractResult result1 = tradee.Subtract(tradeeOffer);
            if (result1.Remaining != 0)
                return Fail("Failed to subtract tradee offer from tradee");

            SubtractResult result2 = trader.Subtract(traderOffer);
            if (result2.Remaining != 0)
                return Fail("Failed to subtract trader offer from trader");

            AddResult result3 = tradee.Add(traderOffer);
            if (result3.Remaining != 0)
                return Fail("Failed to add trader offer to tradee");

            AddResult result4 = trader.Add(tradeeOffer);
            if (result4.Remaining != 0)
                return Fail("Failed to add tradee offer to trader");

            tradee.Commit();
            trader.Commit();
            return new TradeResult(true, "");

            TradeResult Fail(string msg)
            {
                tradee.Rollback();
                trader.Rollback();
                return new TradeResult(false, msg);
            }
        }
    }
    #endregion
}
