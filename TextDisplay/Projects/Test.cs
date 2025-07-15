using System.Numerics;
using Playground.Projects;
#pragma warning disable IDE0010, IDE0011, CA1716
namespace Playground.Drawing.Projects
{
    public class TestProject : Project
    {
        public TextDisplay Display { get; private set; } = new(64, 10, 0, 0);
        public Dictionary<string, TextMenu> Menus { get; } = new()
        {
            {"Main", new TextMenu("Main")},
            {"Fish", new TextMenu("Fish")},
            {"Shop", new TextMenu("Shop")}
        };

        private readonly object _lock = new();

        public override void Start()
        {
            Console.Clear();
            Console.CursorVisible = false;

            InputManager.InputManager.Start();
            InputManager.InputManager.OnKeyPressed += ManangeInput;

            {
                TextMenuItem item = new("Fish");
                item.OnAccept += () =>
                {
                    Display.SetActive(Menus["Fish"]);
                };
                item.SetActive(true);
                Menus["Main"].AddItem(item);
            }
            {
                TextMenuItem item = new("Shop");
                item.OnAccept += () =>
                {
                    Display.SetActive(Menus["Shop"]);
                };
                item.SetActive(true);
                Menus["Main"].AddItem(item);
            }

            {
                TextMenuItem item = new("Back");
                item.OnAccept += () =>
                {
                    Display.SetActive(Menus["Main"]);
                };
                item.SetActive(true);
                Menus["Fish"].AddItem(item);
                Menus["Shop"].AddItem(item);
            }

            Display.SetActive(Menus["Main"]);
        }

        public override void Update(long delta)
        {
            Display.Update();
            lock (_lock)
            {
                Display.Draw();
            }
            Thread.Sleep(5);
        }

        public override void Finish()
        {
            base.Finish();
        }

        public void ManangeInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Q:
                    Stop = true;
                    break;
                case ConsoleKey.Enter:
                    Display.Active?.Accept();
                    break;
                case ConsoleKey.UpArrow:
                    Display.Active?.Next(-1);
                    break;
                case ConsoleKey.DownArrow:
                    Display.Active?.Next();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Text menu
    /// </summary>
    public class TextMenu(string name) : ITextObject
    {
        public bool Active { get; private set; }
        public int Selected { get; private set; }
        public string Name { get; private set; } = name;

        private readonly List<TextMenuItem> items = [];

        public void AddItem(TextMenuItem item) { items.Add(item); }
        public bool RemoveItem(TextMenuItem item) { return items.Remove(item); }
        public TextMenuItem[] GetItems() { return [.. items]; }

        public void Draw(Vector2 size, Vector2 position)
        {
            if (!Active) return;
            position += new Vector2(2, 1);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Draw(size, position, i == Selected);
                position = new Vector2(position.X, position.Y + 1);
            }
        }

        public void SetActive(bool value)
        {
            Active = value;
            if (Active) Selected = 0;
        }

        public void Update() { }

        public void Next(int sign = 1)
        {
            if (!Active || items.Count == 0) return;

            int NextSelected = Selected + Math.Sign(sign);
            NextSelected = Math.Clamp(NextSelected, 0, items.Count - 1);

            if (Selected != NextSelected)
            {
                items[Selected].Deselect();
                items[NextSelected].Select();
                Selected = NextSelected;
            }
        }

        public void Accept()
        {
            _ = items[Selected].Accept();
        }
    }

    /// <summary>
    /// Text menu item
    /// </summary>
    public class TextMenuItem(string name) : ITextObject
    {
        public bool Active { get; private set; }
        public string Name { get; private set; } = name;

        public void Draw(Vector2 size, Vector2 position) { Draw(size, position, false); }

        public void Draw(Vector2 size, Vector2 position, bool selected = false)
        {
            if (!Active) return;
            string str = $"{(selected ? "> " : "")}{Name}{(selected ? "" : "  ")}";
            int len = (int)Math.Clamp(size.X - position.X, 0, str.Length);
            string text = str[..len];

            Console.SetCursorPosition((int)position.X, (int)position.Y);
            Console.Write(text);
        }

        public void SetActive(bool value)
        {
            Active = value;
        }

        public void Update() { }

        public void Select() { }

        public void Deselect() { }

        public bool Accept()
        {
            OnAccept?.Invoke();
            return false;
        }

        public void Next(int sign)
        {
            throw new NotImplementedException();
        }

        void ITextObject.Accept()
        {
            throw new NotImplementedException();
        }

        public event Action? OnAccept;
    }
}
