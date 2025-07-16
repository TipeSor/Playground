using System.Text;
using Playground.Inventory.Core;
#pragma warning disable IDE0011, IDE0022, IDE0024, IDE0058, IDE0090, CA1305, CA1711, CA1716

namespace Playground.Inventory.Crafter
{
    #region Records
    public record CraftResult(uint Crafted, bool Success, string Message);
    public record CraftingRecipe((Item Item, uint Amount)[] Input, (Item Item, uint Amount)[] Output);
    #endregion

    public class Crafter()
    {
        #region Properties
        public CrafterInventory Inventory { get; } = new();
        public CraftingRecipe? ActiveRecipe => Inventory.ActiveRecipe;
        public bool InTransaction => Inventory.InTransaction;

        private IItemStack[] ActiveInput => Inventory.ActiveInput;
        private IItemStack[] ActiveOutput => Inventory.ActiveOutput;
        #endregion

        #region Public Methods
        public void SetRecipe(CraftingRecipe recipe)
        {
            Inventory.SetRecipe(recipe);
        }

        public AddResult Add(ItemStack stack)
        {
            return Inventory.Add(stack);
        }

        public SubtractResult Subtract(ItemStack stack)
        {
            return Inventory.Subtract(stack);
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

            ProcessInputItems(maxCrafts);
            ProcessOutputItems(maxCrafts);

            return new CraftResult(maxCrafts, true, "");
        }

        public uint GetMaxCraftableAmount()
        {
            if (ActiveRecipe == null)
                return 0;

            uint maxCrafts = uint.MaxValue;

            foreach ((Item item, uint requiredAmount) in ActiveRecipe.Input)
            {
                uint availableAmount = GetAvailableInputAmount(item);

                if (availableAmount < requiredAmount)
                    return 0;

                maxCrafts = Math.Min(maxCrafts, availableAmount / requiredAmount);
            }

            foreach ((Item item, uint amount) in ActiveRecipe.Output)
            {
                if (amount == 0) continue;
                IItemStack[] stacks = GetOutputItems(item);
                if (stacks.Length == 0) return 0;
                foreach (IItemStack stack in stacks)
                {
                    maxCrafts = Math.Min(maxCrafts, (stack.MaxAmount - stack.Amount) / amount);
                    if (maxCrafts == 0) return 0;
                }
            }

            return maxCrafts;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Crafter:");

            AppendRecipeContent(sb);
            AppendInventoryContents(sb);
            return sb.ToString();
        }
        #endregion

        #region Private Methods
        private uint GetAvailableInputAmount(Item item)
        {
            return (uint)ActiveInput
                .Where(stack => stack.Item == item)
                .Sum(stack => stack.Amount);
        }

        private IItemStack[] GetOutputItems(Item item)
        {
            return [.. ActiveOutput.Where(stack => stack.Item == item)];
        }

        private void ProcessInputItems(uint craftCount)
        {
            foreach ((Item item, uint requiredAmount) in ActiveRecipe!.Input)
            {
                uint toSubtract = requiredAmount * craftCount;
                IOrderedEnumerable<IItemStack> stacks = ActiveInput
                    .Where(s => s.Item == item)
                    .OrderByDescending(s => s.Amount);

                foreach (IItemStack? stack in stacks)
                {
                    SubtractResult result = stack.Subtract(toSubtract);
                    toSubtract = result.Remaining;
                    if (toSubtract == 0) break;
                }
            }
        }

        private void ProcessOutputItems(uint craftCount)
        {
            foreach ((Item item, uint producedAmount) in ActiveRecipe!.Output)
            {
                uint toAdd = producedAmount * craftCount;
                IOrderedEnumerable<IItemStack> stacks = ActiveOutput
                    .Where(s => s.Item == item)
                    .OrderByDescending(s => s.Amount);

                foreach (IItemStack? stack in stacks)
                {
                    AddResult result = stack.Add(toAdd);
                    toAdd = result.Remaining;
                    if (toAdd == 0) break;
                }
            }
        }

        private void AppendRecipeContent(StringBuilder sb)
        {
            sb.AppendLine("- Recipe");
            if (ActiveRecipe == null) { sb.AppendLine("  - No recipe"); return; }

            sb.AppendLine("  - Input:");
            foreach ((Item item, uint amount) in ActiveRecipe.Input)
            {
                sb.AppendLine($"    - {amount}x {item.Name}");
            }

            sb.AppendLine("  - Output:");
            foreach ((Item item, uint amount) in ActiveRecipe.Output)
            {
                sb.AppendLine($"    - {amount}x {item.Name}");
            }
        }

        private void AppendInventoryContents(StringBuilder sb)
        {
            sb.AppendLine("- Inventory");
            sb.AppendLine("  - Input:");
            foreach (IItemStack input in ActiveInput)
            {
                sb.AppendLine($"    - {input.Item.Name} {input.Amount}");
            }

            sb.AppendLine("  - Output:");
            foreach (IItemStack output in ActiveOutput)
            {
                sb.AppendLine($"    - {output.Item.Name} {output.Amount}");
            }
        }
        #endregion
    }
}
