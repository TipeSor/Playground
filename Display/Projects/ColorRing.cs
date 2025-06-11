using System.Numerics;
using Playground.Projects;

namespace Playground.Drawing.Projects
{
    public class ColorRing : Project
    {
        private readonly Vector3 Size = new(16, 16, 1);
        private readonly Vector3 Offset = new(31, 31, 31);
        private readonly Quaternion[] Rotation = new Quaternion[3];

        private readonly ColorDisplay Display = new(64, 64);

        private readonly float Speed = 0.0001f;

        public override void Start()
        {
            Console.Clear();
            new Thread(QuitThread).Start();

            Rotation[0] = Quaternion.Identity;
            Rotation[1] = Quaternion.Identity;
            Rotation[2] = Quaternion.Identity;
        }

        public override void Update(long delta)
        {
            Display.Clear();

            {
                Vector3 rotation = new Vector3(1f, -2f, 3f) * delta * Speed;
                Quaternion deltaRot = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                Rotation[0] = Quaternion.Normalize(Rotation[0] * deltaRot);
            }
            {
                Vector3 rotation = new Vector3(-4f, 5f, 6f) * delta * Speed;
                Quaternion deltaRot = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                Rotation[1] = Quaternion.Normalize(Rotation[2] * deltaRot);
            }
            {
                Vector3 rotation = new Vector3(7f, 8f, -9f) * delta * Speed;
                Quaternion deltaRot = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                Rotation[2] = Quaternion.Normalize(Rotation[2] * deltaRot);
            }

            DrawRing(Size / 1, Offset, Rotation[0], new(255, 0, 0));
            DrawRing(Size / 2, Offset, Rotation[1], new(0, 255, 0));
            DrawRing(Size / 3, Offset, Rotation[2], new(0, 0, 255));

            Display.Flush();
        }

        public override void Finish()
        {
            Console.Clear();
        }

        public void DrawRing(Vector3 size, Vector3 offset, Quaternion rotation, Color color)
        {
            for (int i = 0; i < 628; i++)
            {
                Vector3 pos = new(0, 0, 0)
                {
                    X = MathF.Cos(i / 100f) * size.X,
                    Y = MathF.Sin(i / 100f) * size.Y,
                    Z = 0 * size.Z,
                };

                pos = Vector3.Transform(pos, rotation);

                pos += offset;

                float depth = Math.Clamp(pos.Z / 60f, 0, 1);

                int x = (int)Math.Round(pos.X, 0);
                int y = (int)Math.Round(pos.Y, 0);

                Color _color = new(
                    (byte)Math.Clamp(color.R * depth, 1, 255),
                    (byte)Math.Clamp(color.G * depth, 1, 255),
                    (byte)Math.Clamp(color.B * depth, 1, 255)
                );

                Display.SetPixel(x, y, _color, pos.Z);
            }
        }

        public void QuitThread()
        {
            while (Console.ReadKey(true).KeyChar != 'q') { }
            Stop = true;
        }
    }
}
