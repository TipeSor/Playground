using System.Numerics;
#pragma warning disable CA1822
namespace Playground.Projects
{
    public class Rings : Project
    {
        private readonly char[] map = ['.', ',', '-', '~', ':', ';', '=', '!', '*', '#', '$', '@'];
        private readonly Dictionary<(int, int), float> zbuffer = [];
        private readonly Vector3[] rotation = new Vector3[9];

        private Vector3 Size = new(30, 30, 1);
        private Vector3 Offset = new(30, 30, 30);

        private readonly float Speed = 0.001f;

        public override void Start()
        {
            // Console.Console.Start();

            rotation[0] = new(0, 0, 0);
            rotation[1] = new(0, 0, 0);
            rotation[2] = new(0, 0, 0);
            rotation[3] = new(0, 0, 0);
            rotation[4] = new(0, 0, 0);
            rotation[5] = new(0, 0, 0);
            rotation[6] = new(0, 0, 0);
            rotation[7] = new(0, 0, 0);
            rotation[8] = new(0, 0, 0);

        }

        public override void Update(float delta)
        {

            while (true)
            {
                // Console.Console.Clear();
                System.Console.SetCursorPosition(0, 0);

                DrawDonut(Size / 1.000f, Offset, rotation[0], zbuffer);
                DrawDonut(Size / 1.500f, Offset, rotation[1], zbuffer);
                DrawDonut(Size / 2.000f, Offset, rotation[2], zbuffer);
                // DrawDonut(Size / 2.500f, Offset, rotation[3], zbuffer);
                // DrawDonut(Size / 3.000f, Offset, rotation[4], zbuffer);
                // DrawDonut(Size / 3.500f, Offset, rotation[5], zbuffer);
                // DrawDonut(Size / 4.000f, Offset, rotation[6], zbuffer);
                // DrawDonut(Size / 4.500f, Offset, rotation[7], zbuffer);
                // DrawDonut(Size / 5.000f, Offset, rotation[8], zbuffer);

                rotation[0] += new Vector3(0.00f, 0.01f, 0.04f) * delta * Speed;
                rotation[1] += new Vector3(0.01f, 0.02f, 0.05f) * delta * Speed;
                rotation[2] += new Vector3(0.02f, 0.03f, 0.06f) * delta * Speed;
                rotation[3] += new Vector3(0.03f, 0.04f, 0.07f) * delta * Speed;
                rotation[4] += new Vector3(0.04f, 0.05f, 0.08f) * delta * Speed;
                rotation[5] += new Vector3(0.05f, 0.06f, 0.09f) * delta * Speed;
                rotation[6] += new Vector3(0.06f, 0.07f, 0.11f) * delta * Speed;
                rotation[7] += new Vector3(0.07f, 0.08f, 0.12f) * delta * Speed;
                rotation[8] += new Vector3(0.08f, 0.09f, 0.13f) * delta * Speed;

                zbuffer.Clear();

                // Console.Console.Flush();
            }
        }

#pragma warning disable IDE0055
        public Vector3 Rotate(Vector3 vec, float yaw, float pitch, float roll)
        {
            return Roll(Pitch(Yaw(vec, yaw), pitch), roll);
        }

        public Vector3 Yaw(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X = (vec.X * MathF.Cos(rot)) - (vec.Y * MathF.Sin(rot)) + (vec.Z * 0),
                Y = (vec.X * MathF.Sin(rot)) + (vec.Y * MathF.Cos(rot)) - (vec.Z * 0),
                Z = (vec.X * 0)              + (vec.Y * 0)              + (vec.Z * 1)
            };
            return vec2;
        }

        public Vector3 Pitch(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X =  (vec.X * MathF.Cos(rot)) + (vec.Y * 0) + (vec.Z * MathF.Sin(rot)),
                Y =  (vec.X * 0)              + (vec.Y * 1) + (vec.Z * 0),
                Z = -(vec.X * MathF.Sin(rot)) + (vec.Y * 0) + (vec.Z * MathF.Cos(rot))
            };
            return vec2;
        }

        public Vector3 Roll(Vector3 vec, float rot)
        {
            Vector3 vec2 = new(0, 0, 0)
            {
                X = (vec.X * 1) + (vec.Y * 0)              + (vec.Z * 0),
                Y = (vec.X * 0) + (vec.Y * MathF.Cos(rot)) - (vec.Z * MathF.Sin(rot)),
                Z = (vec.X * 0) + (vec.Y * MathF.Sin(rot)) + (vec.Z * MathF.Cos(rot))
            };
            return vec2;
        }
#pragma warning restore IDE0055

        public void DrawDonut(Vector3 size, Vector3 offset, Vector3 rotation, Dictionary<(int, int), float> zbuffer)
        {
            for (float i = 0; i < 6.24; i += 0.01f)
            {
                Vector3 pos = new(0, 0, 0)
                {
                    X = MathF.Cos(i) * size.X,
                    Y = MathF.Sin(i) * size.Y,
                    Z = 0 * size.Z,
                };

                pos = Rotate(pos, rotation.X, rotation.Y, rotation.Z);

                // Console.Console.Write(0, 0, $"{pos.X:F2} {pos.Y:F2} {pos.Z:F2}");
                pos += offset;
                // Console.Console.Write(0, 1, $"{pos.X:F2} {pos.Y:F2} {pos.Z:F2}");

                pos.Z += 10;

                float zScaled = pos.Z / offset.Z / 2;
                int index = (int)System.Math.Round(zScaled * (map.Length - 1), 0);
                char symbol = map[System.Math.Clamp(index, 0, map.Length - 1)];

                int x = (int)System.Math.Round(pos.X * 2, 0);
                int y = (int)System.Math.Round(pos.Y, 0);

                if (!zbuffer.TryGetValue((x, y), out float z))
                {
                    zbuffer[(x, y)] = pos.Z;
                }

                if (pos.Z > z)
                {
                    // Console.Console.SetChar(x, y, symbol);
                }
            }
        }
    }
}
