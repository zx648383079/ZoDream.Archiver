namespace UnityEngine
{
    public abstract class Component : Object
    {

        public IPPtr<GameObject>? GameObject { get; set; }

        public IPPtr<Transform>? Transform { get; set; }

        public string? Tag { get; set; }
    }
}
