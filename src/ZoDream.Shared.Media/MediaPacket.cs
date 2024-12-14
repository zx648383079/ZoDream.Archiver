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
            : this(buffer, buffer.Length)
        {
        }

        public MediaPacket(byte[] buffer, int length)
            : this()
        {
            fixed (byte* waitDecodeData = buffer)
            {
                _pPacket->data = waitDecodeData;
                _pPacket->size = length;
            }
        }

        private readonly AVPacket* _pPacket;

        public int Length => _pPacket->size;

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

        public void CopyTo(byte[] buffer)
        {
            Marshal.Copy(new IntPtr(_pPacket->data), buffer, 0, _pPacket->size);
        }

        public byte[] ToArray()
        {
            var buffer = new byte[Length];
            CopyTo(buffer);
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
