using System.Text;
#pragma warning disable IDE0011, IDE0058, IDE0090, CA1305
namespace Playground.Inventory
{
    public class Crafter : ISafeInventory
    {
        private IItemStack[] _committedInput = [];
        private IItemStack[] _committedOutput = [];

        private IItemStack[] _workingInput = [];
        private IItemStack[] _workingOutput = [];

        private IItemStack[] ActiveInput => InTransaction ? _workingInput : _committedInput;
        private IItemStack[] ActiveOutput => InTransaction ? _workingOutput : _committedOutput;

        public bool InTransaction { get; private set; }
        public bool HasRecipe => ActiveRecipe != null;
        public CraftingRecipe? ActiveRecipe { get; private set; }

        public AddResult Add(IItemStack stack)
        {
            List<IItemStack> stacks = [];
            stacks.AddRange(ActiveInput.Where(s => s.Item == stack.Item));
            stacks.AddRange(ActiveOutput.Where(s => s.Item == stack.Item));

            uint left = stack.Amount;
            foreach (IItemStack _stack in stacks.OrderBy(s => -s.Amount))
            {
                left = _stack.Add(left).Remaining;
                if (left == 0)
                    return new AddResult(0, stack.Amount, true);
            }
            return new AddResult(left, stack.Amount - left, true);
        }

        public SubtractResult Subtract(IItemStack stack)
        {
            List<IItemStack> stacks = [];
            stacks.AddRange(ActiveInput.Where(s => s.Item == stack.Item));
            stacks.AddRange(ActiveOutput.Where(s => s.Item == stack.Item));

            uint left = stack.Amount;
            foreach (IItemStack _stack in stacks.OrderByDescending(s => s.Amount))
            {
                left = _stack.Subtract(left).Remaining;
                if (left == 0)
                    return new SubtractResult(0, stack.Amount, true);
            }
            return new SubtractResult(left, stack.Amount - left, true);
        }

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
            _workingInput = [];
            _workingOutput = [];
            InTransaction = false;
        }

        public void Rollback()
        {
            if (!InTransaction) return;
            _workingInput = [];
            _workingOutput = [];
            InTransaction = false;
        }

        public uint GetCount(Item item)
        {
            IEnumerable<IItemStack> inputStacks = ActiveInput.Where(s => s.Item == item);
            IEnumerable<IItemStack> outputStacks = ActiveOutput.Where(s => s.Item == item);

            uint total = 0;

            foreach (IItemStack stack in inputStacks)
                total += stack.Amount;

            foreach (IItemStack stack in outputStacks)
                total += stack.Amount;

            return total;
        }

        public TransferResult Transfer(ISafeInventory target, IItemStack stack, bool exact = true)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(stack);

            if (this == target)
                return new TransferResult(0, false, "can't transfer to self");

            if (InTransaction)
                return new TransferResult(0, false, "source in transaction");

            if (target.InTransaction)
                return new TransferResult(0, false, "target in transaction");

            BeginTransaction();
            target.BeginTransaction();

            uint available = GetCount(stack.Item);
            uint left = exact ? stack.Amount : Math.Min(available, stack.Amount);

            if (available < left)
                return new TransferResult(0, false, "Not enough items at source");

            OverflowStack temp = new OverflowStack(stack.Item, left);

            AddResult result = target.Add(temp.Clone());
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

        public void SetRecipe(CraftingRecipe recipe)
        {
            if (InTransaction) return;

            _committedInput = [];
            _committedOutput = [];

            _committedInput = CreateStacks([.. recipe.Input.Select(static i => i.Item1)]);
            _committedOutput = CreateStacks([.. recipe.Output.Select(static o => o.Item1)]);
            ActiveRecipe = recipe;
        }

        public CraftResult Craft(uint amount)
        {
            if (ActiveRecipe == null)
                return new CraftResult(0, false, "No active recipe");

            if (amount == 0)
                return new CraftResult(0, true, "");

            uint maxCrafts = Math.Min(amount, GetMaxCraftableAmount());
            if (maxCrafts == 0)
                return new CraftResult(0, false, "Not enough items to craft any");

            foreach ((Item item, uint requiredAmount) in ActiveRecipe.Input)
            {
                uint toSubtract = requiredAmount * maxCrafts;
                foreach (IItemStack? stack in ActiveInput.Where(s => s.Item == item).OrderByDescending(s => s.Amount))
                {
                    SubtractResult result = stack.Subtract(toSubtract);
                    toSubtract = result.Remaining;
                    if (toSubtract == 0)
                        break;
                }
            }

            foreach ((Item item, uint producedAmount) in ActiveRecipe.Output)
            {
                uint toAdd = producedAmount * maxCrafts;
                foreach (IItemStack? stack in ActiveOutput.Where(s => s.Item == item).OrderByDescending(s => s.Amount))
                {
                    AddResult result = stack.Add(toAdd);
                    toAdd = result.Remaining;
                    if (toAdd == 0)
                        break;
                }
            }

            return new CraftResult(maxCrafts, true, "");
        }


        public uint GetMaxCraftableAmount()
        {
            if (ActiveRecipe == null)
                return 0;

            uint maxCrafts = uint.MaxValue;

            foreach ((Item item, uint requiredAmount) in ActiveRecipe.Input)
            {
                uint availableAmount = (uint)ActiveInput
                    .Where(stack => stack.Item == item)
                    .Sum(stack => stack.Amount);

                if (availableAmount < requiredAmount)
                {
                    return 0;
                }

                maxCrafts = Math.Min(maxCrafts, availableAmount / requiredAmount);
            }

            return maxCrafts;
        }

        private static IItemStack[] CloneStacks(IItemStack[] source)
        {
            return [.. source.Select(static s => s.Clone())];
        }

        private static IItemStack[] CreateStacks(Item[] items)
        {
            return [.. items.Select(static i => new ItemStack(i, 0))];
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Crafter:");
            if (ActiveRecipe == null)
                return sb.AppendLine("- no recipe").ToString();

            sb.AppendLine("- Input:");
            foreach (IItemStack input in ActiveInput)
            {
                sb.AppendLine($"- - {input.Item.Name} {input.Amount}");
            }
            sb.AppendLine("- Output:");
            foreach (IItemStack output in ActiveOutput)
            {
                sb.AppendLine($"- - {output.Item.Name} {output.Amount}");
            }
            return sb.ToString();
        }
    }

    public record CraftResult(uint Crafted, bool Success, string Message);
    public record CraftingRecipe((Item, uint)[] Input, (Item, uint)[] Output);
}
