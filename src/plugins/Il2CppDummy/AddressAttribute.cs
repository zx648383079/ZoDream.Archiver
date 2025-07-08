using System;

namespace Il2CppDummy
{
    public class AddressAttribute : Attribute
    {
        public string RVA;

        public string Offset;

        public string VA;

        public string Slot;
    }
}
