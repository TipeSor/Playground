#pragma warning disable IDE0011, IDE0058, IDE0090, CA1305
using System.Text;

namespace Playground.Inventory
{
    public class BasicSafeInventory : ISafeInventory
    {
        private Dictionary<Item, List<ItemStack>> _committedStacks = [];
        private Dictionary<Item, List<ItemStack>> _workingStacks = [];

        public bool InTransaction { get; private set; }

        public int Capacity { get; init; } = int.MaxValue;
        public int UniqueItemTypes => ActiveStacks.Count;
        public int TotalStacks => ActiveStacks.Sum(static s => s.Value.Count);

        private Dictionary<Item, List<ItemStack>> ActiveStacks => InTransaction ? _workingStacks : _committedStacks;

        public AddResult Add(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0)
                return new AddResult(0, 0, true);

            uint left = stack.Amount;
            if (!ActiveStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
            {
                stacks = [];
                ActiveStacks[stack.Item] = stacks;
            }

            // back filling the exising stacks
            foreach (ItemStack existingStack in stacks)
            {
                left = existingStack.Add(left).Remaining;
                if (left == 0)
                    return new AddResult(0, stack.Amount, true);
            }

            // creating new stacks for leftover items
            while (left > 0 && TotalStacks < Capacity)
            {
                ItemStack newStack = new ItemStack(stack.Item, left);
                stacks.Add(newStack);
                left -= newStack.Amount;
            }

            return new AddResult(left, stack.Amount - left, true);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0) return new SubtractResult(0, 0, true);

            if (!ActiveStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
                return new SubtractResult(stack.Amount, 0, true);

            uint left = stack.Amount;
            uint totalSubtracted = 0;

            for (int i = stacks.Count - 1; i >= 0 && left > 0; i--)
            {
                SubtractResult subtractResult = stacks[i].Subtract(left);
                left = subtractResult.Remaining;
                totalSubtracted += subtractResult.Subtracted;

                if (stacks[i].Amount == 0)
                {
                    stacks.RemoveAt(i);
                }
            }

            return new SubtractResult(left, totalSubtracted, true);
        }

        public uint GetCount(Item item)
        {
            return (uint)(ActiveStacks.TryGetValue(item, out List<ItemStack>? stacks)
                    ? stacks.Sum(static s => s.Amount)
                    : 0);
        }

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
            _workingStacks.Clear();
            InTransaction = false;
        }

        public void Rollback()
        {
            if (!InTransaction) return;
            _workingStacks.Clear();
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

        private static Dictionary<Item, List<ItemStack>> CloneStacks(Dictionary<Item, List<ItemStack>> source)
        {
            return source.ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value.Select(static stack => new ItemStack(stack.Item, stack.Amount)).ToList()
            );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Inventory:");
            foreach ((Item item, List<ItemStack> stacks) in ActiveStacks.OrderBy(static i => i.Key.Name))
            {
                sb.AppendLine($"{item.Name}:");
                sb.AppendLine($"- total: {GetCount(item)}");
            }
            return sb.ToString();
        }
    }
}
