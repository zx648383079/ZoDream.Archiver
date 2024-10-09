using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZoDream.Shared.Media
{
    /// <summary>
    /// 解码器
    /// </summary>
    internal unsafe class MediaCodec: IDisposable
    {

        public MediaCodec(AVCodec* pCodec)
        {
            _pDecodec = pCodec;
            _pDecodecContext = ffmpeg.avcodec_alloc_context3(_pDecodec);
            ffmpeg.avcodec_open2(_pDecodecContext, _pDecodec, null);
        }

        public MediaCodec(AVCodec* pCodec, int width, int height)
        {
            _pDecodec = pCodec;
            _pDecodecContext = ffmpeg.avcodec_alloc_context3(_pDecodec);
            _pDecodecContext->width = width;
            _pDecodecContext->height = height;
            _pDecodecContext->time_base = new AVRational { num = 1, den = 30 };
            _pDecodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            _pDecodecContext->framerate = new AVRational { num = 30, den = 1 };
            _pDecodecContext->gop_size = 30;
            // 设置预测算法
            _pDecodecContext->flags |= ffmpeg.AV_CODEC_FLAG_PSNR;
            _pDecodecContext->flags2 |= ffmpeg.AV_CODEC_FLAG2_FAST;
            _pDecodecContext->max_b_frames = 0;
            ffmpeg.av_opt_set(_pDecodecContext->priv_data, "preset", "veryfast", 0);
            ffmpeg.av_opt_set(_pDecodecContext->priv_data, "tune", "zerolatency", 0);

            ffmpeg.avcodec_open2(_pDecodecContext, _pDecodec, null);
            //_pConvertContext = ffmpeg.sws_getContext(
            //    width,
            //    height,
            //    AVPixelFormat.AV_PIX_FMT_YUV420P,
            //    width,
            //    height,
            //    true ? AVPixelFormat.AV_PIX_FMT_RGB24 : AVPixelFormat.AV_PIX_FMT_BGRA,
            //   ffmpeg.SWS_FAST_BILINEAR,
            //    null, null, null);
        }

        //解码器
        private AVCodec* _pDecodec;
        private AVCodecContext* _pDecodecContext;

        public MediaFrame Decode(byte[] buffer)
        {
            using var packet = new MediaPacket(buffer);
            var frame = new MediaFrame();
            frame.Unref();
            int error;
            do
            {
                packet.Send(_pDecodecContext);
                error = frame.Receive(_pDecodecContext);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            return frame;
        }

        public byte[] Encode(MediaFrame frame)
        {
            using var packet = new MediaPacket();
            int error;
            packet.Unref();
            do
            {
                frame.Send(_pDecodecContext);
                error = packet.Receive(_pDecodecContext);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            return packet.ToArray();
        }


        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            //释放解码器
            var pDecodecContext = _pDecodecContext;
            ffmpeg.avcodec_free_context(&pDecodecContext);
            ffmpeg.av_free(_pDecodecContext);
            //释放转换器
            //Marshal.FreeHGlobal(_convertedFrameBufferPtr);
            //ffmpeg.sws_freeContext(_pConvertContext);
        }

        public static MediaCodec Create(AVCodecID codecFormat, int width, int height)
        {
            return new MediaCodec(ffmpeg.avcodec_find_decoder(codecFormat), width, height);
        }

        public static MediaCodec Create(AVCodecID codecFormat)
        {
            return new MediaCodec(ffmpeg.avcodec_find_decoder(codecFormat));
        }
    }
}
