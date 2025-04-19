using System.Collections.Generic;

namespace UnityEngine
{
    public class SerializedPass
    {
        public Hash128[] EditorDataHash;
        public byte[] Platforms;
        public ushort[] LocalKeywordMask;
        public ushort[] GlobalKeywordMask;
        public KeyValuePair<string, int>[] NameIndices;
        public PassType Type;
        public SerializedShaderState State;
        public uint ProgramMask;
        public SerializedProgram progVertex;
        public SerializedProgram progFragment;
        public SerializedProgram progGeometry;
        public SerializedProgram progHull;
        public SerializedProgram progDomain;
        public SerializedProgram progRayTracing;
        public bool HasInstancingVariant;
        public string UseName;
        public string Name;
        public string TextureName;
        public SerializedTagMap Tags;
        public ushort[] SerializedKeywordStateMask;

    }

}
