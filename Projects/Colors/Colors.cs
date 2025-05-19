using System.Numerics;

namespace Playground.Projects
{
    public class Colors : Project
    {
        private Vector3 Size = new(30, 30, 1);
        private Vector3 Offset = new(30, 30, 30);
        private readonly Vector3[] Rotation = new Vector3[3];

        private readonly float Speed = 0.0001f;

        public override void Start()
        {
            Display.ColorScreen.Start();
            new Thread(QuitThread).Start();

            Rotation[0] = new(0, 0, 0);
            Rotation[1] = new(0, 0, 0);
            Rotation[2] = new(0, 0, 0);
        }

        public override void Update(float delta)
        {
            Display.ColorScreen.Clear();

            DrawRing(Size / 1, Offset, Rotation[0], new(255, 0, 0));
            DrawRing(Size / 2, Offset, Rotation[1], new(0, 255, 0));
            DrawRing(Size / 3, Offset, Rotation[2], new(0, 0, 255));

            Rotation[0] += new Vector3(1f, -7f, 3f) * delta * Speed;
            Rotation[1] += new Vector3(-3f, 1f, 7f) * delta * Speed;
            Rotation[2] += new Vector3(7f, 3f, -1f) * delta * Speed;

            Display.ColorScreen.Flush();
        }

        public override void Finish()
        {
            Console.Clear();
        }

        public static void DrawRing(Vector3 size, Vector3 offset, Vector3 rotation, Display.Color color)
        {
            for (int i = 0; i < 62831; i++)
            {
                Vector3 pos = new(0, 0, 0)
                {
                    X = MathF.Cos(i / 10000f) * size.X,
                    Y = MathF.Sin(i / 10000f) * size.Y,
                    Z = 0 * size.Z,
                };

                pos = Math.Euler.Rotate(pos, rotation.X, rotation.Y, rotation.Z);

                pos += offset;

                float depth = System.Math.Clamp(pos.Z / 60f, 0, 1);

                int x = (int)System.Math.Round(pos.X, 0);
                int y = (int)System.Math.Round(pos.Y, 0);

                Display.Color _color = new(
                    (byte)System.Math.Clamp(color.R * depth, 1, 255),
                    (byte)System.Math.Clamp(color.G * depth, 1, 255),
                    (byte)System.Math.Clamp(color.B * depth, 1, 255)
                );

                Display.ColorScreen.SetPixel(x, y, _color, pos.Z);
            }
        }
        public void QuitThread()
        {
            while (Console.ReadKey(true).KeyChar != 'q') { }
            Stop = true;
        }
    }
}
