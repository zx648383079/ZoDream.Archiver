using System;

namespace ZoDream.Shared.Drawing
{
    public readonly ref struct SizeSpan<T>
    {
        public SizeSpan(Span<T> data, int width, int height)
        {
            Width = width;
            Height = height;
            Data = data;
        }
        public int Width { get; }
        public int Height { get; }
        public Span<T> Data { get; }
        public T this[int x, int y] 
        {
            get => Data[y * Width + x];
            set => Data[y * Width + x] = value;
        }
    }
}
