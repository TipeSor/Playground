using Playground.Inventory;
using Playground.Inventory.Core;
using Playground.Inventory.Crafter;
using Playground.Inventory.Shop;
using Playground.Projects;
#pragma warning disable IDE0058, IDE0090, CA1305
namespace Playground.RandomTests
{
    public class Temp : Project
    {
        public override void Start()
        {
            Item stone = new Item("stone.basic", "basic stone", 150);
            Item wood = new Item("wood.basic", "basic wood", 150);
            Item glass = new Item("glass.basic", "basic glass", 150);

            BasicShop shop;
            BasicInventory inventory;
            Crafter crafter;
            CraftingRecipe recipe;

            WriteTitle("Shop setup");
            {
                shop = new BasicShop();
                shop.Add(new UnlimitedStack(stone));
                shop.AddTrade(stone, new CostEntry(wood, 50));

                Console.WriteLine(shop);
            }

            WriteTitle("Inventory setup");
            {
                inventory = new();
                OverflowStack stack = new OverflowStack(wood, 7500);
                inventory.Add(stack);

                Console.WriteLine(inventory);
            }

            WriteTitle("Trade");
            {
                LoggedTrade(inventory, shop, stone, 50);

                Console.WriteLine(shop);
                Console.WriteLine(inventory);
            }

            WriteTitle("Crafter setup");
            {
                crafter = new Crafter();
                recipe = new CraftingRecipe(
                    [(stone, 10), (wood, 50)],
                    [(glass, 1)]
                );

                crafter.SetRecipe(recipe);

                Console.WriteLine(crafter);
            }

            WriteTitle("Transfering items to crafter");
            {
                Console.WriteLine(crafter);
                Console.WriteLine(inventory);

                LoggedTransfer(inventory, crafter.Inventory, new ItemStack(recipe.Input[0].Item, 150), false);
                LoggedTransfer(inventory, crafter.Inventory, new ItemStack(recipe.Input[1].Item, 150), false);

                Console.WriteLine(crafter);
                Console.WriteLine(inventory);
            }

            WriteTitle("Crafting");
            {
                LoggedCrafting(crafter, uint.MaxValue);

                Console.WriteLine(crafter);
            }

            WriteTitle("Transfering items from crafter");
            {
                LoggedTransfer(crafter.Inventory, inventory, new ItemStack(recipe.Output[0].Item, 150));

                Console.WriteLine(crafter);
                Console.WriteLine(inventory);
            }
        }

        public static void LoggedTransfer(IInventory source, IInventory target, IItemStack stack, bool exact = true)
        {
            Console.WriteLine($"Transfer: {stack.Amount} {stack.Item.Name} from `{nameof(source)}` to `{nameof(target)}`");
            TransferResult result = InventoryUtil.Transfer(source, target, stack, exact);
            Console.WriteLine($"Transfer Status:");
            Console.WriteLine($"- Transfered: {result.Amount} {stack.Item.Name} from `{nameof(source)}` to `{nameof(target)}`");
            Console.WriteLine($"- Status: {(result.Success ? "succeeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(result.Message) ? "nothing" : result.Message)}");
            Console.WriteLine();
        }

        public static void LoggedTrade(IInventory buyer, IShop seller, Item item, uint amount)
        {
            if (!seller.Trades.TryGetValue(item, out CostEntry? costEntry))
            { Console.WriteLine("Trade not found."); return; }

            Item currency = costEntry.Currency;
            uint cost = costEntry.Amount * amount;

            Console.WriteLine($"Trade: {cost}x {currency.Name} -> {amount}x {item.Name}\n");
            TradeResult result = seller.Trade(buyer, item, amount);

            Console.WriteLine($"Trade Status:");
            Console.WriteLine($"- Status: {(result.Success ? "succeeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(result.Message) ? "nothing" : result.Message)}");
            Console.WriteLine();

        }

        public static void LoggedCrafting(Crafter crafter, uint amount)
        {
            if (crafter.ActiveRecipe == null) { Console.WriteLine("No recipe selected"); return; }

            Console.WriteLine($"Craft: ");
            foreach ((Item output, _) in crafter.ActiveRecipe.Output)
            {
                Console.WriteLine($"- {(amount == uint.MaxValue ? "max" : $"{amount}x")} {output.Name}");
            }
            Console.WriteLine();

            CraftResult result3 = crafter.Craft(amount);

            Console.WriteLine($"Craft Status:");
            Console.WriteLine($"- Used:");
            foreach ((Item input, uint used) in crafter.ActiveRecipe.Input)
            {
                Console.WriteLine($"  - {used * result3.Crafted}x {input.Name}");
            }
            Console.WriteLine($"- Created:");
            foreach ((Item output, uint created) in crafter.ActiveRecipe.Output)
            {
                Console.WriteLine($"  - {created * result3.Crafted}x {output.Name}");
            }
            Console.WriteLine();
        }

        public static void WriteTitle(string title)
        {
            Console.WriteLine($"=== {title} ===");
        }
    }
}
