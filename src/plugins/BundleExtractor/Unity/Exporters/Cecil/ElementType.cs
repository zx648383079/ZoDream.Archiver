using Mono.Cecil;
using System;

namespace ZoDream.BundleExtractor.Unity.Exporters.Cecil
{
    internal static class ElementType
    {
        public static TypeReference For(TypeReference byRefType)
        {
            var refType = byRefType as TypeSpecification;
            if (refType != null)
                return refType.ElementType;

            throw new ArgumentException(string.Format("TypeReference isn't a TypeSpecification {0} ", byRefType));
        }
    }
}
