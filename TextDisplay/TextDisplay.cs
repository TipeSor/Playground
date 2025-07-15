using System.Numerics;
#pragma warning disable CA1716
namespace Playground.Drawing
{
    public class TextDisplay(Vector2 size, Vector2? position = null)
    {
        public TextDisplay(float width, float height, float x = 0, float y = 0) :
            this(new Vector2(width, height), new Vector2(x, y))
        { }

        public Vector2 Position { get; private set; } = position ?? Vector2.Zero;
        public Vector2 Size { get; private set; } = size;
        public ITextObject? Active { get; private set; }

        public void SetActive(ITextObject textObject)
        {
            Active?.SetActive(false);
            textObject.SetActive(true);
            Active = textObject;
        }

        public void Draw()
        {
            Console.SetCursorPosition((int)Position.X, (int)Position.Y);
            Console.Write($"╔{Active?.Name}{new string('═', (int)(Size.X - 2 - Active?.Name.Length ?? 0))}╗");
            for (int i = 1; i < Size.Y - 1; i++)
            {
                Console.SetCursorPosition((int)Position.X, (int)Position.Y + i);
                Console.Write($"║{new string(' ', (int)(Size.X - 2))}║");
            }
            Console.SetCursorPosition((int)Position.X, (int)(Position.Y + Size.Y - 1));
            Console.Write($"╚{new string('═', (int)(Size.X - 2))}╝");

            Active?.Draw(Size, Position);
        }

        public void Update()
        {
            Active?.Update();
        }
    }

    public interface ITextObject
    {
        string Name { get; }
        bool Active { get; }

        void SetActive(bool value);
        void Draw(Vector2 size, Vector2 position);
        void Update();

        void Next(int sign = 1);
        void Accept();
    }
}
