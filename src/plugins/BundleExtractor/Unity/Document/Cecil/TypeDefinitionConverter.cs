using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoDream.BundleExtractor.Unity.Document.Cecil
{
    internal class TypeDefinitionConverter(TypeDefinition typeDef, DocumentBuilder builder, int indent)
    {
        private readonly TypeResolver TypeResolver = new();

        public void ConvertTo()
        {
            var baseTypes = new Stack<TypeReference>();
            var lastBaseType = typeDef.BaseType;
            while (!SerializationLogic.IsNonSerialized(lastBaseType))
            {
                if (lastBaseType is GenericInstanceType genericInstanceType)
                {
                    TypeResolver.Add(genericInstanceType);
                }
                baseTypes.Push(lastBaseType);
                lastBaseType = lastBaseType.Resolve().BaseType;
            }
            while (baseTypes.Count > 0)
            {
                var typeReference = baseTypes.Pop();
                var typeDefinition = typeReference.Resolve();
                foreach (var fieldDefinition in typeDefinition.Fields.Where(WillUnitySerialize))
                {
                    if (!IsHiddenByParentClass(baseTypes, fieldDefinition, typeDef))
                    {
                        ProcessingFieldRef(ResolveGenericFieldReference(fieldDefinition));
                    }
                }

                if (typeReference is GenericInstanceType genericInstanceType)
                {
                    TypeResolver.Remove(genericInstanceType);
                }
            }
            foreach (var field in FilteredFields())
            {
                ProcessingFieldRef(field);
            }
        }

        private bool WillUnitySerialize(FieldDefinition fieldDefinition)
        {
            try
            {
                var resolvedFieldType = TypeResolver.Resolve(fieldDefinition.FieldType);
                if (SerializationLogic.ShouldNotTryToResolve(resolvedFieldType))
                {
                    return false;
                }
                if (!EngineTypePredicates.IsUnityEngineObject(resolvedFieldType))
                {
                    if (resolvedFieldType.FullName == fieldDefinition.DeclaringType.FullName)
                    {
                        return false;
                    }
                }
                return SerializationLogic.WillUnitySerialize(fieldDefinition, TypeResolver);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception while processing {0} {1}, error {2}", fieldDefinition.FieldType.FullName, fieldDefinition.FullName, ex.Message));
            }
        }

        private static bool IsHiddenByParentClass(IEnumerable<TypeReference> parentTypes, FieldDefinition fieldDefinition, TypeDefinition processingType)
        {
            return processingType.Fields.Any(f => f.Name == fieldDefinition.Name) || parentTypes.Any(t => t.Resolve().Fields.Any(f => f.Name == fieldDefinition.Name));
        }

        private IEnumerable<FieldDefinition> FilteredFields()
        {
            return typeDef.Fields.Where(WillUnitySerialize).Where(f =>
                SerializationLogic.IsSupportedCollection(f.FieldType) ||
                !f.FieldType.IsGenericInstance ||
                SerializationLogic.ShouldImplementIDeserializable(f.FieldType.Resolve()));
        }

        private FieldReference ResolveGenericFieldReference(FieldReference fieldRef)
        {
            var field = new FieldReference(fieldRef.Name, fieldRef.FieldType, ResolveDeclaringType(fieldRef.DeclaringType));
            return typeDef.Module.ImportReference(field);
        }

        private TypeReference? ResolveDeclaringType(TypeReference declaringType)
        {
            var typeDefinition = declaringType.Resolve();
            if (typeDefinition == null || !typeDefinition.HasGenericParameters)
            {
                return typeDefinition;
            }
            var genericInstanceType = new GenericInstanceType(typeDefinition);
            foreach (var genericParameter in typeDefinition.GenericParameters)
            {
                genericInstanceType.GenericArguments.Add(genericParameter);
            }
            return TypeResolver.Resolve(genericInstanceType);
        }

        private void ProcessingFieldRef(FieldReference fieldDef)
        {
            var typeRef = TypeResolver.Resolve(fieldDef.FieldType);
            TypeRefToTypeTreeNodes(typeRef, fieldDef.Name, indent, false);
        }

        private static bool IsStruct(TypeReference typeRef)
        {
            return typeRef.IsValueType && !IsEnum(typeRef) && !typeRef.IsPrimitive;
        }

        private static bool IsEnum(TypeReference typeRef)
        {
            return !typeRef.IsArray && typeRef.Resolve().IsEnum;
        }

        private static bool RequiresAlignment(TypeReference typeRef)
        {
            return typeRef.MetadataType switch
            {
                MetadataType.Boolean or MetadataType.Char or MetadataType.SByte or MetadataType.Byte or MetadataType.Int16 or MetadataType.UInt16 => true,
                _ => SerializationLogic.IsSupportedCollection(typeRef) && RequiresAlignment(CecilUtils.ElementTypeOfCollection(typeRef)),
            };
        }

        private static bool IsSystemString(TypeReference typeRef)
        {
            return typeRef.FullName == "System.String";
        }

        private void TypeRefToTypeTreeNodes(TypeReference typeRef, string name, int indent, bool isElement)
        {
            var align = false;

            if (!IsStruct(typeDef) || !EngineTypePredicates.IsUnityEngineValueType(typeDef))
            {
                if (IsStruct(typeRef) || RequiresAlignment(typeRef))
                {
                    align = true;
                }
            }

            if (typeRef.IsPrimitive)
            {
                var primitiveName = typeRef.Name;
                primitiveName = primitiveName switch
                {
                    "Boolean" => "bool",
                    "Byte" => "UInt8",
                    "SByte" => "SInt8",
                    "Int16" => "SInt16",
                    "UInt16" => "UInt16",
                    "Int32" => "SInt32",
                    "UInt32" => "UInt32",
                    "Int64" => "SInt64",
                    "UInt64" => "UInt64",
                    "Char" => "char",
                    "Double" => "double",
                    "Single" => "float",
                    _ => throw new NotSupportedException(),
                };
                if (isElement)
                {
                    align = false;
                }
                builder.Add(new(primitiveName, name, indent, align));
            }
            else if (IsSystemString(typeRef))
            {
                builder.AddString(name, indent);
            }
            else if (IsEnum(typeRef))
            {
                builder.Add(new("SInt32", name, indent, align));
            }
            else if (CecilUtils.IsGenericList(typeRef))
            {
                var elementRef = CecilUtils.ElementTypeOfCollection(typeRef);
                builder.Add(new(typeRef.Name, name, indent, align));
                builder.AddArray(indent + 1);
                TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true);
            }
            else if (typeRef.IsArray)
            {
                var elementRef = typeRef.GetElementType();
                builder.Add(new(typeRef.Name, name, indent, align));
                builder.AddArray(indent + 1);
                TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true);
            }
            else if (EngineTypePredicates.IsUnityEngineObject(typeRef))
            {
                builder.AddPPtr(typeRef.Name, name, indent);
            }
            else if (EngineTypePredicates.IsSerializableUnityClass(typeRef) || EngineTypePredicates.IsSerializableUnityStruct(typeRef))
            {
                switch (typeRef.FullName)
                {
                    case "UnityEngine.AnimationCurve":
                        builder.AddAnimationCurve(name, indent);
                        break;
                    case "UnityEngine.Gradient":
                        builder.AddGradient(name, indent);
                        break;
                    case "UnityEngine.GUIStyle":
                        builder.AddGUIStyle(name, indent);
                        break;
                    case "UnityEngine.RectOffset":
                        builder.AddRectOffset(name, indent);
                        break;
                    case "UnityEngine.Color32":
                        builder.AddColor32(name, indent);
                        break;
                    case "UnityEngine.Matrix4x4":
                        builder.AddMatrix4x4(name, indent);
                        break;
                    case "UnityEngine.Rendering.SphericalHarmonicsL2":
                        builder.AddSphericalHarmonicsL2(name, indent);
                        break;
                    case "UnityEngine.PropertyName":
                        builder.AddPropertyName(name, indent);
                        break;
                }
            }
            else
            {
                builder.Add(new(typeRef.Name, name, indent, align));
                var typeDef = typeRef.Resolve();
                var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, builder, indent + 1);
                typeDefinitionConverter.ConvertTo();
            }
        }
    }
}