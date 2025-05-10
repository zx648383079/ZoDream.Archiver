using Mono.Cecil;

namespace ZoDream.BundleExtractor.Unity.Document.Cecil
{
    internal static class TypeDefinitionExtensions
    {
        public static bool IsSubclassOf(this TypeDefinition type, string baseTypeName)
        {
            var baseType = type.BaseType;
            if (baseType == null)
                return false;
            if (baseType.FullName == baseTypeName)
                return true;

            var baseTypeDef = baseType.Resolve();
            if (baseTypeDef == null)
                return false;

            return baseTypeDef.IsSubclassOf(baseTypeName);
        }

        public static bool IsSubclassOf(this TypeDefinition type, params string[] baseTypeNames)
        {
            var baseType = type.BaseType;
            if (baseType == null)
                return false;

            for (int i = 0; i < baseTypeNames.Length; i++)
                if (baseType.FullName == baseTypeNames[i])
                    return true;

            var baseTypeDef = baseType.Resolve();
            if (baseTypeDef == null)
                return false;

            return baseTypeDef.IsSubclassOf(baseTypeNames);
        }
    }
}
