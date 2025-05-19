#pragma warning disable IDE0011
using System.Text;

namespace Playground.Display
{
    public static class ColorScreen
    {
        private static readonly Lock s_syncObject = new();

        private static Color[]? s_buffer;
        private static float[]? s_zbuffer;

        private static int? s_width;
        private static int? s_height;

        public static int Width => s_width ?? 0;
        public static int Height => s_height ?? 0;

        public static Color[] Buffer
        {
            get
            {
                lock (s_syncObject)
                {
                    if (s_buffer == null)
                    {
                        s_width = Console.WindowWidth;
                        s_height = Console.WindowHeight * 2;

                        int size = Width * Height;

                        s_buffer = new Color[size];
                        Array.Fill(s_buffer, new(0, 0, 0));
                    }
                    return s_buffer;
                }
            }
            set
            {
                lock (s_syncObject)
                {
                    s_buffer = value;
                }
            }
        }

        public static float[] ZBuffer
        {
            get
            {
                lock (s_syncObject)
                {
                    if (s_zbuffer == null)
                    {
                        s_zbuffer = new float[Buffer.Length];
                        Array.Fill(s_zbuffer, float.MinValue);
                    }
                    return s_zbuffer;
                }
            }
        }

        public static void SetPixel(int x, int y, byte r, byte g, byte b, float depth)
        {
            SetPixel(x, y, new(r, g, b), depth);
        }

        public static void SetPixel(int x, int y, Color color, float depth)
        {
            int index = (y * Width) + x;
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            SetPixel(index, color, depth);
        }

        public static void SetPixel(int index, Color color, float depth)
        {
            if (index < 0 && index >= Height * Width) return;
            if (ZBuffer[index] >= depth) return;
            Buffer[index] = color;
            ZBuffer[index] = depth;
        }

        public static void Start()
        {
            Console.Clear();
            Console.CursorVisible = false;
        }

        public static void Clear()
        {
            lock (s_syncObject)
            {
                Array.Fill(Buffer, new(0, 0, 0));
                Array.Fill(ZBuffer, float.MinValue);
            }
        }

        public static void Flush()
        {
            Color[] localBuffer;
            lock (s_syncObject)
            {
                localBuffer = Buffer;
            }

            StringBuilder sb = new();

            for (int y = 0; y < Height / 2; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index_t = (2 * y * Width) + x;
                    int index_b = index_t + Width;

                    Color top = localBuffer[index_t];
                    Color btm = localBuffer[index_b];

                    sb = sb.Append("\x1b[38;2;")
                        .Append(top.R).Append(';').Append(top.G).Append(';').Append(top.B)
                        .Append("m\x1b[48;2;")
                        .Append(btm.R).Append(';').Append(btm.G).Append(';').Append(btm.B)
                        .Append("m▀");
                }
            }

            sb = sb.Append("\x1b[0m");

            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
            Console.SetCursorPosition(0, 0);
        }
    }

    public readonly struct Color(byte r, byte g, byte b)
    {
        public byte R { get; } = r;
        public byte G { get; } = g;
        public byte B { get; } = b;
    }
}
