namespace UnityEngine
{
    public class MeshLodInfo
    {

        public LodSelectionCurve LodSelectionCurve;

        public int NumLevels;
    }

    public class LodSelectionCurve
    {

        public float LodSlope;

        public float LodBias;
    }
}
