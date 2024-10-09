using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.SerializedFiles;

namespace ZoDream.BundleExtractor.UI
{
    public class StaticBatchInfo
    {
        public ushort firstSubMesh;
        public ushort subMeshCount;

        public StaticBatchInfo(UIReader reader)
        {
            firstSubMesh = reader.Reader.ReadUInt16();
            subMeshCount = reader.Reader.ReadUInt16();
        }
    }

    public abstract class UIRenderer : UIComponent
    {
        public List<PPtr<Material>> m_Materials;
        public StaticBatchInfo m_StaticBatchInfo;
        public uint[] m_SubsetIndices;
        private bool isNewHeader = false;

        public static bool HasPrope(SerializedType type)
        {
            var hash = Convert.ToHexString(type.OldTypeHash);
            return hash == "F622BC5EE0E86D7BDF8C912DD94DCBF5"
            || hash == "9255FA54269ADD294011FDA525B5FCAC";
        }

        protected UIRenderer(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.Major < 5) //5.0 down
            {
                var m_Enabled = reader.Reader.ReadBoolean();
                var m_CastShadows = reader.Reader.ReadBoolean();
                var m_ReceiveShadows = reader.Reader.ReadBoolean();
                var m_LightmapIndex = reader.Reader.ReadByte();
            }
            else //5.0 and up
            {
                if (version.GreaterThanOrEquals(5,4)) //5.4 and up
                {
                    if (reader.IsGI())
                    {
                        CheckHeader(reader, 0x1A);
                    }
                    if (reader.IsBH3())
                    {
                        CheckHeader(reader, 0x12);
                    }
                    var m_Enabled = reader.Reader.ReadBoolean();
                    var m_CastShadows = reader.Reader.ReadByte();
                    var m_ReceiveShadows = reader.Reader.ReadByte();
                    if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
                    {
                        var m_DynamicOccludee = reader.Reader.ReadByte();
                    }
                    if (reader.IsBH3Group())
                    {
                        var m_AllowHalfResolution = reader.Reader.ReadByte();
                        int m_EnableGpuQuery = isNewHeader ? reader.Reader.ReadByte() : 0;
                    }
                    if (reader.IsGIGroup())
                    {
                        var m_ReceiveDecals = reader.Reader.ReadByte();
                        var m_EnableShadowCulling = reader.Reader.ReadByte();
                        var m_EnableGpuQuery = reader.Reader.ReadByte();
                        var m_AllowHalfResolution = reader.Reader.ReadByte();
                        if (!reader.IsGICB1())
                        {
                            if (reader.IsGI())
                            {
                                var m_AllowPerMaterialProp = isNewHeader ? 
                                    reader.Reader.ReadByte() : 0;
                            }
                            var m_IsRainOccluder = reader.Reader.ReadByte();
                            if (!reader.IsGICB2())
                            {
                                var m_IsDynamicAOOccluder = reader.Reader.ReadByte();
                                if (reader.IsGI())
                                {
                                    var m_IsHQDynamicAOOccluder = reader.Reader.ReadByte();
                                    var m_IsCloudObject = reader.Reader.ReadByte();
                                    var m_IsInteriorVolume = reader.Reader.ReadByte();
                                }
                            }
                            if (!reader.IsGIPack())
                            {
                                var m_IsDynamic = reader.Reader.ReadByte();
                            }
                            if (reader.IsGI())
                            {
                                var m_UseTessellation = reader.Reader.ReadByte();
                                var m_IsTerrainTessInfo = isNewHeader ? reader.Reader.ReadByte() : 0;
                                var m_UseVertexLightInForward = isNewHeader ? reader.Reader.ReadByte() : 0;
                                var m_CombineSubMeshInGeoPass = isNewHeader ? reader.Reader.ReadByte() : 0;
                            }
                        }
                    }
                    if (version.Major >= 2021) //2021.1 and up
                    {
                        var m_StaticShadowCaster = reader.Reader.ReadByte();
                        if (reader.IsArknightsEndfield())
                        {
                            var m_RealtimeShadowCaster = reader.Reader.ReadByte();
                            var m_SubMeshRenderMode = reader.Reader.ReadByte();
                            var m_CharacterIndex = reader.Reader.ReadByte();
                        }
                    }
                    var m_MotionVectors = reader.Reader.ReadByte();
                    var m_LightProbeUsage = reader.Reader.ReadByte();
                    var m_ReflectionProbeUsage = reader.Reader.ReadByte();
                    if (version.GreaterThanOrEquals(2019,3)) //2019.3 and up
                    {
                        var m_RayTracingMode = reader.Reader.ReadByte();
                    }
                    if (version.Major >= 2020) //2020.1 and up
                    {
                        var m_RayTraceProcedural = reader.Reader.ReadByte();
                    }
                    if (reader.IsGI() || reader.IsGICB3() || reader.IsGICB3Pre())
                    {
                        var m_MeshShowQuality = reader.Reader.ReadByte();
                    }
                    reader.Reader.AlignStream();
                }
                else
                {
                    var m_Enabled = reader.Reader.ReadBoolean();
                    reader.Reader.AlignStream();
                    var m_CastShadows = reader.Reader.ReadByte();
                    var m_ReceiveShadows = reader.Reader.ReadBoolean();
                    reader.Reader.AlignStream();
                }

                if (version.Major >= 2018 || (reader.IsBH3() && isNewHeader)) //2018 and up
                {
                    var m_RenderingLayerMask = reader.Reader.ReadUInt32();
                }

                if (version.GreaterThanOrEquals(2018,3)) //2018.3 and up
                {
                    var m_RendererPriority = reader.Reader.ReadInt32();
                }

                var m_LightmapIndex = reader.Reader.ReadUInt16();
                var m_LightmapIndexDynamic = reader.Reader.ReadUInt16();
                if (reader.IsGIGroup() && (m_LightmapIndex != 0xFFFF || m_LightmapIndexDynamic != 0xFFFF))
                {
                    throw new Exception("Not Supported !! skipping....");
                }
            }

            if (version.Major >= 3) //3.0 and up
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }

            if (version.Major >= 5) //5.0 and up
            {
                var m_LightmapTilingOffsetDynamic = reader.ReadVector4();
            }

            if (reader.IsGIGroup())
            {
                var m_ViewDistanceRatio = reader.Reader.ReadSingle();
                var m_ShaderLODDistanceRatio = reader.Reader.ReadSingle();
            }
            var m_MaterialsSize = reader.Reader.ReadInt32();
            m_Materials = new List<PPtr<Material>>();
            for (int i = 0; i < m_MaterialsSize; i++)
            {
                m_Materials.Add(new PPtr<Material>(reader));
            }

            if (version.Major < 3) //3.0 down
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }
            else //3.0 and up
            {
                if (version.GreaterThanOrEquals(5,5)) //5.5 and up
                {
                    m_StaticBatchInfo = new StaticBatchInfo(reader);
                }
                else
                {
                    m_SubsetIndices = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_StaticBatchRoot = new PPtr<Transform>(reader);
            }

            if (reader.IsGIGroup())
            {
                var m_MatLayers = reader.Reader.ReadInt32();
            }

            if (!reader.IsSR() || !HasPrope(reader.SerializedType))
            {
            if (version.GreaterThanOrEquals(5,4)) //5.4 and up
            {
                var m_ProbeAnchor = new PPtr<Transform>(reader);
                var m_LightProbeVolumeOverride = new PPtr<GameObject>(reader);
            }
            else if (version.GreaterThanOrEquals(3, 5)) //3.5 - 5.3
            {
                var m_UseLightProbes = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();

                if (version.Major >= 5)//5.0 and up
                {
                    var m_ReflectionProbeUsage = reader.Reader.ReadInt32();
                }

                var m_LightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
            }
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                if (version.Major == 4 && version.Minor == 3) //4.3
                {
                    var m_SortingLayer = reader.Reader.ReadInt16();
                }
                else
                {
                    var m_SortingLayerID = reader.Reader.ReadUInt32();
                }

                //SInt16 m_SortingLayer 5.6 and up
                var m_SortingOrder = reader.Reader.ReadInt16();
                reader.Reader.AlignStream();
                if (reader.IsGIGroup() || reader.IsBH3())
                {
                    var m_UseHighestMip = reader.Reader.ReadBoolean();
                    reader.Reader.AlignStream();
                }
                if (reader.IsSR())
                {
                    var RenderFlag = reader.Reader.ReadUInt32();
                    reader.Reader.AlignStream();
                }
            }
        }

        private void CheckHeader(UIReader reader, int offset)
        {
            short value = 0;
            var pos = reader.Position;
            while (value != -1 && reader.Position <= pos + offset)
            {
                value = reader.Reader.ReadInt16();
            }
            isNewHeader = (reader.Position - pos) == offset;
            reader.Position = pos;
        }
    }
}
