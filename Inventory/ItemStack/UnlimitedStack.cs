#pragma warning disable IDE0011
namespace Playground.Inventory
{
    public class UnlimitedStack(Item item) : IItemStack
    {
        public Item Item { get; } = item;
        public uint Amount => MaxAmount;
        public uint MaxAmount => uint.MaxValue;
        public bool IsFull => false;

        public AddResult Add(uint amount)
        {
            return new(0, amount, true);
        }

        public SubtractResult Subtract(uint amount)
        {
            return new(0, amount, true);
        }

        public SetResult Set(uint amount)
        {
            return new(0, amount, true);
        }

        public IItemStack Clone()
        {
            return new UnlimitedStack(Item);
        }
    }
}
