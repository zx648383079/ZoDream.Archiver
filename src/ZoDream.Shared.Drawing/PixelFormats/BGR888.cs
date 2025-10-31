namespace ZoDream.Shared.Drawing
{
    public class BGR888() : RGBASwapDecoder(new ColorSwapper([ColorChannel.B, ColorChannel.G, ColorChannel.R]))
    {
    }
}
