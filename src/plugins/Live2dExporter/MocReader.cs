using System.IO;
using ZoDream.Live2dExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.Live2dExporter
{
    public class MocReader
    {
        private static CubismMoc? Read(Stream input, string[] textureItems)
        {
            var header = new MocHeader();
            if (!header.TryRead(input))
            {
                return null;
            }
            var res = new CubismMoc()
            {
                Header = header,
            };
            var reader = new BundleBinaryReader(input, header.IsBigEndian ? EndianType.BigEndian : EndianType.LittleEndian);
            var counter = new MocCountInfoTable();
            counter.Read(reader, header.Version);
            res.CountInfo = counter;
            // CanvasInfo
            res.CanvasInfo.Read(reader);
            // part
            res.Parts.Read(reader, (int)counter.Parts);
            // Deformers
            res.Deformers.Read(reader, (int)counter.Deformers);
            // WarpDeformers
            res.WarpDeformers.Read(reader, (int)counter.WarpDeformers);
            // RotationDeformers
            res.RotationDeformers.Read(reader, (int)counter.RotationDeformers);



            #region ArtMeshes
            res.ArtMeshes.Read(reader, (int)counter.ArtMeshes);

            #endregion

            // Parameters
            res.Parameters.Read(reader, (int)counter.Parameters);

            // PartKeyforms
            res.PartKeyforms.Read(reader, (int)counter.PartKeyForms);
            // WarpDeformerKeyforms
            res.WarpDeformerKeyforms.Read(reader, (int)counter.WarpDeformerKeyForms);
            // RotationDeformerKeyforms
            res.RotationDeformerKeyforms.Read(reader, (int)counter.RotationDeformerKeyForms);
            // ArtMeshKeyforms
            res.ArtMeshKeyforms.Read(reader, (int)counter.ArtMeshKeyForms);
            // KeyformPositions
            res.KeyformPositions.Read(reader, (int)counter.KeyFormPositions);
            // ParameterBindingIndices
            res.ParameterBindingIndices.Read(reader, (int)counter.ParameterBindingIndices);
            // KeyformBindings
            res.KeyformBindings.Read(reader, (int)counter.KeyFormBindings);
            // ParameterBindings
            res.ParameterBindings.Read(reader, (int)counter.ParameterBindings);
            // Keys
            res.Keys.Read(reader, (int)counter.Keys);

            // UVS
            res.Uvs.Read(reader, (int)counter.Uvs);

            res.VertexIndices.Read(reader, (int)counter.PositionIndices);
            res.ArtMeshMasks.Read(reader, (int)counter.DrawableMasks);
            res.DrawOrderGroups.Read(reader, (int)counter.DrawOrderGroups);
            res.DrawOrderGroupObjects.Read(reader, (int)counter.DrawOrderGroupObjects);
            res.Glues.Read(reader, (int)counter.Glue);
            res.GlueInfos.Read(reader, (int)counter.GlueInfo);
            res.GlueKeyforms.Read(reader, (int)counter.GlueKeyForms);
            if (header.Version >= MocVersion.V3_03_00)
            {
                res.WarpDeformerKeyforms_v303.Read(reader, (int)counter.WarpDeformers);
            }
            if (header.Version >= MocVersion.V4_02_00)
            {
                res.ParameterExtensions.Read(reader, (int)counter.Parameters);
                res.WarpDeformerKeyforms_v402.Read(reader, (int)counter.WarpDeformers);
                res.RotationDeformerKeyforms_v402.Read(reader, (int)counter.RotationDeformers);
                res.ArtMeshDeformerKeyforms_v402.Read(reader, (int)counter.ArtMeshes);
                res.KeyformMultiplyColors.Read(reader, (int)counter.KeyFormMultiplyColors);
                res.KeyformScreenColors.Read(reader, (int)counter.KeyFormScreenColors);
                res.Parameters_v402.Read(reader, (int)counter.Parameters);
                res.BlendShapeParameterBindings.Read(reader, (int)counter.BlendShapeParameterBindings);
                res.BlendShapeKeyformBindings.Read(reader, (int)counter.BlendShapeKeyFormBindings);
                res.BlendShapeWarpDeformers.Read(reader, (int)counter.BlendShapesWarpDeformers);
                res.BlendShapeArtMeshes.Read(reader, (int)counter.BlendShapesArtMeshes);
                res.BlendShapeConstraintIndices.Read(reader, (int)counter.BlendShapeConstraintIndices);
                res.BlendShapeConstraints.Read(reader, (int)counter.BlendShapeConstraints);
                res.BlendShapeConstraintValues.Read(reader, (int)counter.BlendShapeConstraintValues);
            }
            if (header.Version >= MocVersion.V5_00_00)
            {
                res.WarpDeformerKeyforms_v50.Read(reader, (int)counter.WarpDeformerKeyForms);
                res.RotationDeformerKeyforms_50.Read(reader, (int)counter.RotationDeformerKeyForms);
                res.ArtMeshKeyforms_v50.Read(reader, (int)counter.ArtMeshKeyForms);
                res.BlendShapesParts.Read(reader, (int)counter.BlendShapesParts);
                res.BlendShapesRotationDeformers.Read(reader, (int)counter.BlendShapesRotationDeformers);
                res.BlendShapesGlue.Read(reader, (int)counter.BlendShapesGlue);
            }
            return res;
        }

    }
}
