using System.IO;
using ZoDream.Live2dExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.Live2dExporter
{
    public partial class MocReader
    {
        private static object? Read(Stream input, string[] textureItems)
        {
            var header = new MocHeader();
            if (!header.TryRead(input))
            {
                return null;
            }
            var reader = new BundleBinaryReader(input, header.IsBigEndian ? EndianType.BigEndian : EndianType.LittleEndian);
            var counter = new MocCountInfoTable();
            counter.Read(reader, header.Version);
            // CanvasInfo
            new MocCanvasInfo().Read(reader);
            // part
            new MocPartOffset().Read(reader, (int)counter.Parts);
            // Deformers
            new MocDeformerOffset().Read(reader, (int)counter.Deformers);
            // WarpDeformers
            new MocWarpDeformerOffset().Read(reader, (int)counter.WarpDeformers);
            // RotationDeformers
            new MocRotationDeformerOffset().Read(reader, (int)counter.RotationDeformers);



            #region ArtMeshes
            var artMeshes = new MocArtMeshOffset();
            artMeshes.Read(reader, (int)counter.ArtMeshes);

            #endregion

            // Parameters
            new MocParameterOffset().Read(reader, (int)counter.Parameters);

            // PartKeyforms
            new MocPartKeyFormOffset().Read(reader, (int)counter.PartKeyForms);
            // WarpDeformerKeyforms
            new MocWarpDeformerKeyFormOffset().Read(reader, (int)counter.WarpDeformerKeyForms);
            // RotationDeformerKeyforms
            new MocRotationDeformerKeyFormOffset().Read(reader, (int)counter.RotationDeformerKeyForms);
            // ArtMeshKeyforms
            new MocArtMeshKeyFormOffset().Read(reader, (int)counter.ArtMeshKeyForms);
            // KeyformPositions
            new MocKeyFormPositionOffset().Read(reader, (int)counter.KeyFormPositions);
            // ParameterBindingIndices
            new MocParameterBindingIndicesOffset().Read(reader, (int)counter.ParameterBindingIndices);
            // KeyformBindings
            new MocKeyFormBindingOffset().Read(reader, (int)counter.KeyFormBindings);
            // ParameterBindings
            new MocParameterBindingOffset().Read(reader, (int)counter.ParameterBindings);
            // Keys
            new MocKeyOffset().Read(reader, (int)counter.Keys);

            // UVS
            var uvItems = new MocUVOffset();
            uvItems.Read(reader, (int)counter.Uvs);

            new MocPositionIndicesOffset().Read(reader, (int)counter.PositionIndices);
            new MocDrawableMaskOffset().Read(reader, (int)counter.DrawableMasks);
            new MocDrawOrderGroupOffset().Read(reader, (int)counter.DrawOrderGroups);
            new MocDrawOrderGroupObjectOffset().Read(reader, (int)counter.DrawOrderGroupObjects);
            new MocGlueOffset().Read(reader, (int)counter.Glue);
            new MocGlueInfoOffset().Read(reader, (int)counter.GlueInfo);
            new MocGlueKeyFormOffset().Read(reader, (int)counter.GlueKeyForms);
            if (header.Version >= MocVersion.V3_03_00)
            {
                new MocWarpDeformerKeyFormOffsetV3_3().Read(reader, (int)counter.WarpDeformers);
            }
            if (header.Version >= MocVersion.V4_02_00)
            {
                new MocParameterExtensionOffset().Read(reader, (int)counter.Parameters);
                new MocWarpDeformerKeyFormOffsetV4_2().Read(reader, (int)counter.WarpDeformers);
                new MocRotationDeformerOffsetV4_2().Read(reader, (int)counter.RotationDeformers);
                new MocArtMeshOffsetV4_2().Read(reader, (int)counter.ArtMeshes);
                new MocKeyFormColorOffset().Read(reader, (int)counter.KeyFormMultiplyColors);
                new MocKeyFormColorOffset().Read(reader, (int)counter.KeyFormScreenColors);
                new MocParameterOffsetsV4_2().Read(reader, (int)counter.Parameters);
                new MocBlendShapeParameterBindingOffset().Read(reader, (int)counter.BlendShapeParameterBindings);
                new MocBlendShapeKeyFormBindingOffset().Read(reader, (int)counter.BlendShapeKeyFormBindings);
                new MocBlendShapeOffset().Read(reader, (int)counter.BlendShapesWarpDeformers);
                new MocBlendShapeOffset().Read(reader, (int)counter.BlendShapesArtMeshes);
                new MocBlendShapeConstraintIndicesOffset().Read(reader, (int)counter.BlendShapeConstraintIndices);
                new MocBlendShapeConstraintOffset().Read(reader, (int)counter.BlendShapeConstraints);
                new MocBlendShapeConstraintValueOffset().Read(reader, (int)counter.BlendShapeConstraintValues);
            }
            if (header.Version >= MocVersion.V5_00_00)
            {
                new MocWarpDeformerKeyFormOffsetV5_0().Read(reader, (int)counter.WarpDeformerKeyForms);
                new MocRotationDeformerKeyFormOffsetsV5_0().Read(reader, (int)counter.RotationDeformerKeyForms);
                new MocArtMeshKeyFormOffsetsV5_0().Read(reader, (int)counter.ArtMeshKeyForms);
                new MocBlendShapeOffset().Read(reader, (int)counter.BlendShapesParts);
                new MocBlendShapeOffset().Read(reader, (int)counter.BlendShapesRotationDeformers);
                new MocBlendShapeOffset().Read(reader, (int)counter.BlendShapesGlue);
            }
            return null;
        }

    }
}
