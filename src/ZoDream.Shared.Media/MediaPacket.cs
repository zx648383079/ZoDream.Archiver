using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Media
{
    internal unsafe class MediaPacket: IDisposable
    {

        public MediaPacket(AVPacket* pAVPacket)
        {
            _pPacket = pAVPacket;
        }

        public MediaPacket()
            : this(ffmpeg.av_packet_alloc())
        {
            
        }

        public MediaPacket(byte[] buffer)
            : this()
        {
            fixed (byte* waitDecodeData = buffer)
            {
                _pPacket->data = waitDecodeData;
                _pPacket->size = buffer.Length;
            }
        }

        private readonly AVPacket* _pPacket;

        public void Unref()
        {
            ffmpeg.av_packet_unref(_pPacket);
        }

        public void Send(AVCodecContext* pDecodecContext)
        {
            ffmpeg.avcodec_send_packet(pDecodecContext, _pPacket);
        }

        public int Receive(AVCodecContext* pDecodecContext)
        {
            return ffmpeg.avcodec_receive_packet(pDecodecContext, _pPacket);
        }

        public byte[] ToArray()
        {
            var buffer = new byte[_pPacket->size];
            Marshal.Copy(new IntPtr(_pPacket->data), buffer, 0, _pPacket->size);
            return buffer;
        }

        public void Dispose()
        {
            Unref();
            fixed (AVPacket** ppPacket = &_pPacket)
            {
                ffmpeg.av_packet_free(ppPacket);
            }
        }
    }
}
