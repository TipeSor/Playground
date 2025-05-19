namespace Playground.Display
{
    public static class Screen
    {
        private static readonly Lock s_syncObject = new();
        private static char[]? s_buffer;
        private static int[]? s_zbuffer;
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static char[] Buffer
        {
            get
            {
                lock (s_syncObject)
                {
                    if (s_buffer == null) { CreateBuffer(); }
                    return s_buffer!;
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

        private static void CreateBuffer()
        {
            Width = System.Console.WindowWidth;
            Height = System.Console.WindowHeight;
            int size = Width * Height;

            s_buffer = new char[size];
            Array.Fill(s_buffer, ' ');

            s_zbuffer = new int[size];
            Array.Fill(s_zbuffer, int.MinValue);
        }

        public static void SetChar(int x, int y, char c)
        {
            int index = (y * Width) + x;
            if (index >= 0 && index < Height * Width &&
                x >= 0 && x < Width)
            {
                Buffer[index] = c;
            }
        }

        public static void DrawChar()
        {

        }

        public static void Start()
        {
            System.Console.Clear();
            System.Console.CursorVisible = false;
        }

        public static void Clear()
        {
            lock (s_syncObject)
            {
                Array.Fill(Buffer, ' ');
            }
        }

        public static void Flush()
        {
            lock (s_syncObject)
            {
                System.Console.SetCursorPosition(0, 0);
                System.Console.Write(Buffer);
                System.Console.SetCursorPosition(0, 0);
            }
        }

        public static void Write(int x, int y, string text)
        {
            char[] chars = text.ToCharArray();
            int index = (y * Width) + x;
            if (index >= 0 && index < Height * Width)
            {
                int charsLen = System.Math.Min(chars.Length, Width - x);
                chars = chars[..charsLen];
                chars.CopyTo(Buffer, index);
            }
        }
    }
}

