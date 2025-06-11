using System.Diagnostics;
using Playground.Projects;

namespace Playground
{
    public class Program
    {
        public static Project Active { get; internal set; } = new Drawing.Projects.Test();

        public static void Main(string[] args)
        {
            Active.Start(args);

            Stopwatch watch = Stopwatch.StartNew();
            while (!Active.Stop)
            {
                long delta = watch.ElapsedMilliseconds;
                watch.Restart();
                Active.Update(delta);
            }
            Active.Finish();
        }
    }

    public class Basic : Project
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
