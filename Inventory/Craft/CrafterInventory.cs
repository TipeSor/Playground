#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

using Playground.Inventory.Core;

namespace Playground.Inventory.Crafter
{
    public class CrafterInventory : IInventory
    {
        #region Fields
        private IItemStack[] _committedInput = [];
        private IItemStack[] _committedOutput = [];
        private IItemStack[] _workingInput = [];
        private IItemStack[] _workingOutput = [];
        #endregion

        #region Properties
        public IItemStack[] ActiveInput => InTransaction ? _workingInput : _committedInput;
        public IItemStack[] ActiveOutput => InTransaction ? _workingOutput : _committedOutput;
        public bool InTransaction { get; private set; }
        public bool HasRecipe => ActiveRecipe != null;
        public CraftingRecipe? ActiveRecipe { get; private set; }
        #endregion

        #region Transaction Management
        public void BeginTransaction()
        {
            if (InTransaction) return;
            _workingInput = CloneStacks(_committedInput);
            _workingOutput = CloneStacks(_committedOutput);
            InTransaction = true;
        }

        public void Commit()
        {
            if (!InTransaction) return;
            _committedInput = CloneStacks(_workingInput);
            _committedOutput = CloneStacks(_workingOutput);
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
            _workingInput = [];
            _workingOutput = [];
        }
        #endregion

        #region Inventory Operations
        public AddResult Add(IItemStack stack)
        {
            IEnumerable<IItemStack> relevantStacks = GetRelevantStacks(stack.Item);
            uint remaining = stack.Amount;

            foreach (IItemStack? targetStack in relevantStacks.OrderByDescending(static s => s.Amount))
            {
                remaining = targetStack.Add(remaining).Remaining;
                if (remaining == 0) break;
            }

            return new AddResult(remaining, stack.Amount - remaining);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            IEnumerable<IItemStack> relevantStacks = GetRelevantStacks(stack.Item);
            uint remaining = stack.Amount;

            foreach (IItemStack? sourceStack in relevantStacks.OrderByDescending(static s => s.Amount))
            {
                remaining = sourceStack.Subtract(remaining).Remaining;
                if (remaining == 0) break;
            }

            return new SubtractResult(remaining, stack.Amount - remaining);
        }

        public uint GetCount(Item item)
        {
            return (uint)ActiveInput.Concat(ActiveOutput)
                .Where(s => s.Item == item)
                .Sum(s => s.Amount);
        }
        #endregion

        #region Recipe Management
        public void SetRecipe(CraftingRecipe recipe)
        {
            if (InTransaction) return;

            _committedInput = CreateStacks([.. recipe.Input.Select(static i => i.Item)]);
            _committedOutput = CreateStacks([.. recipe.Output.Select(static o => o.Item)]);
            ActiveRecipe = recipe;
        }
        #endregion

        #region Helper Methods
        private IEnumerable<IItemStack> GetRelevantStacks(Item item)
        {
            return ActiveInput.Concat(ActiveOutput)
                .Where(s => s.Item == item);
        }

        private static IItemStack[] CloneStacks(IEnumerable<IItemStack> source)
        {
            return [.. source.Select(static s => s.Clone())];
        }

        private static IItemStack[] CreateStacks(Item[] items)
        {
            return [.. items.Select(static i => new ItemStack(i, 0))];
        }
        #endregion
    }
}
