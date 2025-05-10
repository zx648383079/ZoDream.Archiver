namespace UnityEngine
{
    public abstract class Object
    {
        public string? Name { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"[{GetType().Name}]{Name}";
            }
            return base.ToString();
        }
    }
}
