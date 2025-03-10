using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.SpineExporter.Models
{
    public class Skin
    {
        public Dictionary<AttachmentKeyTuple, AttachmentBase> Attachments { get; set; } = [];

        public string Name { get; set; }

        public void Add(int slotIndex, string name, AttachmentBase attachment)
        {
            Attachments.TryAdd(new AttachmentKeyTuple(slotIndex, name), attachment);
        }

        public bool TryGet<T>(int slotIndex, string name, [NotNullWhen(true)] out T? attachment)
            where T : AttachmentBase
        {
            if (Attachments.TryGetValue(new AttachmentKeyTuple(slotIndex, name), out var res))
            {
                attachment = (T)res;
                return true;
            }
            attachment = null;
            return false;
        }
    }

    public class AttachmentKeyTuple(int slotIndex, string name)
    {
        public int SlotIndex => slotIndex;
        public string Name => name;

        public override string ToString()
        {
            return $"{slotIndex}-{name}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is AttachmentKeyTuple a)
            {
                return a.SlotIndex == SlotIndex && a.Name == Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return SlotIndex;
        }
    }
}
