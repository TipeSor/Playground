using System.Text;
using Playground.Inventory.Core;
#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory
{
    public class BasicInventory : IInventory, ITransactional
    {
        #region Fields
        private Dictionary<Item, List<ItemStack>> _committedStacks = [];
        private Dictionary<Item, List<ItemStack>> _workingStacks = [];
        #endregion

        #region Properties
        public bool InTransaction { get; private set; }
        public int Capacity { get; init; } = int.MaxValue;
        public int UniqueItemTypes => ActiveStacks.Count;
        public int TotalStacks => ActiveStacks.Sum(static s => s.Value.Count);

        private Dictionary<Item, List<ItemStack>> ActiveStacks =>
            InTransaction ? _workingStacks : _committedStacks;
        #endregion

        #region Inventory Operations
        public AddResult Add(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new AddResult(0, 0);

            if (!ActiveStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
            {
                stacks = [];
                ActiveStacks[stack.Item] = stacks;
            }

            uint remaining = stack.Amount;

            foreach (ItemStack existingStack in stacks)
            {
                remaining = existingStack.Add(remaining).Remaining;
                if (remaining == 0) break;
            }

            while (remaining > 0 && TotalStacks < Capacity)
            {
                ItemStack newStack = new ItemStack(stack.Item, remaining);
                stacks.Add(newStack);
                remaining -= newStack.Amount;
            }

            return new AddResult(remaining, stack.Amount - remaining);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new SubtractResult(0, 0);

            if (!ActiveStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
                return new SubtractResult(stack.Amount, 0);

            uint remaining = stack.Amount;
            uint totalSubtracted = 0;

            for (int i = stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                SubtractResult result = stacks[i].Subtract(remaining);
                remaining = result.Remaining;
                totalSubtracted += result.Subtracted;

                if (stacks[i].Amount == 0)
                {
                    stacks.RemoveAt(i);
                }
            }

            return new SubtractResult(remaining, totalSubtracted);
        }

        public uint GetCount(Item item)
        {
            return (uint)(ActiveStacks.TryGetValue(item, out List<ItemStack>? stacks)
                ? stacks.Sum(static s => s.Amount)
                : 0);
        }
        #endregion

        #region Transaction Management
        public void BeginTransaction()
        {
            if (InTransaction) return;
            _workingStacks = CloneStacks(_committedStacks);
            InTransaction = true;
        }

        public void Commit()
        {
            if (!InTransaction) return;
            _committedStacks = CloneStacks(_workingStacks);
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
            _workingStacks.Clear();
        }
        #endregion

        #region Helper Methods
        private static Dictionary<Item, List<ItemStack>> CloneStacks(
            Dictionary<Item, List<ItemStack>> source)
        {
            return source.ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value.Select(static stack => (ItemStack)stack.Clone()).ToList());
        }
        #endregion

        #region String Representation
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Inventory Contents:");

            if (ActiveStacks.Count == 0)
            {
                sb.AppendLine("- Empty");
                return sb.ToString();
            }

            foreach ((Item item, List<ItemStack> stacks) in ActiveStacks.OrderBy(static i => i.Key.Name))
            {
                sb.AppendLine($"- {item.Name}: {GetCount(item)} total");
            }

            return sb.ToString();
        }
        #endregion
    }
}
