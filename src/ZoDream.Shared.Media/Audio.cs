using FFmpeg.AutoGen.Abstractions;
using FFmpegSharp;
using System.IO;
using System.Linq;

namespace ZoDream.Shared.Media
{
    public static class Audio
    {

        public static byte[] Decode(byte[] input)
        {
            return Decode(input, AVCodecID.AV_CODEC_ID_FMVC, AVCodecID.AV_CODEC_ID_FIRST_AUDIO);
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
            return Decode(input, inputCodecId, output, AVCodecID.AV_CODEC_ID_FIRST_AUDIO);
        }

        public static int Decode(Stream input, 
            AVCodecID inputCodecId, 
            Stream output,
            AVCodecID outputCodecId)
        {
            // var inputCodec = MediaCodec.FindDecoder(inputCodecId);
            using (var reader = MediaDemuxer.Open(input))
            using (var writer = MediaMuxer.Create(output, OutputFormat.GetFormats().Where(i => i.AudioCodec == outputCodecId).First()))
            {
                reader.DumpFormat();
                var audioSrc = reader.FirstOrDefault(_ => _.CodecparRef.codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO);
                if (audioSrc is null) 
                {
                    return 0;
                }
                using (var decoder = MediaDecoder.CreateDecoder(audioSrc.CodecparRef))
                //using (var encoder = MediaEncoder.CreateAudioEncoder(outputCodecId, audioSrc.CodecparRef.sample_rate, audioSrc.CodecparRef.ch_layout.nb_channels))
                using (var encoder = MediaEncoder.CreateAudioEncoder(writer.Format, decoder.SampleRate, decoder.ChLayout, AVSampleFormat.AV_SAMPLE_FMT_FLT))
                using (var cvt = SampleConverter.Create(decoder.ChLayout, decoder.SampleRate, AVSampleFormat.AV_SAMPLE_FMT_FLT, decoder.FrameSize))
                using (var pkt = new MediaPacket())
                using (var frame = new MediaFrame())
                {
                    writer.AddStream(encoder);
                    writer.WriteHeader();
                    foreach (var packet in reader.ReadPackets(pkt))
                    {
                        if (packet.StreamIndex == audioSrc.Index)
                        {
                            foreach (var f in decoder.DecodePacket(packet, frame))
                            {
                                foreach (var o in cvt.Convert(f))
                                {
                                    // WriteAudioOut(o, audioOutput);
                                    foreach (var outputPkt in encoder.EncodeFrame(o))
                                    {
                                        writer.WritePacket(outputPkt);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (int)output.Position;
        }


        //public static int Decode(string fileName)
        //{
        //    using var fs = File.OpenRead(fileName);
        //    using var output = File.OpenWrite(fileName + ".wav");
        //    try
        //    {
        //        return Decode(fs, AVCodecID.AV_CODEC_ID_VORBIS, output, AVCodecID.AV_CODEC_ID_FIRST_AUDIO);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return -1;
        //    }
        //}
    }
}
