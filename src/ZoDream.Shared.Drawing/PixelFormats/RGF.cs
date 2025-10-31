namespace ZoDream.Shared.Drawing
{
    public class RGF() : FloatSwapDecoder(new ColorSwapper([ColorChannel.R, ColorChannel.G]))
    {
    }
}
