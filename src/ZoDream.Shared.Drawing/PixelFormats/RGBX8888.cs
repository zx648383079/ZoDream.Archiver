namespace ZoDream.Shared.Drawing
{
    public class RGBX8888(): RGBASwapDecoder(new ColorSwapper([ColorChannel.R, ColorChannel.G, ColorChannel.B, ColorChannel.X]))
    {
    }
}
