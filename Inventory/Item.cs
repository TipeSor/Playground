namespace Playground.Inventory
{
    public readonly struct Item(string id, string name, uint maxAmount = 64) : IEquatable<Item>
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public uint MaxAmount { get; } = maxAmount == 0 ? uint.MaxValue : maxAmount;

        public bool Equals(Item other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is Item other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Item left, Item right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Item left, Item right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Name} ({Id}) - Max {MaxAmount}";
        }
    }
}
