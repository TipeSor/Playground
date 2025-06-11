namespace Playground.Projects
{
    public abstract class Project
    {
        public virtual void Start(string[] args) { Start(); }
        public virtual void Start() { }
        public virtual void Update(long delta) { Stop = true; }
        public virtual void Finish() { }

        public bool Stop { get; set; }
    }
}
