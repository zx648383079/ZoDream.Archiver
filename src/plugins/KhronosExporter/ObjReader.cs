using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public partial class ObjReader : IEntryReader<ModelRoot>
    {
        public async Task<ModelRoot?> ReadAsync(IStorageFileEntry entry)
        {
            var res = Read(await entry.OpenReadAsync(), entry.FullPath);
            if (res is not null)
            {
                res.FileName = entry.FullPath;
            }
            return res;
        }

        public ModelSource? Read(Stream input, string outputFileName)
        {
            var data = ReadObj(input);
            var res = new ModelSource(outputFileName, RequiresUint32Indices(data));
            res.Scenes.Add(new());
            res.Materials.AddRange(data.Materials.Select(x => ConvertMaterial(x, t => GetTextureIndex(res, t))));
            var meshes = data.Geometries.ToArray();
            var meshesLength = meshes.Length;
            for (var i = 0; i < meshesLength; i++)
            {
                var mesh = meshes[i];
                if (mesh.Faces.Count == 0)
                {
                    continue;
                }
                var meshIndex = AddMesh(res, data, mesh);
                AddNode(res, mesh.Id, meshIndex, -1);
            }

            if (res.Images.Count > 0)
            {
                res.Samplers.Add(new TextureSampler
                {
                    MagFilter = TextureInterpolationFilter.LINEAR,
                    MinFilter = TextureMipMapFilter.LINEAR_MIPMAP_NEAREST,
                    WrapS = TextureWrapMode.REPEAT,
                    WrapT = TextureWrapMode.REPEAT
                });
            }
            return res;
        }

        private static bool RequiresUint32Indices(ObjModelRoot objModel)
        {
            return objModel.Vertices.Count > 65534 || objModel.Uvs.Count > 65534 || objModel.Geometries.Any(g => g.Faces.Count > 65534);
        }

        public static bool CheckWindingCorrect(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            var ba = new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            var ca = new Vector3(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            var cross = Vector3.Cross(ba, ca);
            return Vector3.Dot(normal, cross) >= 0;
        }


        #region Materials

        /// <summary>
        /// Translate the blinn-phong model to the pbr metallic-roughness model
        /// Roughness factor is a combination of specular intensity and shininess
        /// Metallic factor is 0.0
        /// Textures are not converted for now
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static double Luminance(ObjFactorColor color)
        {
            return color.Red * 0.2125 + color.Green * 0.7154 + color.Blue * 0.0721;
        }

        private static int AddTexture(ModelRoot model, string textureFilename)
        {
            var imageIndex = model.Images.Count;
            model.Images.Add(new()
            {
                Name = textureFilename,
                Uri = textureFilename
            });

            var textureIndex = model.Textures.Count;
            model.Textures.Add(new()
            {
                Name = textureFilename,
                Source = imageIndex,
                Sampler = 0
            });
            return textureIndex;
        }



        private static Material GetDefault(string name = "default", 
            AlphaMode mode = AlphaMode.OPAQUE)
        {
            return new Material
            {
                AlphaMode = mode,
                Name = name,
                EmissiveFactor = new( 1f, 1, 1 ),
                PbrMetallicRoughness = new()
                {
                    BaseColorFactor = new( 0.5f, 0.5f, 0.5f, 1 ),
                    MetallicFactor = 1f,
                    RoughnessFactor = 0f
                }
            };
        }

        private static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat"></param>
        /// <returns>roughnessFactor</returns>
        private static double ConvertTraditional2MetallicRoughness(ObjMaterial mat)
        {
            // Transform from 0-1000 range to 0-1 range. Then invert.
            //var roughnessFactor = mat.SpecularExponent; // options.metallicRoughness ? 1.0 : 0.0;
            //roughnessFactor = roughnessFactor / 1000.0;
            var roughnessFactor = 1.0 - mat.SpecularExponent / 1000.0;
            roughnessFactor = Clamp(roughnessFactor, 0.0, 1.0);

            if (mat.Specular == null || mat.Specular.Color == null)
            {
                mat.Specular = new ObjReflectivity(new ObjFactorColor());
                return roughnessFactor;
            }
            // Translate the blinn-phong model to the pbr metallic-roughness model
            // Roughness factor is a combination of specular intensity and shininess
            // Metallic factor is 0.0
            // Textures are not converted for now
            var specularIntensity = Luminance(mat.Specular.Color);


            // Low specular intensity values should produce a rough material even if shininess is high.
            if (specularIntensity < 0.1)
            {
                roughnessFactor *= (1.0 - specularIntensity);
            }

            var metallicFactor = 0.0;
            mat.Specular = new ObjReflectivity(new ObjFactorColor(metallicFactor));
            return roughnessFactor;
        }

        private static int AddMaterial(ModelRoot model, Material? material)
        {
            ArgumentNullException.ThrowIfNull(material);

            var matIndex = model.Materials.Count;
            model.Materials.Add(material);
            return matIndex;
        }

        private static int GetTextureIndex(ModelRoot model, string path)
        {
            for (var i = 0; i < model.Textures.Count; i++)
            {
                if (path == model.Textures[i].Name)
                {
                    return i;
                }
            }
            return AddTexture(model, path);
        }

        private static Material ConvertMaterial(ObjMaterial mat, Func<string, int> getOrAddTextureFunction)
        {
            var roughnessFactor = ConvertTraditional2MetallicRoughness(mat);

            var gMat = new Material
            {
                Name = mat.Name,
                AlphaMode = AlphaMode.OPAQUE
            };

            var alpha = mat.GetAlpha();
            var metallicFactor = 0.0;
            if (mat.Specular != null && mat.Specular.Color != null)
            {
                metallicFactor = mat.Specular.Color.Red;
            }
            gMat.PbrMetallicRoughness = new()
            {
                RoughnessFactor = (float)roughnessFactor,
                MetallicFactor = (float)metallicFactor
            };
            if (mat.Diffuse != null)
            {
                gMat.PbrMetallicRoughness.BaseColorFactor = new(ToArray(mat.Diffuse.Color, alpha));
            }
            else if (mat.Ambient != null)
            {
                gMat.PbrMetallicRoughness.BaseColorFactor = new(ToArray(mat.Ambient.Color, alpha));
            }
            else
            {
                gMat.PbrMetallicRoughness.BaseColorFactor = new( 0.7f, 0.7f, 0.7f, (float)alpha );
            }


            var hasTexture = !string.IsNullOrEmpty(mat.DiffuseTextureFile);
            if (hasTexture)
            {
                var index = getOrAddTextureFunction(mat.DiffuseTextureFile);
                gMat.PbrMetallicRoughness.BaseColorTexture = new()
                {
                    Index = index
                };
            }


            var hasNormalTexture = !string.IsNullOrEmpty(mat.NormalTextureFile);
            if (hasNormalTexture)
            {
                var index = getOrAddTextureFunction(mat.NormalTextureFile);
                gMat.NormalTexture = new()
                {
                    Index = index
                };
            }


            if (mat.Emissive != null && mat.Emissive.Color != null)
            {
                gMat.EmissiveFactor = new(ToArray(mat.Emissive.Color));
            }

            if (alpha < 1.0)
            {
                gMat.AlphaMode = AlphaMode.BLEND;
                gMat.DoubleSided = true;
            }

            return gMat;
        }

        //TODO: move to gltf model ?!?
        private static int GetMaterialIndex(ModelRoot model, string matName)
        {
            for (var i = 0; i < model.Materials.Count; i++)
            {
                if (model.Materials[i].Name == matName)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion Materials

        #region Meshes

        private static int AddMesh(ModelSource model, ObjModelRoot objModel, ObjGeometry mesh)
        {
            var ps = AddVertexAttributes(model, objModel, mesh);

            var meshIndex = model.Meshes.AddWithIndex(new()
            {
                Name = mesh.Id,
                Primitives = ps
            });
            return meshIndex;
        }

        private static List<MeshPrimitive> AddVertexAttributes(ModelSource model,
                                                    ObjModelRoot objModel,
                                                    ObjGeometry mesh)
        {
            var facesGroup = mesh.Faces.GroupBy(c => c.MatName);
            var faces = new List<ObjFace>();
            foreach (var fg in facesGroup)
            {
                var matName = fg.Key;
                var f = new ObjFace(matName);

                foreach (var ff in fg)
                {
                    f.Triangles.AddRange(ff.Triangles);
                }

                if (f.Triangles.Count > 0)
                {
                    faces.Add(f);
                }

            }

            // Vertex attributes are shared by all primitives in the mesh
            var name0 = mesh.Id;

            var ps = new List<MeshPrimitive>(faces.Count * 2);
            var index = 0;
            foreach (var f in faces)
            {
                var faceName = name0;
                if (index > 0)
                {
                    faceName = $"{name0}_{index}";
                }

                var hasUvs = f.Triangles.Any(d => d.V1.T > 0);
                var hasNormals = f.Triangles.Any(d => d.V1.N > 0);

                var materialIndex = GetMaterialIndexOrDefault(model, objModel, f.MatName);
                var material = materialIndex < objModel.Materials.Count ? objModel.Materials[materialIndex] : null;
                var materialHasTexture = material?.DiffuseTextureFile != null;

                // every primitive needs their own vertex indices(v,t,n)
                var faceVertexCache = new Dictionary<string, int>();
                var faceVertexCount = 0;

                var atts = new Dictionary<string, int>();
                var indicesAccessorIndex = model.CreateIndicesAccessor(faceName + "_indices");
                var positionAccessorIndex = model.CreateAccessor<Vector3>(faceName + "_positions");
                atts.Add("POSITION", positionAccessorIndex);
                var normalsAccessorIndex = -1;
                if (hasNormals)
                {
                    normalsAccessorIndex = model.CreateAccessor<Vector3>(faceName + "_normals", true);
                    atts.Add("NORMAL", normalsAccessorIndex);
                }
                var uvAccessorIndex = -1;
                if (materialHasTexture)
                {
                    if (hasUvs)
                    {
                        uvAccessorIndex = model.CreateAccessor<Vector2>(faceName + "_texcoords", true);
                        atts.Add("TEXCOORD_0", uvAccessorIndex);
                    }
                    else
                    {
                        var gMat = model.Materials[materialIndex];
                        if (gMat.PbrMetallicRoughness.BaseColorTexture != null)
                        {
                            gMat.PbrMetallicRoughness.BaseColorTexture = null;
                        }
                    }
                }

                // f is a primitive
                var iList = new List<int>(f.Triangles.Count * 3 * 2); // primitive indices
                foreach (var triangle in f.Triangles)
                {
                    var v1Index = triangle.V1.V - 1;
                    var v2Index = triangle.V2.V - 1;
                    var v3Index = triangle.V3.V - 1;
                    var v1 = objModel.Vertices[v1Index];
                    var v2 = objModel.Vertices[v2Index];
                    var v3 = objModel.Vertices[v3Index];

                    var n1 = new Vector3();
                    var n2 = new Vector3();
                    var n3 = new Vector3();

                    if (triangle.V1.N > 0) // hasNormals
                    {
                        var n1Index = triangle.V1.N - 1;
                        var n2Index = triangle.V2.N - 1;
                        var n3Index = triangle.V3.N - 1;
                        n1 = objModel.Normals[n1Index];
                        n2 = objModel.Normals[n2Index];
                        n3 = objModel.Normals[n3Index];
                    }

                    var t1 = new Vector2();
                    var t2 = new Vector2();
                    var t3 = new Vector2();

                    if (materialHasTexture)
                    {
                        if (triangle.V1.T > 0) // hasUvs
                        {
                            var t1Index = triangle.V1.T - 1;
                            var t2Index = triangle.V2.T - 1;
                            var t3Index = triangle.V3.T - 1;
                            t1 = objModel.Uvs[t1Index];
                            t2 = objModel.Uvs[t2Index];
                            t3 = objModel.Uvs[t3Index];
                        }
                    }

                    var v1Str = triangle.V1.ToString();
                    if (!faceVertexCache.ContainsKey(v1Str))
                    {
                        faceVertexCache.Add(v1Str, faceVertexCount++);

                        model.AddAccessorBuffer(positionAccessorIndex, v1);

                        if (triangle.V1.N > 0) // hasNormals
                        {
                            model.AddAccessorBuffer(normalsAccessorIndex, n1);
                        }
                        if (materialHasTexture)
                        {
                            if (triangle.V1.T > 0) // hasUvs
                            {
                                var uv = new Vector2(t1.X, 1 - t1.Y);
                                model.AddAccessorBuffer(uvAccessorIndex, uv);
                            }
                        }
                    }

                    var v2Str = triangle.V2.ToString();
                    if (!faceVertexCache.ContainsKey(v2Str))
                    {
                        faceVertexCache.Add(v2Str, faceVertexCount++);

                        model.AddAccessorBuffer(positionAccessorIndex, v2);
                        if (triangle.V2.N > 0) // hasNormals
                        {
                            model.AddAccessorBuffer(normalsAccessorIndex, n2);
                        }
                        if (materialHasTexture)
                        {
                            if (triangle.V2.T > 0) // hasUvs
                            {
                                var uv = new Vector2(t2.X, 1 - t2.Y);
                                model.AddAccessorBuffer(uvAccessorIndex, uv);
                            }
                        }
                    }

                    var v3Str = triangle.V3.ToString();
                    if (!faceVertexCache.ContainsKey(v3Str))
                    {
                        faceVertexCache.Add(v3Str, faceVertexCount++);

                        model.AddAccessorBuffer(positionAccessorIndex, v3);
                        if (triangle.V3.N > 0) // hasNormals
                        {
                            model.AddAccessorBuffer(normalsAccessorIndex, n3);
                        }
                        if (materialHasTexture)
                        {
                            if (triangle.V3.T > 0) // hasUvs
                            {
                                var uv = new Vector2(t3.X, 1 - t3.Y);
                                model.AddAccessorBuffer(uvAccessorIndex, uv);
                            }
                        }
                    }

                    // Vertex Indices
                    var correctWinding = CheckWindingCorrect(v1, v2, v3, n1);
                    if (correctWinding)
                    {
                        iList.AddRange([
                            faceVertexCache[v1Str],
                            faceVertexCache[v2Str],
                            faceVertexCache[v3Str]
                        ]);
                    }
                    else
                    {
                        iList.AddRange([
                            faceVertexCache[v1Str],
                            faceVertexCache[v3Str],
                            faceVertexCache[v2Str]
                        ]);
                    }
                }

                foreach (var i in iList)
                {
                    model.AddAccessorBuffer(indicesAccessorIndex, i);
                }

                ps.Add(new()
                {
                    Attributes = atts,
                    Indices = indicesAccessorIndex,
                    Material = materialIndex,
                    Mode = PrimitiveType.TRIANGLES
                });


                index++;
            }

            return ps;
        }

        private static int GetMaterialIndexOrDefault(ModelRoot model, ObjModelRoot objModel, string materialName)
        {
            if (string.IsNullOrEmpty(materialName))
            {
                materialName = "default";
            }

            var materialIndex = GetMaterialIndex(model, materialName);
            if (materialIndex == -1)
            {
                var objMaterial = objModel.Materials.FirstOrDefault(c => c.Name == materialName);
                if (objMaterial == null)
                {
                    materialName = "default";
                    materialIndex = GetMaterialIndex(model, materialName);
                    if (materialIndex == -1)
                    {
                        var gMat = GetDefault();
                        materialIndex = AddMaterial(model, gMat);
                    }
                    else
                    {
#if DEBUG
                        Debugger.Break();
#endif
                    }
                }
                else
                {
                    var gMat = ConvertMaterial(objMaterial, t => GetTextureIndex(model, t));
                    materialIndex = AddMaterial(model, gMat);
                }
            }

            return materialIndex;
        }

        private int AddNode(ModelRoot model, string name, int meshIndex, int parentIndex = -1)
        {
            var node = new Node { Name = name, Mesh = meshIndex };
            var nodeIndex = model.Nodes.AddWithIndex(node);
            model.Scenes[model.Scene].Nodes.Add(nodeIndex);

            return nodeIndex;
        }

        #endregion Meshes

        private static float[] ToArray(ObjFactorColor color, double? alpha = null)
        {
            var max = new double[] { color.Red, color.Green, color.Blue }.Max();
            if (max > 1)
            {
                color.Red /= max;
                color.Green /= max;
                color.Blue /= max;
            }
            if (alpha == null) {
                return [(float)color.Red, (float)color.Green, (float)color.Blue];
            }
            return [(float)color.Red, (float)color.Green, (float)color.Blue, (float)alpha];
        }
    }
}
