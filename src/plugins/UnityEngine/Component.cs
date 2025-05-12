namespace UnityEngine
{
    public abstract class Component : Object
    {

        public IPPtr<GameObject>? GameObject { get; set; }

    }
}
