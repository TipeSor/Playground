using Playground.Projects;

namespace Playground.Drawing.Projects
{
    public class Test : Project
    {
        private readonly ColorDisplay Display = new(32 + 2, (9 * 2) + 2);

        public override void Start()
        {
            Console.Clear();

            for (int x = 0; x < Display.Width; x++)
            {
                for (int y = 0; y < Display.Height; y++)
                {
                    Color color;

                    color = (x + y) % 2 == 0 ? new Color(0, 0, 0) : new Color(255, 255, 255);

                    if (x == 0 || x == Display.Width - 1 ||
                        y == 0 || y == Display.Height - 1)
                    {
                        color = new Color(255, 0, 255);
                    }

                    Display.SetPixel(
                        x: x, y: y,
                        color: color,
                        depth: 0
                    );
                }
            }

            Display.Flush();
        }

        public override void Update(long delta)
        {
            if (Console.ReadKey(true).KeyChar == 'q')
            {
                Stop = true;
            }
        }
    }
}

