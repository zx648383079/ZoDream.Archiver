using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter
{
    public partial class ObjReader
    {
        public const string TagComment = "#";
        public const string TagMtlLib = "mtllib";
        public const string TagVertex = "v";
        public const string TagVectorNormal = "vn";
        public const string TagVectorTexture = "vt";
        public const string TagGroup = "g";
        public const string TagUseMaterial = "usemtl";
        public const string TagFace = "f";

        private bool _removeDegenerateFaces = false;

        internal ObjModelRoot ReadObj(Stream input)
        {
            using var reader = new StreamReader(input);
            var model = new ObjModelRoot();
            model.Materials.Add(new() { Ambient = new(new ObjFactorColor(1)) });
            var currentMaterialName = "default";
            var currentGeometries = model.GetOrAddGeometries("default").ToArray();
            ObjFace? currentFace = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (line.StartsWith(TagComment))
                {
                    continue;
                }
                if (StartWith(line, TagMtlLib))
                {
                    model.MatFilename = line[6..].Trim();
                }
                else if (StartWith(line, TagVertex))
                {
                    var vStr = line[2..].Trim();
                    var strs = SplitLine(vStr);
                    var v = new Vector3(
                        float.Parse(strs[0], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(strs[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(strs[2], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat));
                    model.Vertices.Add(v);
                }
                else if (StartWith(line, TagVectorNormal))
                {
                    var vnStr = line[3..].Trim();
                    var strs = SplitLine(vnStr);
                    var vn = new Vector3(
                        float.Parse(strs[0], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(strs[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(strs[2], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat));
                    model.Normals.Add(vn);
                }
                else if (StartWith(line, TagVectorTexture))
                {
                    var vtStr = line[3..].Trim();
                    var strs = SplitLine(vtStr);
                    var vt = new Vector2(
                        float.Parse(strs[0], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(strs[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat));
                    model.Uvs.Add(vt);
                }
                else if (StartWith(line, TagGroup))
                {
                    var gStr = line[2..];
                    var groupNames = SplitLine(gStr);

                    currentGeometries = model.GetOrAddGeometries(groupNames).ToArray();
                }
                else if (StartWith(line, TagUseMaterial))
                {
                    var umtl = line[7..].Trim();
                    currentMaterialName = umtl;
                }
                else if (StartWith(line, TagFace))
                {
                    var fStr = line[2..].Trim();
                    currentFace = new ObjFace(currentMaterialName);
                    var strs = SplitLine(fStr);
                    if (strs.Length < 3)
                    {
                        continue; // ignore face that has less than 3 vertices
                    }
                    if (strs.Length == 3)
                    {
                        var v1 = GetVertex(strs[0]);
                        var v2 = GetVertex(strs[1]);
                        var v3 = GetVertex(strs[2]);
                        var f = new ObjFaceTriangle(v1, v2, v3);
                        currentFace.Triangles.Add(f);
                    }
                    else if (strs.Length == 4)
                    {
                        var v1 = GetVertex(strs[0]);
                        var v2 = GetVertex(strs[1]);
                        var v3 = GetVertex(strs[2]);
                        var f = new ObjFaceTriangle(v1, v2, v3);
                        currentFace.Triangles.Add(f);
                        var v4 = GetVertex(strs[3]);
                        var ff = new ObjFaceTriangle(v1, v3, v4);
                        currentFace.Triangles.Add(ff);
                    }
                    else //if (strs.Length > 4)
                    {
                        var points = new List<Vector3>();
                        for (var i = 0; i < strs.Length; i++)
                        {
                            var vv = GetVertex(strs[i]);
                            var p = model.Vertices[vv.V - 1];
                            points.Add(p);
                        }
                        var planeAxis = ComputeProjectTo2DArguments(points);
                        if (planeAxis != null)
                        {
                            var points2D = CreateProjectPointsTo2DFunction(planeAxis, points);
                            var indices = PolygonPipeline.Triangulate(points2D, null);
                            if (indices.Length == 0)
                            {
                                // TODO:
                            }
                            for (var i = 0; i < indices.Length - 2; i += 3)
                            {
                                var vv1 = GetVertex(strs[indices[i]]);
                                var vv2 = GetVertex(strs[indices[i + 1]]);
                                var vv3 = GetVertex(strs[indices[i + 2]]);
                                var ff = new ObjFaceTriangle(vv1, vv2, vv3);
                                currentFace.Triangles.Add(ff);
                            }
                        }
                        else
                        {
                            // TODO:
                        }
                    }
                    foreach (var currentGeometry in currentGeometries)
                    {
                        currentGeometry.Faces.Add(currentFace);
                    }
                }
                else
                {
                    //var strs = SplitLine(line);
                }
            }
            if (_removeDegenerateFaces)
            {
                foreach (var geom in model.Geometries)
                {
                    foreach (var face in geom.Faces)
                    {
                        var notDegradedTriangles = new List<ObjFaceTriangle>();
                        for (int i = 0; i < face.Triangles.Count; i++)
                        {
                            var triangle = face.Triangles[i];
                            var a = model.Vertices[triangle.V1.V - 1];
                            var b = model.Vertices[triangle.V2.V - 1];
                            var c = model.Vertices[triangle.V3.V - 1];
                            var sideLengths = new List<float>() {
                                    (a - b).Length(),
                                    (b - c).Length(),
                                    (c - a).Length()
                                };
                            sideLengths.Sort();
                            if (!(sideLengths[0] + sideLengths[1] <= sideLengths[2]))
                            {
                                notDegradedTriangles.Add(triangle);
                            }
                        }
                        face.Triangles = notDegradedTriangles;
                    }
                }

            }
            return model;
        }


        private static string[] SplitLine(string line)
        {
            return line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool StartWith(string line, string str)
        {
            return line.StartsWith(str) && (line[str.Length] == ' ' || line[str.Length] == '\t');
        }

        private static ObjFaceVertex GetVertex(string vStr)
        {
            var v1Str = vStr.Split('/');
            if (v1Str.Length >= 3)
            {
                var v = int.Parse(v1Str[0]);
                var t = 0;
                if (!string.IsNullOrEmpty(v1Str[1]))
                {
                    t = int.Parse(v1Str[1]);
                }
                var n = int.Parse(v1Str[2]);
                return new ObjFaceVertex(v, t, n);
            }
            else if (v1Str.Length >= 2)
            {
                return new ObjFaceVertex(int.Parse(v1Str[0]), int.Parse(v1Str[1]), 0);
            }
            return new ObjFaceVertex(int.Parse(v1Str[0]));
        }

        internal static PlanarAxis ComputeProjectTo2DArguments(IList<Vector3> positions)
        {
            var box = OrientedBoundingBox.FromPoints(positions);

            var halfAxis = box.HalfAxis;
            var xAxis = halfAxis.Col(0);
            var yAxis = halfAxis.Col(1);
            var zAxis = halfAxis.Col(2);
            var xMag = xAxis.Length();
            var yMag = yAxis.Length();
            var zMag = zAxis.Length();
            var min = new[] { xMag, yMag, zMag }.Min();

            // If all the points are on a line return undefined because we can't draw a polygon
            if ((xMag == 0 && (yMag == 0 || zMag == 0)) || (yMag == 0 && zMag == 0))
            {
                return null;
            }

            var planeAxis1 = new Vector3();
            var planeAxis2 = new Vector3();

            if (min == yMag || min == zMag)
            {
                planeAxis1 = xAxis;
            }
            if (min == xMag)
            {
                planeAxis1 = yAxis;
            }
            else if (min == zMag)
            {
                planeAxis2 = yAxis;
            }
            if (min == xMag || min == yMag)
            {
                planeAxis2 = zAxis;
            }

            return new PlanarAxis
            {
                Center = box.Center,
                Axis1 = planeAxis1,
                Axis2 = planeAxis2
            };
        }


        private static Vector2 Project2D(Vector3 p, Vector3 center, Vector3 axis1, Vector3 axis2)
        {
            var v = p - center;
            var x = Vector3.Dot(axis1, v);
            var y = Vector3.Dot(axis2, v);

            return new Vector2(x, y);
        }

        internal static IList<Vector2> CreateProjectPointsTo2DFunction(PlanarAxis axis, IList<Vector3> positions)
        {
            var pnts = new Vector2[positions.Count];
            for (var i = 0; i < pnts.Length; i++)
            {
                pnts[i] = Project2D(positions[i], axis.Center, axis.Axis1, axis.Axis2);
            }
            return pnts;
        }
    }
}
