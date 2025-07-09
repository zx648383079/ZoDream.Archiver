using System.IO;
using System.Text;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class MeshExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not Mesh res)
            {
                return;
            }
            if (res.VertexCount < 0 || res.Vertices == null || res.Vertices.Length == 0)
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".obj", mode, out fileName))
            {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("g " + res.Name);
            #region Vertices
            int c = 3;
            if (res.Vertices.Length == res.VertexCount * 4)
            {
                c = 4;
            }
            for (int v = 0; v < res.VertexCount; v++)
            {
                sb.AppendFormat("v {0} {1} {2}\r\n", -res.Vertices[v * c], res.Vertices[v * c + 1], res.Vertices[v * c + 2]);
            }
            #endregion

            #region UV
            if (res.UV0?.Length > 0)
            {
                c = 4;
                if (res.UV0.Length == res.VertexCount * 2)
                {
                    c = 2;
                }
                else if (res.UV0.Length == res.VertexCount * 3)
                {
                    c = 3;
                }
                for (int v = 0; v < res.VertexCount; v++)
                {
                    sb.AppendFormat("vt {0} {1}\r\n", res.UV0[v * c], res.UV0[v * c + 1]);
                }
            }
            #endregion

            #region Normals
            if (res.Normals?.Length > 0)
            {
                if (res.Normals.Length == res.VertexCount * 3)
                {
                    c = 3;
                }
                else if (res.Normals.Length == res.VertexCount * 4)
                {
                    c = 4;
                }
                for (int v = 0; v < res.VertexCount; v++)
                {
                    sb.AppendFormat("vn {0} {1} {2}\r\n", -res.Normals[v * c], res.Normals[v * c + 1], res.Normals[v * c + 2]);
                }
            }
            #endregion

            #region Face
            int sum = 0;
            for (var i = 0; i < res.SubMeshes.Length; i++)
            {
                sb.AppendLine($"g {res.Name}_{i}");
                int indexCount = (int)res.SubMeshes[i].IndexCount;
                var end = sum + indexCount / 3;
                for (int f = sum; f < end; f++)
                {
                    sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\r\n", res.Indices[f * 3 + 2] + 1, res.Indices[f * 3 + 1] + 1, res.Indices[f * 3] + 1);
                }
                sum = end;
            }
            #endregion

            sb.Replace("NaN", "0");
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}
