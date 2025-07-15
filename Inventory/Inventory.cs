#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716
namespace Playground.Inventory
{
    public interface IInventory
    {
        AddResult Add(IItemStack stack);
        SubtractResult Subtract(IItemStack stack);
        uint GetCount(Item item);
    }

    public interface ISafeInventory
    {
        AddResult Add(IItemStack stack);
        SubtractResult Subtract(IItemStack stack);
        uint GetCount(Item item);

        void BeginTransaction();
        void Commit();
        void Rollback();

        TransferResult Transfer(ISafeInventory target, IItemStack stack, bool exact = true);

        bool InTransaction { get; }
    }

    public interface IItemStack
    {
        Item Item { get; }
        uint Amount { get; }
        uint MaxAmount { get; }
        bool IsFull => Amount == MaxAmount;

        AddResult Add(uint amount);
        SetResult Set(uint amount);
        SubtractResult Subtract(uint amount);

        IItemStack Clone();
    }

    public record AddResult(uint Remaining, uint Added, bool Success);
    public record SubtractResult(uint Remaining, uint Subtracted, bool Success);
    public record SetResult(uint Remaining, uint Value, bool Success);

    public record TransferResult(uint Amount, bool Success, string Message);
}
