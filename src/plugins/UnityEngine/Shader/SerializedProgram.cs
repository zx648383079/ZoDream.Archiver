
namespace UnityEngine
{
    public class SerializedProgram
    {
        public SerializedSubProgram[] SubPrograms;
        public SerializedPlayerSubProgram[][] PlayerSubPrograms;
        public uint[][] ParameterBlobIndices;
        public SerializedProgramParameters CommonParameters;
        public ushort[] SerializedKeywordStateMask;

    }

}
