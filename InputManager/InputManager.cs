namespace Playground.InputManager
{
    public static class InputManager
    {
        public static bool IsRunning { get; private set; }
        internal static Task? ReadingTask;
        internal static CancellationTokenSource? Source;
        internal static CancellationToken Token => Source?.Token ?? default;

        public static void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            Source = new();
            ReadingTask = Task.Run(KeyReader, Token);
        }

        public static void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;

            Source?.Cancel();
            Source?.Dispose();
            Source = null;

            ReadingTask = null;
        }

        private static async Task KeyReader()
        {
            while (IsRunning && !(Source?.IsCancellationRequested ?? true))
            {
                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                    OnKeyPressed?.Invoke(keyInfo);
                }
                await Task.Delay(16);
            }
        }

        public static event Action<ConsoleKeyInfo>? OnKeyPressed;
    }
}
