using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Compression.Lz4;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderExporter(Shader shader) : IFileExporter
    {
        private const string NoteHeader = "//////////////////////////////////////////\n" +
                                      "//\n" +
                                      "// NOTE: This is *not* a valid shader file\n" +
                                      "//\n" +
                                      "///////////////////////////////////////////\n";
        public string Name => shader.Name;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".shader", mode, out fileName))
            {
                return;
            }
            using var fs = File.OpenWrite(fileName);
            using var sw = new CodeWriter(fs);
            sw.Write(NoteHeader).WriteLine();
            if (shader.m_SubProgramBlob != null) //5.3 - 5.4
            {
                var uncompressedSize = (int)shader.decompressedSize;
                using var ms = new ArrayMemoryStream(uncompressedSize);
                new Lz4Decompressor(shader.decompressedSize)
                        .Decompress(
                        shader.m_SubProgramBlob,
                        shader.m_SubProgramBlob.Length, ms.GetBuffer(), uncompressedSize);
                using var blobReader = new BundleBinaryReader(ms, leaveOpen: false);
                blobReader.Add(shader.AssetFile.UnityVersion);
                var program = new ShaderProgram(blobReader);
                program.Read(blobReader, 0);
                program.Write(Encoding.UTF8.GetString(shader.m_Script), sw);
                return;
            }

            if (shader.compressedBlob != null) //5.5 and up
            {
                ConvertSerializedShader(sw);
                return;
            }
            sw.Write(shader.m_Script);
        }

        private void ConvertSerializedShader(ICodeWriter writer)
        {
            var length = shader.platforms.Length;
            var shaderPrograms = new ShaderProgram[length];
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < shader.offsets[i].Length; j++)
                {
                    var offset = (int)shader.offsets[i][j];
                    var compressedLength = (int)shader.compressedLengths[i][j];
                    var decompressedLength = (int)shader.decompressedLengths[i][j];
                    using var ms = new ArrayMemoryStream(decompressedLength);
                    new Lz4Decompressor(decompressedLength)
                        .Decompress(
                            shader.compressedBlob,
                            offset,
                            compressedLength,
                            ms.GetBuffer(),
                            decompressedLength);
                    using var blobReader = new BundleBinaryReader(ms, leaveOpen: false);
                    blobReader.Add(shader.AssetFile.UnityVersion);
                    if (j == 0)
                    {
                        shaderPrograms[i] = new ShaderProgram(blobReader);
                    }
                    shaderPrograms[i].Read(blobReader, j);
                }
            }

            ConvertSerializedShader(writer, shader.m_ParsedForm, shader.platforms, shaderPrograms);
        }

        private static void ConvertSerializedShader(ICodeWriter writer, 
            SerializedShader m_ParsedForm, ShaderCompilerPlatform[] platforms, 
            ShaderProgram[] shaderPrograms)
        {

            writer.WriteFormat("Shader \"{0}\" {{", m_ParsedForm.m_Name)
                .WriteIndentLine();

            ConvertSerializedProperties(writer, m_ParsedForm.m_PropInfo);

            foreach (var m_SubShader in m_ParsedForm.m_SubShaders)
            {
                ConvertSerializedSubShader(writer, m_SubShader, platforms, shaderPrograms);
            }
            
            if (!string.IsNullOrEmpty(m_ParsedForm.m_FallbackName))
            {
                writer.Write($"Fallback \"{m_ParsedForm.m_FallbackName}\"")
                    .WriteLine(true);
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.m_CustomEditorName))
            {
                writer.Write($"CustomEditor \"{m_ParsedForm.m_CustomEditorName}\"")
                    .WriteLine(true);
            }

            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }

        private static void ConvertSerializedSubShader(ICodeWriter writer, 
            SerializedSubShader m_SubShader, ShaderCompilerPlatform[] platforms, 
            ShaderProgram[] shaderPrograms)
        {
            writer.Write("SubShader {")
                .WriteIndentLine();
            if (m_SubShader.m_LOD != 0)
            {
                writer.Write($"LOD {m_SubShader.m_LOD}")
                    .WriteLine(true);
            }

            ConvertSerializedTagMap(writer, m_SubShader.m_Tags);

            foreach (var m_Passe in m_SubShader.m_Passes)
            {
                ConvertSerializedPass(writer, m_Passe, platforms, shaderPrograms);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }
        private static SerializedPlayerSubProgram[] FlattenPlayerSubPrograms(SerializedProgram program)
        {
            var flatList = new List<SerializedPlayerSubProgram>();
            if (program?.m_PlayerSubPrograms == null)
            {
                return flatList.ToArray();
            }
            foreach (var subArray in program.m_PlayerSubPrograms)
            {
                if (subArray != null)
                {
                    flatList.AddRange(subArray);
                }
            }
            return flatList.ToArray();
        }

        private static void ConvertPrograms(ICodeWriter writer, SerializedProgram program, string programType, 
            ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            if (program?.m_SubPrograms?.Count > 0)
            {
                writer.Write($"Program \"{programType}\" {{").WriteIndentLine();
                ConvertSerializedSubPrograms(writer, [..program.m_SubPrograms], platforms, shaderPrograms);
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }
            SerializedPlayerSubProgram[] flattenedPlayerSubPrograms = FlattenPlayerSubPrograms(program);
            if (flattenedPlayerSubPrograms?.Length > 0)
            {
                writer.Write($"PlayerProgram \"{programType}\" {{").WriteIndentLine();
                ConvertSerializedPlayerSubPrograms(writer, flattenedPlayerSubPrograms, platforms, shaderPrograms);
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }
        }

        private static void ConvertSerializedPass(ICodeWriter writer, SerializedPass m_Passe, 
            ShaderCompilerPlatform[] platforms, 
            ShaderProgram[] shaderPrograms)
        {
            switch (m_Passe.m_Type)
            {
                case PassType.Normal:
                    writer.Write("Pass ");
                    break;
                case PassType.Use:
                    writer.Write("UsePass ");
                    break;
                case PassType.Grab:
                    writer.Write("GrabPass ");
                    break;
            }
            if (m_Passe.m_Type == PassType.Use)
            {
                writer.Write($"\"{m_Passe.m_UseName}\"");
            }
            else
            {
                writer.Write("{");

                if (m_Passe.m_Type == PassType.Grab)
                {
                    if (!string.IsNullOrEmpty(m_Passe.m_TextureName))
                    {
                        writer.WriteLine(true)
                            .Write($"\"{m_Passe.m_TextureName}\"").WriteLine(true);
                    }
                }
                else
                {
                    writer.WriteIndentLine();
                    ConvertSerializedShaderState(writer, m_Passe.m_State);

                    ConvertPrograms(writer, m_Passe.progVertex, "vp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.progFragment, "fp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.progGeometry, "gp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.progHull, "hp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.progDomain, "dp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.progRayTracing, "rtp", platforms, shaderPrograms);
                    writer.WriteOutdentLine();
                }
                writer.Write("}");
            }
        }

        private static void AppendSubProgram<T>(ICodeWriter writer, T serializedSubProgram, ShaderCompilerPlatform platform,
            ShaderProgram shaderProgram, Func<T, uint> getBlobIndex, Func<T, string> getAdditionalInfo)
        {
            writer.Write($"SubProgram \"{GetPlatformString(platform)} ");
            if (getAdditionalInfo != null)
            {
                writer.Write($"{getAdditionalInfo(serializedSubProgram)} ");
            }

            writer.Write("\" {").WriteIndentLine();

            var subProgramWrap = shaderProgram.m_SubProgramWraps[getBlobIndex(serializedSubProgram)];
            var subProgram = subProgramWrap.GenShaderSubProgram();
            subProgram.Write(writer);

            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }


        private static void ConvertSubPrograms<T>(ICodeWriter writer, IEnumerable<T> m_SubPrograms, ShaderCompilerPlatform[] platforms,
            ShaderProgram[] shaderPrograms, Func<T, uint> getBlobIndex,
            Func<T, ShaderGpuProgramType> getGpuProgramType, Func<T, string> getAdditionalInfo = null)
        {
            var groups = m_SubPrograms.GroupBy(getBlobIndex);

            foreach (var group in groups)
            {
                var programs = group.GroupBy(getGpuProgramType);
                foreach (var program in programs)
                {
                    for (int i = 0; i < platforms.Length; i++)
                    {
                        var platform = platforms[i];
                        if (CheckGpuProgramUsable(platform, program.Key))
                        {
                            var shaderProgram = shaderPrograms[i];
                            foreach (var subProgram in program)
                            {
                                AppendSubProgram(writer, subProgram, platform, shaderProgram, getBlobIndex,
                                    getAdditionalInfo);
                            }

                            break;
                        }
                    }
                }
            }

        }

        private static void ConvertSerializedPlayerSubPrograms(ICodeWriter writer, SerializedPlayerSubProgram[] m_SubPrograms,
            ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            ConvertSubPrograms(writer, m_SubPrograms, platforms, shaderPrograms, x => x.m_BlobIndex,
                x => x.m_GpuProgramType);
        }

        private static void ConvertSerializedSubPrograms(ICodeWriter writer, SerializedSubProgram[] m_SubPrograms,
            ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            ConvertSubPrograms(writer, m_SubPrograms, platforms, shaderPrograms, x => x.m_BlobIndex,
                x => x.m_GpuProgramType, x => $"hw_tier{x.m_ShaderHardwareTier:00}");
        }

        private static void ConvertSerializedShaderState(ICodeWriter writer, SerializedShaderState m_State)
        {
            if (!string.IsNullOrEmpty(m_State.m_Name))
            {
                writer.Write($"Name \"{m_State.m_Name}\"").WriteLine(true);
            }
            if (m_State.m_LOD != 0)
            {
                writer.Write($"LOD {m_State.m_LOD}").WriteLine(true);
            }

            ConvertSerializedTagMap(writer, m_State.m_Tags);

            ConvertSerializedShaderRTBlendState(writer, [..m_State.rtBlend], m_State.rtSeparateBlend);

            if (m_State.alphaToMask.val > 0f)
            {
                writer.Write("AlphaToMask On").WriteLine(true);
            }

            if (m_State.zClip?.val != 1f) //ZClip On
            {
                writer.Write("ZClip Off").WriteLine(true);
            }

            if (m_State.zTest.val != 4f) //ZTest LEqual
            {
                writer.Write("ZTest ");
                switch (m_State.zTest.val) //enum CompareFunction
                {
                    case 0f: //kFuncDisabled
                        writer.Write("Off");
                        break;
                    case 1f: //kFuncNever
                        writer.Write("Never");
                        break;
                    case 2f: //kFuncLess
                        writer.Write("Less");
                        break;
                    case 3f: //kFuncEqual
                        writer.Write("Equal");
                        break;
                    case 5f: //kFuncGreater
                        writer.Write("Greater");
                        break;
                    case 6f: //kFuncNotEqual
                        writer.Write("NotEqual");
                        break;
                    case 7f: //kFuncGEqual
                        writer.Write("GEqual");
                        break;
                    case 8f: //kFuncAlways
                        writer.Write("Always");
                        break;
                }

                writer.WriteLine(true);
            }

            if (m_State.zWrite.val != 1f) //ZWrite On
            {
                writer.Write("ZWrite Off").WriteLine(true);
            }

            if (m_State.culling.val != 2f) //Cull Back
            {
                writer.Write("Cull ");
                switch (m_State.culling.val) //enum CullMode
                {
                    case 0f: //kCullOff
                        writer.Write("Off");
                        break;
                    case 1f: //kCullFront
                        writer.Write("Front");
                        break;
                }
                writer.WriteLine(true);
            }

            if (m_State.offsetFactor.val != 0f || m_State.offsetUnits.val != 0f)
            {
                writer.Write($"Offset {m_State.offsetFactor.val}, {m_State.offsetUnits.val}")
                    .WriteLine(true);
            }

            if (m_State.stencilRef.val != 0f ||
                m_State.stencilReadMask.val != 255f ||
                m_State.stencilWriteMask.val != 255f ||
                m_State.stencilOp.pass.val != 0f ||
                m_State.stencilOp.fail.val != 0f ||
                m_State.stencilOp.zFail.val != 0f ||
                m_State.stencilOp.comp.val != 8f ||
                m_State.stencilOpFront.pass.val != 0f ||
                m_State.stencilOpFront.fail.val != 0f ||
                m_State.stencilOpFront.zFail.val != 0f ||
                m_State.stencilOpFront.comp.val != 8f ||
                m_State.stencilOpBack.pass.val != 0f ||
                m_State.stencilOpBack.fail.val != 0f ||
                m_State.stencilOpBack.zFail.val != 0f ||
                m_State.stencilOpBack.comp.val != 8f)
            {
                writer.Write("Stencil {").WriteIndentLine();
                if (m_State.stencilRef.val != 0f)
                {
                    writer.Write($"Ref {m_State.stencilRef.val}").WriteLine(true);
                }
                if (m_State.stencilReadMask.val != 255f)
                {
                    writer.Write($"ReadMask {m_State.stencilReadMask.val}").WriteLine(true);
                }
                if (m_State.stencilWriteMask.val != 255f)
                {
                    writer.Write($"WriteMask {m_State.stencilWriteMask.val}").WriteLine(true);
                }
                if (m_State.stencilOp.pass.val != 0f ||
                    m_State.stencilOp.fail.val != 0f ||
                    m_State.stencilOp.zFail.val != 0f ||
                    m_State.stencilOp.comp.val != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.stencilOp, "");
                }
                if (m_State.stencilOpFront.pass.val != 0f ||
                    m_State.stencilOpFront.fail.val != 0f ||
                    m_State.stencilOpFront.zFail.val != 0f ||
                    m_State.stencilOpFront.comp.val != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.stencilOpFront, "Front");
                }
                if (m_State.stencilOpBack.pass.val != 0f ||
                    m_State.stencilOpBack.fail.val != 0f ||
                    m_State.stencilOpBack.zFail.val != 0f ||
                    m_State.stencilOpBack.comp.val != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.stencilOpBack, "Back");
                }
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }

            if (m_State.fogMode != FogMode.Unknown ||
                m_State.fogColor.x.val != 0f ||
                m_State.fogColor.y.val != 0f ||
                m_State.fogColor.z.val != 0f ||
                m_State.fogColor.w.val != 0f ||
                m_State.fogDensity.val != 0f ||
                m_State.fogStart.val != 0f ||
                m_State.fogEnd.val != 0f)
            {
                writer.Write("Fog {").WriteIndentLine();
                if (m_State.fogMode != FogMode.Unknown)
                {
                    writer.Write("Mode ");
                    switch (m_State.fogMode)
                    {
                        case FogMode.Disabled:
                            writer.Write("Off");
                            break;
                        case FogMode.Linear:
                            writer.Write("Linear");
                            break;
                        case FogMode.Exp:
                            writer.Write("Exp");
                            break;
                        case FogMode.Exp2:
                            writer.Write("Exp2");
                            break;
                    }
                    writer.WriteLine(true);
                }
                if (m_State.fogColor.x.val != 0f ||
                    m_State.fogColor.y.val != 0f ||
                    m_State.fogColor.z.val != 0f ||
                    m_State.fogColor.w.val != 0f)
                {
                    writer.WriteFormat("Color ({0},{1},{2},{3})",
                        m_State.fogColor.x.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.y.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.z.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.w.val.ToString(CultureInfo.InvariantCulture))
                        .WriteLine(true);
                }
                if (m_State.fogDensity.val != 0f)
                {
                    writer.Write($"Density {m_State.fogDensity.val.ToString(CultureInfo.InvariantCulture)}").WriteLine(true);
                }
                if (m_State.fogStart.val != 0f ||
                    m_State.fogEnd.val != 0f)
                {
                    writer.Write($"Range {m_State.fogStart.val.ToString(CultureInfo.InvariantCulture)}, {m_State.fogEnd.val.ToString(CultureInfo.InvariantCulture)}").WriteLine(true);
                }
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }

            if (m_State.lighting)
            {
                writer.Write($"Lighting {(m_State.lighting ? "On" : "Off")}").WriteLine(true);
            }

            writer.Write($"GpuProgramID {m_State.gpuProgramID}").WriteLine(true);

        }

        private static void ConvertSerializedStencilOp(ICodeWriter writer, SerializedStencilOp stencilOp, string suffix)
        {
            writer.Write($"Comp{suffix} {ConvertStencilComp(stencilOp.comp)}").WriteLine(true);
            writer.Write($"Pass{suffix} {ConvertStencilOp(stencilOp.pass)}").WriteLine(true);
            writer.Write($"Fail{suffix} {ConvertStencilOp(stencilOp.fail)}").WriteLine(true);
            writer.Write($"ZFail{suffix} {ConvertStencilOp(stencilOp.zFail)}").WriteLine(true);
        }

        private static string ConvertStencilOp(SerializedShaderFloatValue op)
        {
            return op.val switch
            {
                1f => "Zero",
                2f => "Replace",
                3f => "IncrSat",
                4f => "DecrSat",
                5f => "Invert",
                6f => "IncrWrap",
                7f => "DecrWrap",
                _ => "Keep",
            };
        }

        private static string ConvertStencilComp(SerializedShaderFloatValue comp)
        {
            return comp.val switch
            {
                0f => "Disabled",
                1f => "Never",
                2f => "Less",
                3f => "Equal",
                4f => "LEqual",
                5f => "Greater",
                6f => "NotEqual",
                7f => "GEqual",
                _ => "Always",
            };
        }

        private static void ConvertSerializedShaderRTBlendState(ICodeWriter writer, SerializedShaderRTBlendState[] rtBlend, bool rtSeparateBlend)
        {
            
            for (var i = 0; i < rtBlend.Length; i++)
            {
                var blend = rtBlend[i];
                if (blend.srcBlend.val != 1f ||
                    blend.targetBlend.val != 0f ||
                    blend.srcBlendAlpha.val != 1f ||
                    blend.targetBlendAlpha.val != 0f)
                {
                    writer.Write("Blend ");
                    if (i != 0 || rtSeparateBlend)
                    {
                        writer.Write($"{i} ");
                    }
                    writer.Write($"{ConvertBlendFactor(blend.srcBlend)} {ConvertBlendFactor(blend.targetBlend)}");
                    if (blend.srcBlendAlpha.val != 1f ||
                        blend.targetBlendAlpha.val != 0f)
                    {
                        writer.Write($", {ConvertBlendFactor(blend.srcBlendAlpha)} {ConvertBlendFactor(blend.targetBlendAlpha)}");
                    }
                    writer.WriteLine(true);
                }

                if (blend.blendOp.val != 0f ||
                    blend.blendOpAlpha.val != 0f)
                {
                    writer.Write("BlendOp ");
                    if (i != 0 || rtSeparateBlend)
                    {
                        writer.Write($"{i} ");
                    }
                    writer.Write(ConvertBlendOp(blend.blendOp));
                    if (blend.blendOpAlpha.val != 0f)
                    {
                        writer.Write($", {ConvertBlendOp(blend.blendOpAlpha)}");
                    }
                    writer.WriteLine(true);
                }

                var val = (int)blend.colMask.val;
                if (val != 0xf)
                {
                    writer.Write("ColorMask ");
                    if (val == 0)
                    {
                        writer.Write(0);
                    }
                    else
                    {
                        if ((val & 0x2) != 0)
                        {
                            writer.Write("R");
                        }
                        if ((val & 0x4) != 0)
                        {
                            writer.Write("G");
                        }
                        if ((val & 0x8) != 0)
                        {
                            writer.Write("B");
                        }
                        if ((val & 0x1) != 0)
                        {
                            writer.Write("A");
                        }
                    }
                    writer.Write($" {i}").WriteLine(true);
                }
            }
        }

        private static string ConvertBlendOp(SerializedShaderFloatValue op)
        {
            return op.val switch
            {
                1f => "Sub",
                2f => "RevSub",
                3f => "Min",
                4f => "Max",
                5f => "LogicalClear",
                6f => "LogicalSet",
                7f => "LogicalCopy",
                8f => "LogicalCopyInverted",
                9f => "LogicalNoop",
                10f => "LogicalInvert",
                11f => "LogicalAnd",
                12f => "LogicalNand",
                13f => "LogicalOr",
                14f => "LogicalNor",
                15f => "LogicalXor",
                16f => "LogicalEquiv",
                17f => "LogicalAndReverse",
                18f => "LogicalAndInverted",
                19f => "LogicalOrReverse",
                20f => "LogicalOrInverted",
                _ => "Add",
            };
        }

        private static string ConvertBlendFactor(SerializedShaderFloatValue factor)
        {
            return factor.val switch
            {
                0f => "Zero",
                2f => "DstColor",
                3f => "SrcColor",
                4f => "OneMinusDstColor",
                5f => "SrcAlpha",
                6f => "OneMinusSrcColor",
                7f => "DstAlpha",
                8f => "OneMinusDstAlpha",
                9f => "SrcAlphaSaturate",
                10f => "OneMinusSrcAlpha",
                _ => "One",
            };
        }

        private static void ConvertSerializedTagMap(ICodeWriter writer, SerializedTagMap m_Tags)
        {
            if (m_Tags.tags.Count > 0)
            {
                writer.Write("Tags { ");
                foreach (var pair in m_Tags.tags)
                {
                    writer.Write($"\"{pair.Key}\" = \"{pair.Value}\" ");
                }
                writer.Write("}").WriteLine(true);
            }
        }

        private static void ConvertSerializedProperties(ICodeWriter writer, SerializedProperties m_PropInfo)
        {
            writer.Write("Properties {")
                .WriteIndentLine();
            foreach (var m_Prop in m_PropInfo.m_Props)
            {
                ConvertSerializedProperty(writer, m_Prop);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }

        private static void ConvertSerializedProperty(ICodeWriter writer, SerializedProperty m_Prop)
        {
            foreach (var m_Attribute in m_Prop.m_Attributes)
            {
                writer.Write($"[{m_Attribute}] ");
            }
            //TODO Flag
            writer.Write($"{m_Prop.m_Name} (\"{m_Prop.m_Description}\", ");
            switch (m_Prop.m_Type)
            {
                case SerializedPropertyType.Color:
                    writer.Write("Color");
                    break;
                case SerializedPropertyType.Vector:
                    writer.Write("Vector");
                    break;
                case SerializedPropertyType.Float:
                    writer.Write("Float");
                    break;
                case SerializedPropertyType.Range:
                    writer.Write($"Range({m_Prop.m_DefValue[1]}, {m_Prop.m_DefValue[2]})");
                    break;
                case SerializedPropertyType.Texture:
                    switch (m_Prop.m_DefTexture.m_TexDim)
                    {
                        case TextureDimension.Any:
                            writer.Write("any");
                            break;
                        case TextureDimension.Tex2D:
                            writer.Write("2D");
                            break;
                        case TextureDimension.Tex3D:
                            writer.Write("3D");
                            break;
                        case TextureDimension.Cube:
                            writer.Write("Cube");
                            break;
                        case TextureDimension.Tex2DArray:
                            writer.Write("2DArray");
                            break;
                        case TextureDimension.CubeArray:
                            writer.Write("CubeArray");
                            break;
                    }
                    break;
            }
            writer.Write(") = ");
            switch (m_Prop.m_Type)
            {
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Vector:
                    writer.Write($"({m_Prop.m_DefValue[0]},{m_Prop.m_DefValue[1]},{m_Prop.m_DefValue[2]},{m_Prop.m_DefValue[3]})");
                    break;
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Range:
                    writer.Write(m_Prop.m_DefValue[0]);
                    break;
                case SerializedPropertyType.Int:
                    writer.Write((int)Math.Round(m_Prop.m_DefValue[0]));
                    break;
                case SerializedPropertyType.Texture:
                    writer.Write($"\"{m_Prop.m_DefTexture.m_DefaultName}\" {{ }}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.WriteLine(true);
        }

        private static bool CheckGpuProgramUsable(ShaderCompilerPlatform platform, ShaderGpuProgramType programType)
        {
            return platform switch
            {
                ShaderCompilerPlatform.GL => programType == ShaderGpuProgramType.GLLegacy,
                ShaderCompilerPlatform.D3D9 => programType == ShaderGpuProgramType.DX9VertexSM20
                                        || programType == ShaderGpuProgramType.DX9VertexSM30
                                        || programType == ShaderGpuProgramType.DX9PixelSM20
                                        || programType == ShaderGpuProgramType.DX9PixelSM30,
                ShaderCompilerPlatform.Xbox360 or ShaderCompilerPlatform.PS3 or ShaderCompilerPlatform.PSP2 or ShaderCompilerPlatform.PS4 or ShaderCompilerPlatform.XboxOne or ShaderCompilerPlatform.N3DS or ShaderCompilerPlatform.WiiU or ShaderCompilerPlatform.Switch or ShaderCompilerPlatform.XboxOneD3D12 or ShaderCompilerPlatform.GameCoreXboxOne or ShaderCompilerPlatform.GameCoreScarlett or ShaderCompilerPlatform.PS5 => programType == ShaderGpuProgramType.ConsoleVS
                                        || programType == ShaderGpuProgramType.ConsoleFS
                                        || programType == ShaderGpuProgramType.ConsoleHS
                                        || programType == ShaderGpuProgramType.ConsoleDS
                                        || programType == ShaderGpuProgramType.ConsoleGS,
                ShaderCompilerPlatform.PS5NGGC => programType == ShaderGpuProgramType.PS5NGGC,
                ShaderCompilerPlatform.D3D11 => programType == ShaderGpuProgramType.DX11VertexSM40
                                        || programType == ShaderGpuProgramType.DX11VertexSM50
                                        || programType == ShaderGpuProgramType.DX11PixelSM40
                                        || programType == ShaderGpuProgramType.DX11PixelSM50
                                        || programType == ShaderGpuProgramType.DX11GeometrySM40
                                        || programType == ShaderGpuProgramType.DX11GeometrySM50
                                        || programType == ShaderGpuProgramType.DX11HullSM50
                                        || programType == ShaderGpuProgramType.DX11DomainSM50,
                ShaderCompilerPlatform.GLES20 => programType == ShaderGpuProgramType.GLES,
                //Obsolete
                ShaderCompilerPlatform.NaCl => throw new NotSupportedException(),
                //Obsolete
                ShaderCompilerPlatform.Flash => throw new NotSupportedException(),
                ShaderCompilerPlatform.D3D11_9x => programType == ShaderGpuProgramType.DX10Level9Vertex
                                        || programType == ShaderGpuProgramType.DX10Level9Pixel,
                ShaderCompilerPlatform.GLES3Plus => programType == ShaderGpuProgramType.GLES31AEP
                                        || programType == ShaderGpuProgramType.GLES31
                                        || programType == ShaderGpuProgramType.GLES3,
                //Unknown
                ShaderCompilerPlatform.PSM => throw new NotSupportedException(),
                ShaderCompilerPlatform.Metal => programType == ShaderGpuProgramType.MetalVS
                                        || programType == ShaderGpuProgramType.MetalFS,
                ShaderCompilerPlatform.OpenGLCore => programType == ShaderGpuProgramType.GLCore32
                                        || programType == ShaderGpuProgramType.GLCore41
                                        || programType == ShaderGpuProgramType.GLCore43,
                ShaderCompilerPlatform.Vulkan => programType == ShaderGpuProgramType.SPIRV,
                _ => throw new NotSupportedException(),
            };
        }

        private static string GetPlatformString(ShaderCompilerPlatform platform)
        {
            return platform switch
            {
                ShaderCompilerPlatform.GL => "openGL",
                ShaderCompilerPlatform.D3D9 => "d3d9",
                ShaderCompilerPlatform.Xbox360 => "xbox360",
                ShaderCompilerPlatform.PS3 => "ps3",
                ShaderCompilerPlatform.D3D11 => "d3d11",
                ShaderCompilerPlatform.GLES20 => "gles",
                ShaderCompilerPlatform.NaCl => "glesdesktop",
                ShaderCompilerPlatform.Flash => "flash",
                ShaderCompilerPlatform.D3D11_9x => "d3d11_9x",
                ShaderCompilerPlatform.GLES3Plus => "gles3",
                ShaderCompilerPlatform.PSP2 => "psp2",
                ShaderCompilerPlatform.PS4 => "ps4",
                ShaderCompilerPlatform.XboxOne => "xboxone",
                ShaderCompilerPlatform.PSM => "psm",
                ShaderCompilerPlatform.Metal => "metal",
                ShaderCompilerPlatform.OpenGLCore => "glcore",
                ShaderCompilerPlatform.N3DS => "n3ds",
                ShaderCompilerPlatform.WiiU => "wiiu",
                ShaderCompilerPlatform.Vulkan => "vulkan",
                ShaderCompilerPlatform.Switch => "switch",
                ShaderCompilerPlatform.XboxOneD3D12 => "xboxone_d3d12",
                ShaderCompilerPlatform.GameCoreXboxOne => "xboxone",
                ShaderCompilerPlatform.GameCoreScarlett => "xbox_scarlett",
                ShaderCompilerPlatform.PS5 => "ps5",
                ShaderCompilerPlatform.PS5NGGC => "ps5_nggc",
                _ => "unknown",
            };
        }
        public void Dispose()
        {
        }
    }
}
