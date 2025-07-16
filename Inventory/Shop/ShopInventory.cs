using System.Collections.ObjectModel;
using Playground.Inventory.Core;
#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory.Shop
{
    public class ShopInventory : IInventory
    {
        #region Fields
        private Dictionary<Item, OverflowStack> _committedStocks = [];
        private Dictionary<Item, OverflowStack> _workingStocks = [];
        #endregion

        #region Properties
        public bool InTransaction { get; private set; }
        public int Capacity { get; init; } = int.MaxValue;
        public int UniqueItemTypes => ActiveStocks.Count;
        public int TotalStocks => (int)ActiveStocks.Sum(static s => s.Value.Amount);
        public int Count => ActiveStocks.Count;
        public ReadOnlyDictionary<Item, OverflowStack> Stocks => ActiveStocks.AsReadOnly();

        private Dictionary<Item, OverflowStack> ActiveStocks =>
            InTransaction ? _workingStocks : _committedStocks;
        #endregion

        #region Inventory Operations
        public AddResult Add(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new AddResult(0, 0);

            if (!ActiveStocks.TryGetValue(stack.Item, out OverflowStack? existingStack))
            {
                ActiveStocks[stack.Item] = new OverflowStack(stack.Item, stack.Amount);
                return new AddResult(0, stack.Amount);
            }

            AddResult result = existingStack.Add(stack.Amount);
            ActiveStocks[stack.Item] = existingStack;
            return result;
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new SubtractResult(0, 0);

            if (!ActiveStocks.TryGetValue(stack.Item, out OverflowStack? existingStack))
                return new SubtractResult(stack.Amount, 0);

            SubtractResult result = existingStack.Subtract(stack.Amount);
            ActiveStocks[stack.Item] = existingStack;
            return result;
        }

        public uint GetCount(Item item)
        {
            return ActiveStocks.TryGetValue(item, out OverflowStack? stack) ? stack.Amount : 0;
        }
        #endregion

        #region Transaction Management
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
            ClearWorkingState();
            InTransaction = false;
        }

        public void Rollback()
        {
            if (!InTransaction) return;
            ClearWorkingState();
            InTransaction = false;
        }

        private void ClearWorkingState()
        {
            _workingStocks.Clear();
        }
        #endregion

        #region Helper Methods
        private static Dictionary<Item, OverflowStack> CloneStocks(
            Dictionary<Item, OverflowStack> source)
        {
            return source.ToDictionary(
                static entry => entry.Key,
                static entry => (OverflowStack)entry.Value.Clone());
        }
        #endregion
    }
}
