using System.Numerics;
using System.Text;
#pragma warning disable IDE0011
namespace Playground.Drawing
{
    public class ColorDisplay
    {
        private readonly Lock _lock = new();

        public Color[] Buffer { get; private set; }
        public float[] ZBuffer { get; private set; }

        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;

        public Vector2 Size { get; }
        public Vector2 Position { get; }


        public ColorDisplay(int width, int height, int x = 0, int y = 0) :
            this(new Vector2(width, height), new Vector2(x, y))
        { }

        public ColorDisplay(Vector2 size, Vector2 position)
        {
            lock (_lock)
            {
                Size = size;
                Position = position;

                Buffer = new Color[Width * Height];
                Array.Fill(Buffer, new(0, 0, 0));

                ZBuffer = new float[Width * Height];
                Array.Fill(ZBuffer, float.MinValue);

                Console.CursorVisible = false;
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                StringBuilder sb = new();

                int renderWidth = Math.Min(Width, Console.WindowWidth - (int)Position.X);
                int renderHeight = Math.Min(Height, Console.WindowHeight - (int)Position.Y);

                int rows = (renderHeight + 1) / 2;

                for (int y = 0; y < rows; y++)
                {
                    sb = sb.Clear();

                    Color color_t = new(0, 0, 0);
                    Color color_b = new(0, 0, 0);

                    sb = sb.Append("\x1b[38;2;0;0;0m\x1b[48;2;0;0;0m");

                    for (int x = 0; x < renderWidth; x++)
                    {
                        int index_t = (2 * y * Width) + x;
                        int index_b = index_t + Width;

                        Color top = Buffer[index_t];
                        Color btm = Buffer[index_b];

                        if (color_t != top)
                        {
                            sb = sb.Append("\x1b[38;2;")
                                .Append(top.R).Append(';')
                                .Append(top.G).Append(';')
                                .Append(top.B).Append('m');
                            color_t = top;
                        }

                        if (color_b != btm)
                        {
                            sb = sb.Append("\x1b[48;2;")
                                .Append(btm.R).Append(';')
                                .Append(btm.G).Append(';')
                                .Append(btm.B).Append('m');
                            color_b = btm;
                        }

                        sb = sb.Append('▀');
                    }

                    sb = sb.Append("\x1b[0m");

                    Console.SetCursorPosition((int)Position.X, (int)(Position.Y + y));
                    Console.Write(sb.ToString());
                }


                Console.SetCursorPosition(0, 0);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Buffer = new Color[Width * Height];
                Array.Fill(Buffer, new(0, 0, 0));

                ZBuffer = new float[Width * Height];
                Array.Fill(ZBuffer, float.MinValue);
            }
        }

        public void Fill(Color color)
        {
            lock (_lock)
            {
                Array.Fill(Buffer, color);
            }
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, float depth)
        {
            SetPixel(x, y, new(r, g, b), depth);
        }

        public void SetPixel(int x, int y, Color color, float depth)
        {
            lock (_lock)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height) return;
                int index = (y * Width) + x;
                SetPixel(index, color, depth);
            }
        }

        public void SetPixel(int index, Color color, float depth)
        {
            lock (_lock)
            {
                if (index < 0 || index >= Height * Width) return;
                if (ZBuffer[index] >= depth) return;
                Buffer[index] = color;
                ZBuffer[index] = depth;
            }
        }
    }
}
