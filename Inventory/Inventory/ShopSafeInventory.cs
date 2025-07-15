using System.Collections.ObjectModel;
#pragma warning disable IDE0011, IDE0058, IDE0090, CA1305
namespace Playground.Inventory
{
    public class ShopSafeInventory : IInventory
    {
        private Dictionary<Item, IItemStack> _committedStocks = [];
        private Dictionary<Item, IItemStack> _workingStocks = [];

        public bool InTransaction { get; private set; }

        public int Capacity { get; init; } = int.MaxValue;
        public int UniqueItemTypes => ActiveStocks.Count;
        public int TotalStocks => (int)ActiveStocks.Sum(static s => s.Value.Amount);
        public int Count => ActiveStocks.Count;

        public ReadOnlyDictionary<Item, IItemStack> Stocks => ActiveStocks.AsReadOnly();

        private Dictionary<Item, IItemStack> ActiveStocks => InTransaction ? _workingStocks : _committedStocks;

        public AddResult Add(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0)
                return new AddResult(0, 0, true);

            if (!ActiveStocks.TryGetValue(stack.Item, out IItemStack? existing))
            {
                ActiveStocks[stack.Item] = stack.Clone();
                return new AddResult(0, stack.Amount, true);
            }

            uint left = existing.Add(stack.Amount).Remaining;
            ActiveStocks[stack.Item] = existing;
            return new AddResult(left, stack.Amount - left, true);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new SubtractResult(0, 0, true);

            if (!ActiveStocks.TryGetValue(stack.Item, out IItemStack? existing))
                return new SubtractResult(stack.Amount, 0, true);


            uint left = existing.Subtract(stack.Amount).Remaining;
            ActiveStocks[stack.Item] = existing;
            return new SubtractResult(left, stack.Amount - left, true);
        }

        public uint GetCount(Item item)
        {
            return ActiveStocks.TryGetValue(item, out IItemStack? stacks)
                ? stacks.Amount
                : 0;
        }

        public void BeginTransaction()
        {
            if (InTransaction) return;
            _workingStocks = CloneStocks(_committedStocks);
            InTransaction = true;
        }

        public void Commit()
        {
            if (!InTransaction) return;
            _committedStocks = CloneStocks(_workingStocks);
            _workingStocks.Clear();
            InTransaction = false;
        }

        public void Rollback()
        {
            if (!InTransaction) return;
            _workingStocks.Clear();
            InTransaction = false;
        }

        public TransferResult Transfer(ISafeInventory target, IItemStack stack, bool exact = true)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(stack);

            if (this == target)
                return new TransferResult(0, false, "can't transfer to self");

            uint available = GetCount(stack.Item);
            uint left = exact ? stack.Amount : Math.Min(available, stack.Amount);

            if (available < left)
                return new TransferResult(0, false, "Not enough items at source");

            if (InTransaction)
                return new TransferResult(0, false, "source in transaction");

            if (target.InTransaction)
                return new TransferResult(0, false, "target in transaction");

            BeginTransaction();
            target.BeginTransaction();

            OverflowStack temp = new OverflowStack(stack.Item, left);

            AddResult result = target.Add(temp);
            if (exact && result.Remaining != 0)
                return Fail($"target could not accept entire amount");

            SubtractResult result1 = Subtract(new OverflowStack(stack.Item, result.Added));
            if (exact && result1.Remaining != 0)
                return Fail($"source failed to subtract");

            Commit();
            target.Commit();
            return new TransferResult(result1.Subtracted, true, "");

            TransferResult Fail(string msg)
            {
                Rollback();
                target.Rollback();
                return new TransferResult(0, false, msg);
            }
        }

        private static Dictionary<Item, IItemStack> CloneStocks(Dictionary<Item, IItemStack> source)
        {
            return source.ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value.Clone()
            );
        }
    }
}
