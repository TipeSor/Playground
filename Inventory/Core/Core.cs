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
    }

    public interface ITransactional
    {
        void BeginTransaction();
        void Commit();
        void Rollback();

        bool InTransaction { get; }
    }
    #endregion

    #region Item Implementation
    public readonly record struct Item(string Id, string Name, uint MaxAmount)
    {
        public Item(string id, string name)
            : this(id, name, uint.MaxValue) { }

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

            if (source is not ITransactional Source)
                return new TransferResult(0, false, "Source can't perform transactions");

            if (target is not ITransactional Target)
                return new TransferResult(0, false, "Target can't perform transactions");

            if (Source.InTransaction)
                return new TransferResult(0, false, "Source in transaction");

            if (Target.InTransaction)
                return new TransferResult(0, false, "Target in transaction");

            uint available = source.GetCount(stack.Item);
            uint left = exact ? stack.Amount : Math.Min(available, stack.Amount);

            if (available < left)
                return new TransferResult(0, false, "Not enough items at source");

            Source.BeginTransaction();
            Target.BeginTransaction();

            OverflowStack temp = new OverflowStack(stack.Item, left);

            AddResult result = target.Add(temp);
            if (exact && result.Remaining != 0)
                return Fail($"target could not accept entire amount");

            SubtractResult result1 = source.Subtract(new OverflowStack(stack.Item, result.Added));
            if (exact && result1.Remaining != 0)
                return Fail($"source failed to subtract");

            Source.Commit();
            Target.Commit();
            return new TransferResult(result1.Subtracted, true, "");

            TransferResult Fail(string msg)
            {
                Source.Rollback();
                Target.Rollback();
                return new TransferResult(0, false, msg);
            }
        }

        public static TradeResult Trade(
            IInventory tradee,
            IInventory trader,
            IItemStack[] tradeeOffer,
            IItemStack[] traderOffer)
        {
            ArgumentNullException.ThrowIfNull(tradee);
            ArgumentNullException.ThrowIfNull(trader);
            ArgumentNullException.ThrowIfNull(tradeeOffer);
            ArgumentNullException.ThrowIfNull(traderOffer);

            if (tradee == trader)
                return new TradeResult(false, "Can't trade with yourself");

            if (tradee is not ITransactional Tradee)
                return new TradeResult(false, "Tradee can't perform transactions");

            if (trader is not ITransactional Trader)
                return new TradeResult(false, "Trader can't perform transactions");

            if (Tradee.InTransaction)
                return new TradeResult(false, "Tradee in transaction");

            if (Trader.InTransaction)
                return new TradeResult(false, "Trader in transaction");

            foreach (IItemStack stack in tradeeOffer)
            {
                uint temp1 = tradee.GetCount(stack.Item);
                if (temp1 < stack.Amount)
                    return new TradeResult(false,
                        $"Tradee didn't have enough items. ({temp1})/({stack.Amount}) {stack.Item.Name}");
            }

            foreach (IItemStack stack in traderOffer)
            {
                uint temp2 = trader.GetCount(stack.Item);
                if (temp2 < stack.Amount)
                    return new TradeResult(false,
                        $"Trader didn't have enough items. ({temp2})/({stack.Amount}) {stack.Item.Name}");
            }

            Tradee.BeginTransaction();
            Trader.BeginTransaction();

            foreach (IItemStack stack in tradeeOffer)
            {
                SubtractResult result1 = tradee.Subtract(stack);
                if (result1.Remaining != 0)
                    return Fail("Failed to subtract tradee offer from tradee");

                AddResult result2 = trader.Add(stack);
                if (result2.Remaining != 0)
                    return Fail("Failed to add tradee offer to trader");
            }

            foreach (IItemStack stack in traderOffer)
            {
                SubtractResult result1 = trader.Subtract(stack);
                if (result1.Remaining != 0)
                    return Fail("Failed to subtract trader offer from trader");

                AddResult result2 = tradee.Add(stack);
                if (result2.Remaining != 0)
                    return Fail("Failed to add trader offer to tradee");
            }

            Tradee.Commit();
            Trader.Commit();
            return new TradeResult(true, "");

            TradeResult Fail(string msg)
            {
                Tradee.Rollback();
                Trader.Rollback();
                return new TradeResult(false, msg);
            }
        }
    }
    #endregion
}
