#pragma warning disable IDE0011
namespace Playground.Inventory
{
    /// <summary>Item stack that has a limit</summary>
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
            return new AddResult(amount - added, added, true);
        }

        public SubtractResult Subtract(uint amount)
        {
            uint subtracted = Math.Min(amount, Amount);
            Amount -= subtracted;
            return new SubtractResult(amount - subtracted, subtracted, true);
        }

        public SetResult Set(uint amount)
        {
            uint newAmount = Math.Min(amount, MaxAmount);
            uint leftover = amount - newAmount;
            Amount = newAmount;
            return new SetResult(leftover, amount, true);
        }

        public override string ToString()
        {
            return $"{Item.Name}: {Amount}/{Item.MaxAmount}";
        }

        public IItemStack Clone()
        {
            return new ItemStack(Item, Amount);
        }
    }
}
