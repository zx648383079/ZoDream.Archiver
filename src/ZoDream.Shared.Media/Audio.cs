using FFmpeg.AutoGen;

namespace ZoDream.Shared.Media
{
    public static class Audio
    {

        public static byte[] Decode(byte[] input)
        {
            return Decode(input, AVCodecID.AV_CODEC_ID_FIRST_SUBTITLE, AVCodecID.AV_CODEC_ID_WAVARC);
        }

        public static byte[] Decode(byte[] input, AVCodecID inputCodecId, AVCodecID outputCodecId)
        {
            using var decoder = MediaCodec.Create(inputCodecId);
            using var encoder = MediaCodec.Create(outputCodecId);
            using var frame = decoder.Decode(input);
            return encoder.Encode(frame);
        }


    }
}
