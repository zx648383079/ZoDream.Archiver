using System;

namespace ZoDream.ShaderDecompiler.SpirV
{
    [Flags]
    public enum DisassemblyOptions
    {
        None,
        ShowTypes,
        ShowNames,
        Default = ShowTypes | ShowNames
    }
    internal class SpvGenerator
    {
        public SpvGenerator(string vendor)
        {
            Vendor = vendor;
        }

        public SpvGenerator(string vendor, string name)
            : this(vendor)
        {
            Name = name;
        }

        public string? Name { get; }
        public string Vendor { get; }

        public static SpvGenerator[] Items = [
            new("Khronos"),
            new("LunarG"),
            new("Valve"),
            new("Codeplay"),
            new("NVIDIA"),
            new("ARM"),
            new("Khronos", "LLVM/SPIR-V Translator"),
            new("Khronos", "SPIR-V Tools Assembler"),
            new("Khronos", "Glslang Reference Front End"),
            new("Qualcomm"),
            new("AMD"),
            new("Intel"),
            new("Imagination"),
            new("Google", "Shaderc over Glslang"),
            new("Google", "spiregg"),
            new("Google", "rspirv"),
            new("X-LEGEND", "Mesa-IR/SPIR-V Translator"),
            new("Khronos", "SPIR-V Tools Linker"),
        ];
    }
}
