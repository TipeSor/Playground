using System.Diagnostics;
using System.Reflection;
using Playground.Projects;

namespace Playground.RandomTests
{
    public class TestProject : Project
    {
        public override void Start()
        {
            Test test = new(10);
            Debug.Assert(test.Value == 10);
            FieldInfo? field = typeof(Test).GetField("<Value>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(field != null);
            field.SetValueDirect(__makeref(test), 20);
            Debug.Assert(test.Value == 20);
            Stop = true;
        }

        public override void Finish() { }

        public readonly struct Test(int value)
        {
            public readonly int Value { get; } = value;
        }
    }

    /// <summary>This is a mock class</summary>
    public class MonoBehavior
    {

    }

    public class Inventory : MonoBehavior
    {
        public List<ItemStack> stacks = new();

        public void AddItem(ItemStack stack)
        {
            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i].item.name != stack.item.name) continue;

                stacks[i] = new ItemStack(stack.item, stacks[i].number + stack.number);
                return;
            }

            stacks.Add(stack);
        }

        public void TransferItemsToInventory(Inventory inventory)
        {
            for (int i = stacks.Count-1; i >= 0; i--)
            {
                inventory.AddItem(stacks[i]);
                stacks.Remove(stacks[i]);
            }
        }
    }

    public struct ItemStack
    {
        public ItemStack(Item item, int number)
        {
            this.item = item;
            this.number = number;
        }

        public Item item;
        public int number;
    }

    /// <summary>This is a mock class</summary>
    public struct Item
    {
        public string name;
    }
}
