namespace ZoDream.KhronosExporter.Models
{
    internal class ObjFactorColor
    {
        public double Red { get; set; }

        public double Green { get; set; }

        public double Blue { get; set; }

        public ObjFactorColor()
        {

        }
        public ObjFactorColor(double v)
        {
            Red = v;
            Green = v;
            Blue = v;
        }

        public ObjFactorColor(double r, double g, double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
    }
}