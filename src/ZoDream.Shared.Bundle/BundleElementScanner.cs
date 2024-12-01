namespace ZoDream.Shared.Bundle
{
    public class BundleElementScanner : IBundleElementScanner
    {
        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is IElementLoader r)
            {
                r.Read(reader);
                return true;
            }
            return false;
        }
    }
}
