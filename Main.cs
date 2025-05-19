using System.Diagnostics;

namespace Playground
{
    public static class Program
    {
        private static readonly Projects.Project project = new Projects.Colors();

        public static void Main()
        {
            // setup
            Stopwatch watch = Stopwatch.StartNew();

            // start project
            project.Start();

            // update loop
            while (!project.Stop)
            {
                long delta = watch.ElapsedMilliseconds;
                watch.Restart();
                project.Update(delta);
            }

            project.Finish();
        }
    }
}
