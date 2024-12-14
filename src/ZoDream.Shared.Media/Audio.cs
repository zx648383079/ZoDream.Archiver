using FFmpeg.AutoGen;
using System.IO;

namespace ZoDream.Shared.Media
{
    public static class Audio
    {

        public static byte[] Decode(byte[] input)
        {
            return Decode(input, AVCodecID.AV_CODEC_ID_FMVC, AVCodecID.AV_CODEC_ID_WAVARC);
        }



        public static byte[] Decode(byte[] input, AVCodecID inputCodecId, AVCodecID outputCodecId)
        {
            using var decoder = MediaCodec.Create(inputCodecId);
            using var encoder = MediaCodec.Create(outputCodecId);
            using var frame = decoder.Decode(input);
            return encoder.Encode(frame);
        }

        public static int Decode(Stream input, AVCodecID inputCodecId, Stream output)
        {
            return Decode(input, inputCodecId, output, AVCodecID.AV_CODEC_ID_WAVARC);
        }

        public static int Decode(Stream input, 
            AVCodecID inputCodecId, 
            Stream output,
            AVCodecID outputCodecId)
        {
            using var decoder = MediaCodec.Create(inputCodecId);
            using var encoder = MediaCodec.Create(outputCodecId);
            using var frame = decoder.Decode(input);
            return encoder.Encode(frame, output);
        }
    }
}
