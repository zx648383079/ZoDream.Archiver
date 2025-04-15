﻿using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.Exporters.Cecil
{
    internal static class TypeReferenceExtensions
    {
        public static string SafeNamespace(this TypeReference type)
        {
            if (type.IsGenericInstance)
                return ((GenericInstanceType)type).ElementType.SafeNamespace();
            if (type.IsNested)
                return type.DeclaringType.SafeNamespace();
            return type.Namespace;
        }

        public static bool IsAssignableTo(this TypeReference typeRef, string typeName)
        {
            try
            {
                if (typeRef.IsGenericInstance)
                    return ElementType.For(typeRef).IsAssignableTo(typeName);

                if (typeRef.FullName == typeName)
                    return true;

                return typeRef.CheckedResolve().IsSubclassOf(typeName);
            }
            catch (AssemblyResolutionException) // If we can't resolve our typeref or one of its base types,
            {                                   // let's assume it is not assignable to our target type
                return false;
            }
        }

        public static bool IsEnum(this TypeReference type)
        {
            return type.IsValueType && !type.IsPrimitive && type.CheckedResolve().IsEnum;
        }

        public static bool IsStruct(this TypeReference type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum() && !IsSystemDecimal(type);
        }

        private static bool IsSystemDecimal(TypeReference type)
        {
            return type.FullName == "System.Decimal";
        }
    }
}
