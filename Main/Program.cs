using System.Diagnostics;
using Playground.Projects;

namespace Playground
{
    public class Program
    {
        public static Project Active { get; internal set; } = new RandomTests.Temp();

        public static void Main(string[] args)
        {
            Active.Start(args);

            Stopwatch watch = Stopwatch.StartNew();
            while (!Active.Stop)
            {
                long delta = watch.ElapsedMilliseconds;
                watch.Restart();
                delta = delta == 0 ? 1 : delta;
                Active.Update(delta);
            }

            Active.Finish();
        }
    }

    public class Empty : Project
    {
        public override void Start()
        {
            base.Start();
        }

        public override void Update(long delta)
        {
            base.Update(delta);
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
