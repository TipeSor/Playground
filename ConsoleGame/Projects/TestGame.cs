using System.Numerics;
using Playground.Drawing;
using Playground.Projects;
namespace Playground.ConsoleGame.Projects
{
    public class TestGame : Project
    {
        internal Vector2 OldPosition { get; private set; } = Vector2.Zero;
        internal Vector2 Position { get; private set; } = Vector2.Zero;

        internal Color BackgroundColor = new(50, 50, 50);
        internal Color PlayerColor = new(255, 0, 255);

        internal object _drawLock = new();

        internal ColorDisplay Display { get; } = new ColorDisplay(32, 32);

        public override void Start()
        {
            InputManager.InputManager.Start();
            InputManager.InputManager.OnKeyPressed += HandleInput;

            Console.Clear();
            Display.Clear();
        }

        public override void Update(long delta)
        {
            lock (_drawLock)
            {
                Display.Clear(BackgroundColor);

                Display.SetPixel((int)Position.X, (int)Position.Y, PlayerColor, 0);

                Display.Flush();

                Write(0, 17, Position.ToString());
            }

            Thread.Sleep(16);
        }

        public override void Finish()
        {
            InputManager.InputManager.Stop();
            Console.Clear();
        }

        public void Move(Vector2 delta)
        {
            Vector2 newPosition = Position + delta;
            if (newPosition.X < 0 || newPosition.X >= Display.Width ||
                newPosition.Y < 0 || newPosition.Y >= Display.Height)
                return;

            OldPosition = Position;
            Position = newPosition;
        }

        #pragma warning disable IDE0011, IDE0055
        public void HandleInput(ConsoleKeyInfo input)
        {
            if (input.Key is ConsoleKey.W or ConsoleKey.UpArrow)    Move(new Vector2( 0, -1));
            if (input.Key is ConsoleKey.S or ConsoleKey.DownArrow)  Move(new Vector2( 0,  1));
            if (input.Key is ConsoleKey.A or ConsoleKey.LeftArrow)  Move(new Vector2(-1,  0));
            if (input.Key is ConsoleKey.D or ConsoleKey.RightArrow) Move(new Vector2( 1,  0));

            Write(0, 18, $"Last pressed:");
            Write(4, 19, $"KeyCode: ConsoleKey.{input.Key}");
            Write(4, 20, $"Char: {input.KeyChar}");

            if (input.Key is ConsoleKey.Escape) Stop = true;
        }
        #pragma warning restore IDE0011, IDE0055

        public void Write(int left, int top, string text)
        {
            lock (_drawLock)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(new string(' ', Console.WindowWidth - left));
                Console.SetCursorPosition(left, top);
                Console.Write(text);
            }
        }
    }
}
