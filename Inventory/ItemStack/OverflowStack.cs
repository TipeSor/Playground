#pragma warning disable IDE0011
namespace Playground.Inventory
{
    /// <summary>Item stack with no upper limit</summary>
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
            Amount = amount;
            return new SetResult(0, amount, true);
        }

        public override string ToString()
        {
            return $"{Item.Name}: {Amount}/{MaxAmount}";
        }

        public IItemStack Clone()
        {
            return new OverflowStack(Item, Amount);
        }
    }
}
