namespace ZoDream.SpineExporter.Models
{
    public abstract class CurveTimeline: Timeline
    {

        protected CurveTimeline(int frameCount)
        {
            Curves = new float[(frameCount - 1) * BEZIER_SIZE];
        }

        protected const float LINEAR = 0, STEPPED = 1, BEZIER = 2;
        protected const int BEZIER_SIZE = 10 * 2 - 1;
        abstract public int PropertyId { get; }
        public float[] Curves;
        public int FrameCount => Curves.Length / BEZIER_SIZE + 1;

        public void SetLinear(int frameIndex)
        {
            Curves[frameIndex * BEZIER_SIZE] = LINEAR;
        }

        public void SetStepped(int frameIndex)
        {
            Curves[frameIndex * BEZIER_SIZE] = STEPPED;
        }

        public void SetCurve(int frameIndex, float cx1, float cy1, float cx2, float cy2)
        {
            float tmpx = (-cx1 * 2 + cx2) * 0.03f, tmpy = (-cy1 * 2 + cy2) * 0.03f;
            float dddfx = ((cx1 - cx2) * 3 + 1) * 0.006f, dddfy = ((cy1 - cy2) * 3 + 1) * 0.006f;
            float ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
            float dfx = cx1 * 0.3f + tmpx + dddfx * 0.16666667f, dfy = cy1 * 0.3f + tmpy + dddfy * 0.16666667f;

            int i = frameIndex * BEZIER_SIZE;
            float[] curves = Curves;
            curves[i++] = BEZIER;

            float x = dfx, y = dfy;
            for (int n = i + BEZIER_SIZE - 1; i < n; i += 2)
            {
                curves[i] = x;
                curves[i + 1] = y;
                dfx += ddfx;
                dfy += ddfy;
                ddfx += dddfx;
                ddfy += dddfy;
                x += dfx;
                y += dfy;
            }
        }
    }
}
