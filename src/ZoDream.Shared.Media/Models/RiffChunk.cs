using System.Collections.Generic;

namespace ZoDream.Shared.Media.Models
{
    public interface IRiffChunk
    {
        public string Id { get; }
    }

    public class RiffChunk : IRiffChunk
    {
        public string Id { get; set; }

        public uint Length { get; set; }

        public string Type { get; set; }

        public IList<IRiffChunk> Items { get; set; } = [];
    }

    public class RiffSubChunk : IRiffChunk
    {
        public string Id { get; set; }

        public uint Length { get; set; }

        public long Position { get; set; }
    }
}
