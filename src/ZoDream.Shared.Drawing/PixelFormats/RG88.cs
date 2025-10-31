namespace ZoDream.Shared.Drawing
{
    public class RG88() : RGBASwapDecoder(new ColorSwapper([ColorChannel.R, ColorChannel.G]))
    {
    }
}
