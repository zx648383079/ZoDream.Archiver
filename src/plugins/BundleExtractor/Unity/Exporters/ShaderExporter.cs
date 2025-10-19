using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Compression.Lz4;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        private const string NoteHeader = "//////////////////////////////////////////\n" +
                                      "//\n" +
                                      "// NOTE: This is *not* a valid shader file\n" +
                                      "//\n" +
                                      "///////////////////////////////////////////\n";
        public string FileName => resource[entryId]?.Name ?? string.Empty;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not Shader shader)
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".shader", mode, out fileName))
            {
                return;
            }
            using var fs = File.OpenWrite(fileName);
            using var sw = new CodeWriter(fs);
            sw.Write(NoteHeader).WriteLine();
            if (shader.SubProgramBlob != null) //5.3 - 5.4
            {
                var uncompressedSize = (int)shader.DecompressedSize;
                using var ms = new ArrayMemoryStream(uncompressedSize);
                new Lz4Decompressor(shader.DecompressedSize)
                        .Decompress(
                        shader.SubProgramBlob,
                        shader.SubProgramBlob.Length, ms.GetBuffer(), uncompressedSize);
                using var blobReader = new BundleBinaryReader(ms, leaveOpen: false);
                blobReader.Add(resource.Version);
                var program = new ShaderProgram(blobReader);
                program.Read(blobReader, 0);
                program.Write(Encoding.UTF8.GetString(shader.Script), sw);
                return;
            }

            if (shader.CompressedBlob != null) //5.5 and up
            {
                ConvertSerializedShader(sw, shader);
                return;
            }
            sw.Write(shader.Script);
        }

        private void ConvertSerializedShader(ICodeWriter writer, Shader shader)
        {
            var length = shader.Platforms.Length;
            var shaderPrograms = new ShaderProgram[length];
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < shader.Offsets[i].Length; j++)
                {
                    var offset = (int)shader.Offsets[i][j];
                    var compressedLength = (int)shader.CompressedLengths[i][j];
                    var decompressedLength = (int)shader.DecompressedLengths[i][j];
                    using var ms = new ArrayMemoryStream(decompressedLength);
                    new Lz4Decompressor(decompressedLength)
                        .Decompress(
                            shader.CompressedBlob,
                            offset,
                            compressedLength,
                            ms.GetBuffer(),
                            decompressedLength);
                    using var blobReader = new BundleBinaryReader(ms, leaveOpen: false);
                    blobReader.Add(resource.Version);
                    if (j == 0)
                    {
                        shaderPrograms[i] = new ShaderProgram(blobReader);
                    }
                    shaderPrograms[i].Read(blobReader, j);
                }
            }

            ConvertSerializedShader(writer, shader.ParsedForm, shader.Platforms, shaderPrograms);
        }

        private static void ConvertSerializedShader(ICodeWriter writer, 
            SerializedShader m_ParsedForm, ShaderCompilerPlatform[] platforms, 
            ShaderProgram[] shaderPrograms)
        {

            writer.WriteFormat("Shader \"{0}\" {{", m_ParsedForm.Name)
                .WriteIndentLine();

            ConvertSerializedProperties(writer, m_ParsedForm.PropInfo);

            foreach (var m_SubShader in m_ParsedForm.SubShaders)
            {
                ConvertSerializedSubShader(writer, m_SubShader, platforms, shaderPrograms);
            }
            
            if (!string.IsNullOrEmpty(m_ParsedForm.FallbackName))
            {
                writer.Write($"Fallback \"{m_ParsedForm.FallbackName}\"")
                    .WriteLine(true);
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.CustomEditorName))
            {
                writer.Write($"CustomEditor \"{m_ParsedForm.CustomEditorName}\"")
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
            if (m_SubShader.LOD != 0)
            {
                writer.Write($"LOD {m_SubShader.LOD}")
                    .WriteLine(true);
            }

            ConvertSerializedTagMap(writer, m_SubShader.Tags);

            foreach (var m_Passe in m_SubShader.Passes)
            {
                ConvertSerializedPass(writer, m_Passe, platforms, shaderPrograms);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }
        private static SerializedPlayerSubProgram[] FlattenPlayerSubPrograms(SerializedProgram program)
        {
            var flatList = new List<SerializedPlayerSubProgram>();
            if (program?.PlayerSubPrograms == null)
            {
                return flatList.ToArray();
            }
            foreach (var subArray in program.PlayerSubPrograms)
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
            if (program?.SubPrograms?.Length > 0)
            {
                writer.Write($"Program \"{programType}\" {{").WriteIndentLine();
                ConvertSerializedSubPrograms(writer, [..program.SubPrograms], platforms, shaderPrograms);
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
            switch (m_Passe.Type)
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
            if (m_Passe.Type == PassType.Use)
            {
                writer.Write($"\"{m_Passe.UseName}\"");
            }
            else
            {
                writer.Write("{");

                if (m_Passe.Type == PassType.Grab)
                {
                    if (!string.IsNullOrEmpty(m_Passe.TextureName))
                    {
                        writer.WriteLine(true)
                            .Write($"\"{m_Passe.TextureName}\"").WriteLine(true);
                    }
                }
                else
                {
                    writer.WriteIndentLine();
                    ConvertSerializedShaderState(writer, m_Passe.State);

                    ConvertPrograms(writer, m_Passe.ProgVertex, "vp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.ProgFragment, "fp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.ProgGeometry, "gp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.ProgHull, "hp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.ProgDomain, "dp", platforms, shaderPrograms);
                    ConvertPrograms(writer, m_Passe.ProgRayTracing, "rtp", platforms, shaderPrograms);
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
            ConvertSubPrograms(writer, m_SubPrograms, platforms, shaderPrograms, x => x.BlobIndex,
                x => x.GpuProgramType);
        }

        private static void ConvertSerializedSubPrograms(ICodeWriter writer, SerializedSubProgram[] m_SubPrograms,
            ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            ConvertSubPrograms(writer, m_SubPrograms, platforms, shaderPrograms, x => x.BlobIndex,
                x => x.GpuProgramType, x => $"hw_tier{x.ShaderHardwareTier:00}");
        }

        private static void ConvertSerializedShaderState(ICodeWriter writer, SerializedShaderState m_State)
        {
            if (!string.IsNullOrEmpty(m_State.Name))
            {
                writer.Write($"Name \"{m_State.Name}\"").WriteLine(true);
            }
            if (m_State.LOD != 0)
            {
                writer.Write($"LOD {m_State.LOD}").WriteLine(true);
            }

            ConvertSerializedTagMap(writer, m_State.Tags);

            ConvertSerializedShaderRTBlendState(writer, [..m_State.RtBlend], m_State.RtSeparateBlend);

            if (m_State.AlphaToMask.Value > 0f)
            {
                writer.Write("AlphaToMask On").WriteLine(true);
            }

            if (m_State.ZClip.Value != 1f) //ZClip On
            {
                writer.Write("ZClip Off").WriteLine(true);
            }

            if (m_State.ZTest.Value != 4f) //ZTest LEqual
            {
                writer.Write("ZTest ");
                switch (m_State.ZTest.Value) //enum CompareFunction
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

            if (m_State.ZWrite.Value != 1f) //ZWrite On
            {
                writer.Write("ZWrite Off").WriteLine(true);
            }

            if (m_State.Culling.Value != 2f) //Cull Back
            {
                writer.Write("Cull ");
                switch (m_State.Culling.Value) //enum CullMode
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

            if (m_State.OffsetFactor.Value != 0f || m_State.OffsetUnits.Value != 0f)
            {
                writer.Write($"Offset {m_State.OffsetFactor.Value}, {m_State.OffsetUnits.Value}")
                    .WriteLine(true);
            }

            if (m_State.StencilRef.Value != 0f ||
                m_State.StencilReadMask.Value != 255f ||
                m_State.StencilWriteMask.Value != 255f ||
                m_State.StencilOp.Pass.Value != 0f ||
                m_State.StencilOp.Fail.Value != 0f ||
                m_State.StencilOp.ZFail.Value != 0f ||
                m_State.StencilOp.Comp.Value != 8f ||
                m_State.StencilOpFront.Pass.Value != 0f ||
                m_State.StencilOpFront.Fail.Value != 0f ||
                m_State.StencilOpFront.ZFail.Value != 0f ||
                m_State.StencilOpFront.Comp.Value != 8f ||
                m_State.StencilOpBack.Pass.Value != 0f ||
                m_State.StencilOpBack.Fail.Value != 0f ||
                m_State.StencilOpBack.ZFail.Value != 0f ||
                m_State.StencilOpBack.Comp.Value != 8f)
            {
                writer.Write("Stencil {").WriteIndentLine();
                if (m_State.StencilRef.Value != 0f)
                {
                    writer.Write($"Ref {m_State.StencilRef.Value}").WriteLine(true);
                }
                if (m_State.StencilReadMask.Value != 255f)
                {
                    writer.Write($"ReadMask {m_State.StencilReadMask.Value}").WriteLine(true);
                }
                if (m_State.StencilWriteMask.Value != 255f)
                {
                    writer.Write($"WriteMask {m_State.StencilWriteMask.Value}").WriteLine(true);
                }
                if (m_State.StencilOp.Pass.Value != 0f ||
                    m_State.StencilOp.Fail.Value != 0f ||
                    m_State.StencilOp.ZFail.Value != 0f ||
                    m_State.StencilOp.Comp.Value != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.StencilOp, "");
                }
                if (m_State.StencilOpFront.Pass.Value != 0f ||
                    m_State.StencilOpFront.Fail.Value != 0f ||
                    m_State.StencilOpFront.ZFail.Value != 0f ||
                    m_State.StencilOpFront.Comp.Value != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.StencilOpFront, "Front");
                }
                if (m_State.StencilOpBack.Pass.Value != 0f ||
                    m_State.StencilOpBack.Fail.Value != 0f ||
                    m_State.StencilOpBack.ZFail.Value != 0f ||
                    m_State.StencilOpBack.Comp.Value != 8f)
                {
                    ConvertSerializedStencilOp(writer, m_State.StencilOpBack, "Back");
                }
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }

            if (m_State.FogMode != FogMode.Unknown ||
                m_State.FogColor.X.Value != 0f ||
                m_State.FogColor.Y.Value != 0f ||
                m_State.FogColor.Z.Value != 0f ||
                m_State.FogColor.W.Value != 0f ||
                m_State.FogDensity.Value != 0f ||
                m_State.FogStart.Value != 0f ||
                m_State.FogEnd.Value != 0f)
            {
                writer.Write("Fog {").WriteIndentLine();
                if (m_State.FogMode != FogMode.Unknown)
                {
                    writer.Write("Mode ");
                    switch (m_State.FogMode)
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
                if (m_State.FogColor.X.Value != 0f ||
                    m_State.FogColor.Y.Value != 0f ||
                    m_State.FogColor.Z.Value != 0f ||
                    m_State.FogColor.W.Value != 0f)
                {
                    writer.WriteFormat("Color ({0},{1},{2},{3})",
                        m_State.FogColor.X.Value.ToString(CultureInfo.InvariantCulture),
                        m_State.FogColor.Y.Value.ToString(CultureInfo.InvariantCulture),
                        m_State.FogColor.Z.Value.ToString(CultureInfo.InvariantCulture),
                        m_State.FogColor.W.Value.ToString(CultureInfo.InvariantCulture))
                        .WriteLine(true);
                }
                if (m_State.FogDensity.Value != 0f)
                {
                    writer.Write($"Density {m_State.FogDensity.Value.ToString(CultureInfo.InvariantCulture)}").WriteLine(true);
                }
                if (m_State.FogStart.Value != 0f ||
                    m_State.FogEnd.Value != 0f)
                {
                    writer.Write($"Range {m_State.FogStart.Value.ToString(CultureInfo.InvariantCulture)}, {m_State.FogEnd.Value.ToString(CultureInfo.InvariantCulture)}").WriteLine(true);
                }
                writer.WriteOutdentLine().Write("}").WriteLine(true);
            }

            if (m_State.Lighting)
            {
                writer.Write($"Lighting {(m_State.Lighting ? "On" : "Off")}").WriteLine(true);
            }

            writer.Write($"GpuProgramID {m_State.GpuProgramID}").WriteLine(true);

        }

        private static void ConvertSerializedStencilOp(ICodeWriter writer, SerializedStencilOp stencilOp, string suffix)
        {
            writer.Write($"Comp{suffix} {ConvertStencilComp(stencilOp.Comp)}").WriteLine(true);
            writer.Write($"Pass{suffix} {ConvertStencilOp(stencilOp.Pass)}").WriteLine(true);
            writer.Write($"Fail{suffix} {ConvertStencilOp(stencilOp.Fail)}").WriteLine(true);
            writer.Write($"ZFail{suffix} {ConvertStencilOp(stencilOp.ZFail)}").WriteLine(true);
        }

        private static string ConvertStencilOp(SerializedShaderFloatValue op)
        {
            return op.Value switch
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
            return comp.Value switch
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
                if (blend.SrcBlend.Value != 1f ||
                    blend.TargetBlend.Value != 0f ||
                    blend.SrcBlendAlpha.Value != 1f ||
                    blend.TargetBlendAlpha.Value != 0f)
                {
                    writer.Write("Blend ");
                    if (i != 0 || rtSeparateBlend)
                    {
                        writer.Write($"{i} ");
                    }
                    writer.Write($"{ConvertBlendFactor(blend.SrcBlend)} {ConvertBlendFactor(blend.TargetBlend)}");
                    if (blend.SrcBlendAlpha.Value != 1f ||
                        blend.TargetBlendAlpha.Value != 0f)
                    {
                        writer.Write($", {ConvertBlendFactor(blend.SrcBlendAlpha)} {ConvertBlendFactor(blend.TargetBlendAlpha)}");
                    }
                    writer.WriteLine(true);
                }

                if (blend.BlendOp.Value != 0f ||
                    blend.BlendOpAlpha.Value != 0f)
                {
                    writer.Write("BlendOp ");
                    if (i != 0 || rtSeparateBlend)
                    {
                        writer.Write($"{i} ");
                    }
                    writer.Write(ConvertBlendOp(blend.BlendOp));
                    if (blend.BlendOpAlpha.Value != 0f)
                    {
                        writer.Write($", {ConvertBlendOp(blend.BlendOpAlpha)}");
                    }
                    writer.WriteLine(true);
                }

                var val = (int)blend.ColMask.Value;
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
            return op.Value switch
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
            return factor.Value switch
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
            if (m_Tags.Tags.Length > 0)
            {
                writer.Write("Tags { ");
                foreach (var pair in m_Tags.Tags)
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
            foreach (var m_Prop in m_PropInfo.Props)
            {
                ConvertSerializedProperty(writer, m_Prop);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }

        private static void ConvertSerializedProperty(ICodeWriter writer, SerializedProperty m_Prop)
        {
            foreach (var m_Attribute in m_Prop.Attributes)
            {
                writer.Write($"[{m_Attribute}] ");
            }
            //TODO Flag
            writer.Write($"{m_Prop.Name} (\"{m_Prop.Description}\", ");
            switch (m_Prop.Type)
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
                    writer.Write($"Range({m_Prop.DefValue[1]}, {m_Prop.DefValue[2]})");
                    break;
                case SerializedPropertyType.Texture:
                    switch (m_Prop.DefTexture.TexDim)
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
            switch (m_Prop.Type)
            {
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Vector:
                    writer.Write($"({m_Prop.DefValue[0]},{m_Prop.DefValue[1]},{m_Prop.DefValue[2]},{m_Prop.DefValue[3]})");
                    break;
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Range:
                    writer.Write(m_Prop.DefValue[0]);
                    break;
                case SerializedPropertyType.Int:
                    writer.Write((int)Math.Round(m_Prop.DefValue[0]));
                    break;
                case SerializedPropertyType.Texture:
                    writer.Write($"\"{m_Prop.DefTexture.DefaultName}\" {{ }}");
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
