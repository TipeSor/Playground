using Playground.Inventory;
using Playground.Inventory.Shop;
using Playground.Projects;
#pragma warning disable IDE0058, IDE0090
namespace Playground.RandomTests
{
    public class Temp : Project
    {
        public override void Start()
        {
            Item stone = new Item("stone.basic", "basic stone", 150);
            Item wood = new Item("wood.basic", "basic wood", 150);
            Item glass = new Item("glass.basic", "basic glass", 150);

            BasicShop shop = new BasicShop();
            shop.Add(new UnlimitedStack(stone));
            shop.AddTrade(stone, new CostEntry(wood, 50));

            BasicSafeInventory inventory = new();
            OverflowStack stack = new OverflowStack(wood, 7500);
            inventory.Add(stack);

            Console.WriteLine(shop);
            Console.WriteLine(inventory);
            Console.WriteLine(new string('-', 10));

            Item tradeItem = stone;
            uint tradeAmount = 50;

            if (!shop.Trades.TryGetValue(tradeItem, out CostEntry? costEntry))
            { Console.WriteLine("Trade not found."); return; }

            Item currency = costEntry.Currency;
            uint cost = costEntry.Amount * tradeAmount;

            Console.WriteLine($"Trade: {cost}x {currency.Name} -> {tradeAmount}x {tradeItem.Name}\n");
            TradeResult res = shop.Trade(inventory, stone, 50);

            Console.WriteLine($"Trade Status:");
            Console.WriteLine($"- Status: {(res.Success ? "succeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(res.Message) ? "nothing" : res.Message)}");
            Console.WriteLine();

            Console.WriteLine(shop);
            Console.WriteLine(inventory);
            Console.WriteLine(new string('-', 10));

            Crafter crafter = new Crafter();
            CraftingRecipe recipe = new CraftingRecipe(
                [(stone, 10), (wood, 50)],
                [(glass, 1)]
            );

            crafter.SetRecipe(recipe);

            Console.WriteLine(crafter);
            Console.WriteLine(inventory);
            Console.WriteLine(new string('-', 10));

            ItemStack transfer1 = new ItemStack(recipe.Input[0].Item1, 150);

            Console.WriteLine($"Transfer: {transfer1.Amount} {transfer1.Item.Name} From {nameof(inventory)} to {nameof(crafter)}");
            TransferResult result1 = inventory.Transfer(crafter, transfer1, false);
            Console.WriteLine($"Transfer Status:");
            Console.WriteLine($"- Transfered: {result1.Amount} {transfer1.Item.Name} From {nameof(inventory)} to {nameof(crafter)}");
            Console.WriteLine($"- Status: {(result1.Success ? "succeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(result1.Message) ? "nothing" : res.Message)}");
            Console.WriteLine();

            ItemStack transfer2 = new ItemStack(recipe.Input[1].Item1, 150);

            Console.WriteLine($"Transfer: {transfer2.Amount} {transfer2.Item.Name} From {nameof(inventory)} to {nameof(crafter)}");
            TransferResult result2 = inventory.Transfer(crafter, transfer2, false);
            Console.WriteLine($"Transfer Status:");
            Console.WriteLine($"- Transfered: {result2.Amount} {transfer2.Item.Name} From {nameof(inventory)} to {nameof(crafter)}");
            Console.WriteLine($"- Status: {(result2.Success ? "succeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(result2.Message) ? "nothing" : res.Message)}");
            Console.WriteLine();

            Console.WriteLine(crafter);
            Console.WriteLine(inventory);
            Console.WriteLine(new string('-', 10));


            Console.WriteLine($"Craft: ");
            foreach ((Item output, _) in recipe.Output)
            {
                Console.WriteLine($"- max {output.Name}");
            }
            Console.WriteLine();

            CraftResult result3 = crafter.Craft(uint.MaxValue);

            Console.WriteLine($"Craft Status:");
            Console.WriteLine($"- Used:");
            foreach ((Item input, uint amount) in recipe.Input)
            {
                Console.WriteLine($"- - {amount * result3.Crafted}x {input.Name}");
            }
            Console.WriteLine($"- Created:");
            foreach ((Item output, uint amount) in recipe.Output)
            {
                Console.WriteLine($"- - {amount * result3.Crafted}x {output.Name}");
            }
            Console.WriteLine();

            Console.WriteLine(crafter);
            Console.WriteLine(inventory);
            Console.WriteLine(new string('-', 10));

            ItemStack transfer3 = new ItemStack(recipe.Output[0].Item1, 150);

            Console.WriteLine($"Transfer: {transfer3.Amount} {transfer3.Item.Name} From {nameof(crafter)} to {nameof(inventory)}");
            TransferResult result4 = crafter.Transfer(inventory, transfer3, false);
            Console.WriteLine($"Transfer Status:");
            Console.WriteLine($"- Transfered: {result4.Amount} {transfer3.Item.Name} From {nameof(crafter)} to {nameof(inventory)}");
            Console.WriteLine($"- Status: {(result4.Success ? "succeded" : "failed")}");
            Console.WriteLine($"- Message: {(string.IsNullOrEmpty(result4.Message) ? "nothing" : res.Message)}");
            Console.WriteLine();

            Console.WriteLine(crafter);
            Console.WriteLine(inventory);
        }
    }
}
