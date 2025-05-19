namespace Playground.Projects
{
    public abstract class Project()
    {
        public virtual void Start() { }
        public virtual void Update(float delta) { }
        public virtual void Finish() { }

        public bool Stop { get; set; }
    }
}
