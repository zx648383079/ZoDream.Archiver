using System.IO;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocCountInfoTable
    {
        public uint Parts { get; set; }
        public uint Deformers { get; set; }
        public uint WarpDeformers { get; set; }
        public uint RotationDeformers { get; set; }
        public uint ArtMeshes { get; set; }
        public uint Parameters { get; set; }
        public uint PartKeyForms { get; set; }
        public uint WarpDeformerKeyForms { get; set; }
        public uint RotationDeformerKeyForms { get; set; }
        public uint ArtMeshKeyForms { get; set; }
        public uint KeyFormPositions { get; set; }
        public uint ParameterBindingIndices { get; set; }
        public uint KeyFormBindings { get; set; }
        public uint ParameterBindings { get; set; }
        public uint Keys { get; set; }
        public uint Uvs { get; set; }
        public uint PositionIndices { get; set; }
        public uint DrawableMasks { get; set; }
        public uint DrawOrderGroups { get; set; }
        public uint DrawOrderGroupObjects { get; set; }
        public uint Glue { get; set; }
        public uint GlueInfo { get; set; }
        public uint GlueKeyForms { get; set; }

        #region V4_02
        public uint KeyFormMultiplyColors { get; set; }
        public uint KeyFormScreenColors { get; set; }
        public uint BlendShapeParameterBindings { get; set; }
        public uint BlendShapeKeyFormBindings { get; set; }
        public uint BlendShapesWarpDeformers { get; set; }
        public uint BlendShapesArtMeshes { get; set; }
        public uint BlendShapeConstraintIndices { get; set; }
        public uint BlendShapeConstraints { get; set; }
        public uint BlendShapeConstraintValues { get; set; }
        #endregion
        #region V5
        public uint BlendShapesParts { get; set; }
        public uint BlendShapesRotationDeformers { get; set; }
        public uint BlendShapesGlue { get; set; }
        #endregion

        public void Read(BinaryReader reader, MocVersion version)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(ptr, SeekOrigin.Begin);

            Parts = reader.ReadUInt32();
            Deformers = reader.ReadUInt32();
            WarpDeformers = reader.ReadUInt32();
            RotationDeformers = reader.ReadUInt32();
            ArtMeshes = reader.ReadUInt32();
            Parameters = reader.ReadUInt32();
            PartKeyForms = reader.ReadUInt32();
            WarpDeformerKeyForms = reader.ReadUInt32();
            RotationDeformerKeyForms = reader.ReadUInt32();
            ArtMeshKeyForms = reader.ReadUInt32();
            KeyFormPositions = reader.ReadUInt32();
            ParameterBindingIndices = reader.ReadUInt32();
            KeyFormBindings = reader.ReadUInt32();
            ParameterBindings = reader.ReadUInt32();
            Keys = reader.ReadUInt32();
            Uvs = reader.ReadUInt32();
            PositionIndices = reader.ReadUInt32();
            DrawableMasks = reader.ReadUInt32();
            DrawOrderGroups = reader.ReadUInt32();
            DrawOrderGroupObjects = reader.ReadUInt32();
            Glue = reader.ReadUInt32();
            GlueInfo = reader.ReadUInt32();
            GlueKeyForms = reader.ReadUInt32();

            if (version >= MocVersion.V4_02_00)
            {
                KeyFormMultiplyColors = reader.ReadUInt32();
                KeyFormScreenColors = reader.ReadUInt32();
                BlendShapeParameterBindings = reader.ReadUInt32();
                BlendShapeKeyFormBindings = reader.ReadUInt32();
                BlendShapesWarpDeformers = reader.ReadUInt32();
                BlendShapesArtMeshes = reader.ReadUInt32();
                BlendShapeConstraintIndices = reader.ReadUInt32();
                BlendShapeConstraints = reader.ReadUInt32();
                BlendShapeConstraintValues = reader.ReadUInt32();
            }
            if (version >= MocVersion.V5_00_00)
            {
                BlendShapesParts = reader.ReadUInt32();
                BlendShapesRotationDeformers = reader.ReadUInt32();
                BlendShapesGlue = reader.ReadUInt32();
            }

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
