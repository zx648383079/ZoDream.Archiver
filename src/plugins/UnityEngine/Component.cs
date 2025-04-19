namespace UnityEngine
{
    public abstract class Component : Object
    {

        public PPtr<GameObject>? GameObject { get; set; }

        public PPtr<Transform>? Transform { get; set; }

        public string? Tag { get; set; }
    }
}
