using System.Text;
#pragma warning disable IDE0011, IDE0058, IDE0090, CA1305
namespace Playground.Inventory
{
    /// <summary>A simple Inventory that grows to capacity</summary>
    public class BasicInventory : IInventory
    {
        private readonly Dictionary<Item, List<ItemStack>> _itemStacks = [];
        public int Capacity { get; init; } = int.MaxValue;
        public int UniqueItemTypes => _itemStacks.Count;
        public int TotalStacks => _itemStacks.Sum(static s => s.Value.Count);

        public AddResult Add(IItemStack stack)
        {
            ArgumentNullException.ThrowIfNull(stack);
            if (stack.Amount == 0)
                return new AddResult(0, 0, true);

            uint left = stack.Amount;
            if (!_itemStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
            {
                stacks = [];
                _itemStacks[stack.Item] = stacks;
            }

            // back filling the exising stacks
            foreach (ItemStack existingStack in stacks)
            {
                left = existingStack.Add(left).Remaining;
                if (left == 0) return new AddResult(0, stack.Amount, true);
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

            if (!_itemStacks.TryGetValue(stack.Item, out List<ItemStack>? stacks))
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
            return (uint)(_itemStacks.TryGetValue(item, out List<ItemStack>? stacks)
                    ? stacks.Sum(static s => s.Amount)
                    : 0);
        }

        public void TransferTo(IInventory target, Item item)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (this == target || !_itemStacks.TryGetValue(item, out List<ItemStack>? stacks))
                return;

            uint totalAmount = (uint)stacks.Sum(static s => s.Amount);
            if (totalAmount == 0) { _itemStacks.Remove(item); return; }

            OverflowStack transferStack = new OverflowStack(item, totalAmount);

            uint returned = target.Add(transferStack).Remaining;
            uint transfered = totalAmount - returned;

            _ = transferStack.Set(transfered);
            Subtract(transferStack);
        }

        public void TransferAllTo(IInventory target)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (this == target) return;

            foreach (Item item in _itemStacks.Keys.ToList())
                TransferTo(target, item);
        }

        public bool Contains(Item item)
        {
            return _itemStacks.ContainsKey(item);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Inventory:");
            sb.AppendLine($"- capacity: {TotalStacks}/{Capacity}");
            foreach ((Item item, List<ItemStack> stacks) in _itemStacks.OrderBy(static i => i.Key.Name))
            {
                sb.AppendLine($"{item.Name}:");
                sb.AppendLine($"- total: {GetCount(item)}");
            }
            return sb.ToString();
        }
    }
}
