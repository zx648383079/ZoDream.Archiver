using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.Shared.Numerics;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace ZoDream.KhronosExporter.Converters
{
    internal class VectorConverter<T> : JsonConverter<T>
        where T : struct
    {

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var cvt = options.GetConverter(typeof(float[])) as JsonConverter<float[]>;
            var data = cvt?.Read(ref reader, typeToConvert, options);
            object res;
            if (typeToConvert == typeof(Vector2))
            {
                res = data?.Length == 2 ? new Vector2(data) : Vector2.Zero;
            } else if (typeToConvert == typeof(Vector3))
            {
                res = data?.Length == 3 ? new Vector3(data) : Vector3.Zero;
            }
            else if (typeToConvert == typeof(Vector4))
            {
                res = data?.Length == 4 ? new Vector4(data) : Vector4.Zero;
            }
            else if (typeToConvert == typeof(Quaternion))
            {
                res = data?.Length == 4 ? new Quaternion(data[0], data[1], data[2], data[3]) : Quaternion.Zero;
            }
            else if (typeToConvert == typeof(Matrix4x4))
            {
                var mat = new Matrix4x4();
                if (data?.Length == 16)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            mat[i, j] = data[i * 4 + j];
                        }
                    }
                }
                res = mat;
            } else
            {
                res = null;
            }
            return (T)res;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            float[] data;
            switch(value)
            {
                case Vector2 vec:
                    data = vec.AsArray();
                    break;
                case Vector3 vec:
                    data = vec.AsArray();
                    break;
                case Vector4 vec:
                    data = vec.AsArray();
                    break;
                case Quaternion vec:
                    data = vec.AsArray();
                    break;
                case Matrix4x4 mat:
                    data = new float[16];
                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            data[i * 4 + j] = mat[i, j];
                        }
                    }
                    break;
                default:
                    data = [];
                    break;
            }
            var cvt = options.GetConverter(typeof(float[])) as JsonConverter<float[]>;
            cvt?.Write(writer, data, options);
        }
    }
}
