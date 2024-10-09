using FFmpeg.AutoGen;
using System;

namespace ZoDream.Shared.Media
{
    internal unsafe class MediaFrame: IDisposable
    {

        public MediaFrame(AVFrame* pFrame)
        {
            _pFrame = pFrame;
        }

        public MediaFrame()
            : this(ffmpeg.av_frame_alloc())
        {
            
        }


        private readonly AVFrame* _pFrame;

        public bool IsAudioFrame => _pFrame->nb_samples > 0 && _pFrame->ch_layout.nb_channels > 0;
        public bool IsVideoFrame => _pFrame->width > 0 && _pFrame->height > 0;

        public void AllocateBuffer(int align = 0)
        {
            ffmpeg.av_frame_get_buffer(_pFrame, align);
        }
        /// <summary>
        /// 用于重置 AVFrame 结构体的内部指针，以防止内存泄漏。在处理完一个 AVFrame 之后，应该调用 av_frame_unref 来清理该结构体。
        /// </summary>
        public void Unref()
        {
            ffmpeg.av_frame_unref(_pFrame);
        }

        public int Receive(AVCodecContext* pDecodecContext)
        {
            return ffmpeg.avcodec_receive_frame(pDecodecContext, _pFrame);
        }

        public void Send(AVCodecContext* pDecodecContext)
        {
            ffmpeg.avcodec_send_frame(pDecodecContext, _pFrame);
        }

        public void Dispose()
        {
            Unref();
            fixed (AVFrame** ppFrame = &_pFrame)
            {
                ffmpeg.av_frame_free(ppFrame);
            }
        }


        public static MediaFrame CreateVideoFrame(int width, int height, AVPixelFormat pixelFormat, int align = 0)
        {
            var f = new MediaFrame();
            f._pFrame->format = (int)pixelFormat;
            f._pFrame->width = width;
            f._pFrame->height = height;
            f.AllocateBuffer(align);
            return f;
        }

        public static MediaFrame CreateAudioFrame(int channels, int nbSamples, AVSampleFormat format, int sampleRate = 0, int align = 0)
        {
            var f = new MediaFrame();
            f._pFrame->format = (int)format;
            ffmpeg.av_channel_layout_default(&f._pFrame->ch_layout, channels);
            f._pFrame->nb_samples = nbSamples;
            f._pFrame->sample_rate = sampleRate;
            f.AllocateBuffer(align);
            return f;
        }

        public static MediaFrame CreateAudioFrame(AVChannelLayout channelLayout, int nbSamples, AVSampleFormat format, int sampleRate = 0, int align = 0)
        {
            return CreateAudioFrame(channelLayout.nb_channels, nbSamples, format, sampleRate, align);
        }

        
    }
}
