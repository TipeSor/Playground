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
}
