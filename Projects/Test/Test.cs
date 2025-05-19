namespace Playground.Projects
{
    public class Test : Project
    {
        public override void Start()
        {
            Console.WriteLine((byte)System.Math.Clamp(125005.5f, 0f, 255f));
            unchecked
            {
                Console.WriteLine((byte)256);
            }

            Stop = true;
        }
    }
}
