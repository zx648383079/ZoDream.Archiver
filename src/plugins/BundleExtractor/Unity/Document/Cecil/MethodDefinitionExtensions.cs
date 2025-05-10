﻿using Mono.Cecil;

namespace ZoDream.BundleExtractor.Unity.Document.Cecil
{
    internal static class MethodDefinitionExtensions
    {
        public static bool SameAs(this MethodDefinition self, MethodDefinition other)
        {
            // FIXME: should be able to compare MethodDefinition references directly
            return self.FullName == other.FullName;
        }

        public static string PropertyName(this MethodDefinition self)
        {
            return self.Name.Substring(4);
        }

        public static bool IsConversionOperator(this MethodDefinition method)
        {
            if (!method.IsSpecialName)
                return false;

            return method.Name == "op_Implicit" || method.Name == "op_Explicit";
        }

        public static bool IsSimpleSetter(this MethodDefinition original)
        {
            return original.IsSetter && original.Parameters.Count == 1;
        }

        public static bool IsSimpleGetter(this MethodDefinition original)
        {
            return original.IsGetter && original.Parameters.Count == 0;
        }

        public static bool IsSimplePropertyAccessor(this MethodDefinition method)
        {
            return method.IsSimpleGetter() || method.IsSimpleSetter();
        }

        public static bool IsDefaultConstructor(MethodDefinition m)
        {
            return m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0;
        }
    }
}
