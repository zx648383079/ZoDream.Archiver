using FFmpeg.AutoGen;
using FFmpegSharp;
using System.IO;
using System.Linq;

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
            using var i = new MemoryStream(input);
            using var ms = new MemoryStream();
            Decode(i, inputCodecId, ms, outputCodecId);
            return ms.ToArray();
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
            // var inputCodec = MediaCodec.FindDecoder(inputCodecId);
            using var reader = MediaDemuxer.Open(input);
            using var writer = MediaMuxer.Create(output, OutputFormat.GetFormats().Where(i => i.AudioCodec == outputCodecId).First());
            var audioIndex = reader.First(_ => _.CodecparRef.codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO).Index;
            var encoder = MediaEncoder.CreateAudioEncoder(
                outputCodecId,
                reader[audioIndex].CodecparRef.sample_rate,
                reader[audioIndex].CodecparRef.ch_layout.nb_channels
                );
            // reader.FindBestStream(AVMediaType.AVMEDIA_TYPE_AUDIO, ref inputCodec)
            writer.AddStream(
                encoder
            );
            writer.WriteHeader();
            var decoder = MediaDecoder.CreateDecoder(reader[audioIndex].CodecparRef);
            var converter = new SampleConverter();
            long pts = 0;
            foreach (var packet in reader.ReadPackets())
            {
                foreach (var srcFrame in decoder.DecodePacket(packet))
                {
                    foreach (var dstFrame in converter.Convert(srcFrame))
                    {
                        pts += dstFrame.Const.nb_samples;
                        dstFrame.Pts = pts; // audio's pts is total samples, pts can only increase.
                        foreach (var outPacket in encoder.EncodeFrame(dstFrame))
                        {
                            writer.WritePacket(outPacket);
                        }
                    }
                }
            }
            writer.FlushCodecs([encoder]);
            return (int)output.Position;
        }

    }
}
