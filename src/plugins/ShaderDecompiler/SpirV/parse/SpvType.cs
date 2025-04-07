using System.Collections.Generic;
using System.Text;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvType
    {
        public virtual void Write(ICodeWriter sb)
        {
        }
    }

    public class SpvVoidType : SpvType
    {
        public override string ToString()
        {
            return "void";
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write("void");
        }
    }

    public class SpvScalarType : SpvType
    {
    }

    public class SpvBoolType : SpvScalarType
    {
        public override string ToString()
        {
            return "bool";
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write("bool");
        }
    }

    public class SpvIntegerType : SpvScalarType
    {
        public SpvIntegerType(int width, bool signed)
        {
            Width = width;
            Signed = signed;
        }

        public override string ToString()
        {
            if (Signed)
            {
                return $"i{Width}";
            }
            else
            {
                return $"u{Width}";
            }
        }

        public override void Write(ICodeWriter sb)
        {
            if (Signed)
            {
                sb.Write('i');
            }
            else
            {
                sb.Write('u');
            }
            sb.Write(Width);
        }

        public int Width { get; }
        public bool Signed { get; }
    }

    public class SpvFloatingPointType : SpvScalarType
    {
        public SpvFloatingPointType(int width)
        {
            Width = width;
        }

        public override string ToString()
        {
            return $"f{Width}";
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write('f');
            sb.Write(Width);
        }

        public int Width { get; }
    }

    public class SpvVectorType : SpvType
    {
        public SpvVectorType(SpvScalarType scalarType, int componentCount)
        {
            ComponentType = scalarType;
            ComponentCount = componentCount;
        }

        public override string ToString()
        {
            return $"{ComponentType}_{ComponentCount}";
        }

        public override void Write(ICodeWriter sb)
        {
            ComponentType.Write(sb);
            sb.Write('_').Write(ComponentCount);
        }

        public SpvScalarType ComponentType { get; }
        public int ComponentCount { get; }
    }

    public class SpvMatrixType : SpvType
    {
        public SpvMatrixType(SpvVectorType vectorType, int columnCount)
        {
            ColumnType = vectorType;
            ColumnCount = columnCount;
        }

        public override string ToString()
        {
            return $"{ColumnType}x{ColumnCount}";
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write(ColumnType).Write('x').Write(ColumnCount);
        }

        public SpvVectorType ColumnType { get; }
        public int ColumnCount { get; }
        public int RowCount => ColumnType.ComponentCount;
    }

    public class SpvImageType : SpvType
    {
        public SpvImageType(SpvType sampledType, Dim dim, int depth, bool isArray, bool isMultisampled, int sampleCount,
            ImageFormat imageFormat, AccessQualifier accessQualifier)
        {
            SampledType = sampledType;
            Dim = dim;
            Depth = depth;
            IsArray = isArray;
            IsMultisampled = isMultisampled;
            SampleCount = sampleCount;
            Format = imageFormat;
            AccessQualifier = accessQualifier;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Write(new CodeWriter(sb));
            return sb.ToString();
        }

        public override void Write(ICodeWriter sb)
        {
            switch (AccessQualifier)
            {
                case AccessQualifier.ReadWrite:
                    sb.Write("read_write ");
                    break;
                case AccessQualifier.WriteOnly:
                    sb.Write("write_only ");
                    break;
                case AccessQualifier.ReadOnly:
                    sb.Write("read_only ");
                    break;
            }

            sb.Write("Texture");
            switch (Dim)
            {
                case Dim.Dim1D:
                    sb.Write("1D");
                    break;
                case Dim.Dim2D:
                    sb.Write("2D");
                    break;
                case Dim.Dim3D:
                    sb.Write("3D");
                    break;
                case Dim.Cube:
                    sb.Write("Cube");
                    break;
            }

            if (IsMultisampled)
            {
                sb.Write("MS");
            }
            if (IsArray)
            {
                sb.Write("Array");
            }
        }

        public SpvType SampledType { get; }
        public Dim Dim { get; }
        public int Depth { get; }
        public bool IsArray { get; }
        public bool IsMultisampled { get; }
        public int SampleCount { get; }
        public ImageFormat Format { get; }
        public AccessQualifier AccessQualifier { get; }
    }

    public class SpvSamplerType : SpvType
    {
        public override string ToString()
        {
            return "sampler";
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write("sampler");
        }
    }

    public class SpvSampledImageType : SpvType
    {
        public SpvSampledImageType(SpvImageType imageType)
        {
            ImageType = imageType;
        }

        public override string ToString()
        {
            return $"{ImageType}Sampled";
        }

        public override void Write(ICodeWriter sb)
        {
            ImageType.Write(sb);
            sb.Write("Sampled");
        }

        public SpvImageType ImageType { get; }
    }

    public class SpvArrayType : SpvType
    {
        public SpvArrayType(SpvType elementType, int elementCount)
        {
            ElementType = elementType;
            ElementCount = elementCount;
        }

        public override string ToString()
        {
            return $"{ElementType}[{ElementCount}]";
        }

        public override void Write(ICodeWriter sb)
        {
            ElementType.Write(sb);
            sb.Write('[').Write(ElementCount).Write(']');
        }

        public int ElementCount { get; }
        public SpvType ElementType { get; }
    }

    public class SpvRuntimeArrayType : SpvType
    {
        public SpvRuntimeArrayType(SpvType elementType)
        {
            ElementType = elementType;
        }

        public SpvType ElementType { get; }
    }

    public class SpvStructType : SpvType
    {
        public SpvStructType(IReadOnlyList<SpvType> memberTypes)
        {
            MemberTypes = memberTypes;
            memberNames_ = [];

            for (int i = 0; i < memberTypes.Count; ++i)
            {
                memberNames_.Add(string.Empty);
            }
        }

        public void SetMemberName(uint member, string name)
        {
            memberNames_[(int)member] = name;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Write(new CodeWriter(sb));
            return sb.ToString();
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write("struct {");
            for (int i = 0; i < MemberTypes.Count; ++i)
            {
                SpvType memberType = MemberTypes[i];
                memberType.Write(sb);
                if (!string.IsNullOrEmpty(memberNames_[i]))
                {
                    sb.Write(' ');
                    sb.Write(MemberNames[i]);
                }

                sb.Write(';');
                if (i < (MemberTypes.Count - 1))
                {
                    sb.Write(' ');
                }
            }
            sb.Write('}');
        }

        public IReadOnlyList<SpvType> MemberTypes { get; }
        public IReadOnlyList<string> MemberNames => memberNames_;

        private readonly List<string> memberNames_;
    }

    public class SpvOpaqueType : SpvType
    {
    }

    public class SpvPointerType : SpvType
    {
        public SpvPointerType(StorageClass storageClass, SpvType type)
        {
            StorageClass = storageClass;
            Type = type;
        }

        public SpvPointerType(StorageClass storageClass)
        {
            StorageClass = storageClass;
        }

        public void ResolveForwardReference(SpvType t)
        {
            Type = t;
        }

        public override string ToString()
        {
            if (Type == null)
            {
                return $"{StorageClass} *";
            }
            else
            {
                return $"{StorageClass} {Type}*";
            }
        }

        public override void Write(ICodeWriter sb)
        {
            sb.Write(StorageClass.ToString());
            sb.Write(' ');
            Type?.Write(sb);
            sb.Write('*');
        }

        public StorageClass StorageClass { get; }
        public SpvType? Type { get; private set; }
    }

    public class SpvFunctionType : SpvType
    {
        public SpvFunctionType(SpvType returnType, IReadOnlyList<SpvType> parameterTypes)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }

        public SpvType ReturnType { get; }
        public IReadOnlyList<SpvType> ParameterTypes { get; }
    }

    public class SpvEventType : SpvType
    {
    }

    public class SpvDeviceEventType : SpvType
    {
    }

    public class SpvReserveIdType : SpvType
    {
    }

    public class SpvQueueType : SpvType
    {
    }

    public class SpvPipeType : SpvType
    {
    }

    public class SpvPipeStorage : SpvType
    {
    }

    public class SpvNamedBarrier : SpvType
    {
    }
}
