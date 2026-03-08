using System;
using System.Numerics;

namespace ZoDream.Shared.Bundle.Converters
{
    public class Vector2Converter : BundleConverter<Vector2>
    {
        public override Vector2 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector2();
        }
    }

    public class Vector3Converter : BundleConverter<Vector3>
    {
        public override Vector3 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector3();
        }
    }

    public class Vector4Converter : BundleConverter<Vector4>
    {
        public override Vector4 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector4();
        }
    }

    public class QuaternionConverter : BundleConverter<Quaternion>
    {
        public override Quaternion Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadQuaternion();
        }
    }

    public class MatrixConverter : BundleConverter<Matrix4x4>
    {
        public override Matrix4x4 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadMatrix();
        }
    }
}
