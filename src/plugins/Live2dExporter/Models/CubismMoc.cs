using System.Numerics;

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

        public Vector2 CanvasSize => new(CanvasInfo.CanvasWidth, CanvasInfo.CanvasHeight);

        public string[] ParameterIds => [];

        public string[] PartIds => [];

        public string[] DrawableIds => [];


        public int DrawableCount => 0;



        public int GetDrawableVertexCount(int drawableIndex)
        {
            return 0;
        }

        public Vector2[] GetDrawableVertexPositions(int drawableIndex)
        {
            return [];
        }

        public Vector2[] GetDrawableVertexPositions(int drawableIndex, int vertexCount)
        {
            var data = GetDrawableVertexPositions(drawableIndex);
            var res = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                res[i] = data[i];
            }
            return res;
        }

        public Vector2[] GetDrawableVertexUvs(int drawableIndex)
        {
            return [];
        }

        public Vector2[] GetDrawableVertexUvs(int drawableIndex, int vertexCount)
        {
            var data = GetDrawableVertexUvs(drawableIndex);
            var res = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                res[i] = data[i];
            }
            return res;
        }
    }
}
