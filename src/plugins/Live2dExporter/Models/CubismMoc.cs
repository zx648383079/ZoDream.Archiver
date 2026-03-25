using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class CubismMoc
    {
        public MocHeader Header { get; set; } = new();
        public MocCountInfoTable CountInfo { get; internal set; } = new();
        public MocCanvasInfo CanvasInfo { get; internal set; } = new();
        public MocPartOffset Parts { get; internal set; } = new();
        public MocDeformerOffset Deformers { get; internal set; } = new();
        public MocWarpDeformerOffset WarpDeformers { get; internal set; } = new();
        public MocRotationDeformerOffset RotationDeformers { get; internal set; } = new();
        public MocArtMeshOffset ArtMeshes { get; internal set; } = new();
        public MocParameterOffset Parameters { get; internal set; } = new();

        public MocPartKeyFormOffset PartKeyforms { get; internal set; } = new();

        public MocWarpDeformerKeyFormOffset WarpDeformerKeyforms { get; internal set; } = new();
        public MocRotationDeformerKeyFormOffset RotationDeformerKeyforms { get; internal set; } = new();
        public MocArtMeshKeyFormOffset ArtMeshKeyforms { get; internal set; } = new();
        public MocKeyFormPositionOffset KeyformPositions { get; internal set; } = new();
        public MocParameterBindingIndicesOffset ParameterBindingIndices { get; internal set; } = new();
        public MocKeyFormBindingOffset KeyformBindings { get; internal set; } = new();
        public MocParameterBindingOffset ParameterBindings { get; internal set; } = new();

        public MocKeyOffset Keys { get; internal set; } = new();
        public MocUVOffset Uvs { get; internal set; } = new();
        public MocPositionIndicesOffset VertexIndices { get; internal set; } = new();
        public MocDrawableMaskOffset ArtMeshMasks { get; internal set; } = new();
        public MocDrawOrderGroupOffset DrawOrderGroups { get; internal set; } = new();
        public MocDrawOrderGroupObjectOffset DrawOrderGroupObjects { get; internal set; } = new();
        public MocGlueOffset Glues { get; internal set; } = new();
        public MocGlueInfoOffset GlueInfos { get; internal set; } = new();
        public MocGlueKeyFormOffset GlueKeyforms { get; internal set; } = new();


        public MocWarpDeformerKeyFormOffsetV3_3 WarpDeformerKeyforms_v303 { get; internal set; } = new();
        public MocParameterExtensionOffset ParameterExtensions { get; internal set; } = new();
        public MocWarpDeformerKeyFormOffsetV4_2 WarpDeformerKeyforms_v402 { get; internal set; } = new();
        public MocRotationDeformerOffsetV4_2 RotationDeformerKeyforms_v402 { get; internal set; } = new();
        public MocArtMeshOffsetV4_2 ArtMeshDeformerKeyforms_v402 { get; internal set; } = new();
        public MocKeyFormColorOffset KeyformMultiplyColors { get; internal set; } = new();
        public MocKeyFormColorOffset KeyformScreenColors { get; internal set; } = new();

        public MocParameterOffsetsV4_2 Parameters_v402 { get; internal set; } = new();

        public MocBlendShapeParameterBindingOffset BlendShapeParameterBindings { get; internal set; } = new();
        public MocBlendShapeKeyFormBindingOffset BlendShapeKeyformBindings { get; internal set; } = new();
        public MocBlendShapeOffset BlendShapeWarpDeformers { get; internal set; } = new();
        public MocBlendShapeOffset BlendShapeArtMeshes { get; internal set; } = new();
        public MocBlendShapeConstraintIndicesOffset BlendShapeConstraintIndices { get; internal set; } = new();
        public MocBlendShapeConstraintOffset BlendShapeConstraints { get; internal set; } = new();
        public MocBlendShapeConstraintValueOffset BlendShapeConstraintValues { get; internal set; } = new();
        public MocWarpDeformerKeyFormOffsetV5_0 WarpDeformerKeyforms_v50 { get; internal set; } = new();
        public MocRotationDeformerKeyFormOffsetsV5_0 RotationDeformerKeyforms_50 { get; internal set; } = new();
        public MocArtMeshKeyFormOffsetsV5_0 ArtMeshKeyforms_v50 { get; internal set; } = new();
        public MocBlendShapeOffset BlendShapesParts { get; internal set; } = new();
        public MocBlendShapeOffset BlendShapesRotationDeformers { get; internal set; } = new();
        public MocBlendShapeOffset BlendShapesGlue { get; internal set; } = new();


        public float[] RuntimeKeys => Keys.Values;
        public short[] RuntimeVertexIndices => VertexIndices.Indices;
        public Vector2[] RuntimePositions => KeyformPositions.Coords;
        public Vector2[] RuntimeUvs => Uvs.Uvs;

        public Vector2 CanvasSize => new(CanvasInfo.CanvasWidth / CanvasInfo.PixelsPerUnit, CanvasInfo.CanvasHeight / CanvasInfo.PixelsPerUnit);

        public string[] ParameterIds => Parameters.Ids;

        public string[] PartIds => Parts.Ids;

        public string[] DrawableIds => ArtMeshes.Ids;


        public int DrawableCount => (int)CountInfo.ArtMeshes;

        public int GetDrawableVertexCount(int drawableIndex)
        {
            return ArtMeshes.VertexCounts[drawableIndex];
        }

        public Vector2[] GetDrawableVertexPositions(int drawableIndex)
        {
            //var begin = ArtMeshes.PositionIndexSourcesBeginIndices[drawableIndex];
            //var count = ArtMeshes.PositionIndexSourcesCounts[drawableIndex];
            //var indices = RuntimeVertexIndices[begin..(begin + count)];
            var count = GetDrawableVertexCount(drawableIndex);
            var begin = ArtMeshes.KeyFormSourcesBeginIndices[drawableIndex];
            var vertexes = ArtMeshes.KeyFormSourcesCounts[drawableIndex];
            var res = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                var positionBegin = ArtMeshKeyforms.KeyFormPositionSourcesBeginIndices[begin + i];
                res.AddRange(RuntimePositions[positionBegin..(positionBegin + vertexes)]);
            }
            return [.. res];
        }

        public Vector2[] GetDrawableVertexPositions(int drawableIndex, int vertexCount)
        {
            return [.. GetDrawableVertexPositions(drawableIndex).Take(vertexCount)];
        }

        public Vector2[] GetDrawableVertexUvs(int drawableIndex)
        {
            var begin = ArtMeshes.UvSourcesBeginIndices[drawableIndex];
            var count = GetDrawableVertexCount(drawableIndex);
            return RuntimeUvs[begin..(begin + count)];
        }

        public Vector2[] GetDrawableVertexUvs(int drawableIndex, int vertexCount)
        {
            return [.. GetDrawableVertexUvs(drawableIndex).Take(vertexCount)];
        }
    }
}
