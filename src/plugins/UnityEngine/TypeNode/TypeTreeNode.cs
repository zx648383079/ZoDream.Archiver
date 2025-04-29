namespace UnityEngine
{
    public class TypeTreeNode
    {
        /// <summary>
        /// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.<br/>
        /// Equal to serializedVersion in Yaml format files
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Depth of current type relative to root
        /// </summary>
        public byte Level { get; set; }
        /// <summary>
        /// Array flag, set to 1 if type is "Array" or "TypelessData".
        /// </summary>
        public int TypeFlags { get; set; }

        /// <summary>
        /// Name of the data type. This can be the name of any substructure or a static predefined type.
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Name of the field.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Size of the data value in bytes, e.g. 4 for int. -1 means that there is an array somewhere inside its hierarchy<br/>
        /// Note: The padding for the alignment is not included in the size.
        /// </summary>
        public int ByteSize { get; set; }
        /// <summary>
        /// Index of the field that is unique within a tree.<br/>
        /// Normally starts with 0 and is incremented with each additional field.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Metaflags of the field
        /// </summary>
        public TransferMetaFlags MetaFlag { get; set; }
        public ulong RefTypeHash { get; set; }

        public TypeTreeNode[]? Children { get; set; }
    }
}
