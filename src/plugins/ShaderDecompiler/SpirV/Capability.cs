﻿namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum Capability
    {
        Matrix = 0,
        Shader = 1,
        Geometry = 2,
        Tessellation = 3,
        Addresses = 4,
        Linkage = 5,
        Kernel = 6,
        Vector16 = 7,
        Float16Buffer = 8,
        Float16 = 9,
        Float64 = 10,
        Int64 = 11,
        Int64Atomics = 12,
        ImageBasic = 13,
        ImageReadWrite = 14,
        ImageMipmap = 15,
        Pipes = 17,
        Groups = 18,
        DeviceEnqueue = 19,
        LiteralSampler = 20,
        AtomicStorage = 21,
        Int16 = 22,
        TessellationPointSize = 23,
        GeometryPointSize = 24,
        ImageGatherExtended = 25,
        StorageImageMultisample = 27,
        UniformBufferArrayDynamicIndexing = 28,
        SampledImageArrayDynamicIndexing = 29,
        StorageBufferArrayDynamicIndexing = 30,
        StorageImageArrayDynamicIndexing = 31,
        ClipDistance = 32,
        CullDistance = 33,
        ImageCubeArray = 34,
        SampleRateShading = 35,
        ImageRect = 36,
        SampledRect = 37,
        GenericPointer = 38,
        Int8 = 39,
        InputAttachment = 40,
        SparseResidency = 41,
        MinLod = 42,
        Sampled1D = 43,
        Image1D = 44,
        SampledCubeArray = 45,
        SampledBuffer = 46,
        ImageBuffer = 47,
        ImageMSArray = 48,
        StorageImageExtendedFormats = 49,
        ImageQuery = 50,
        DerivativeControl = 51,
        InterpolationFunction = 52,
        TransformFeedback = 53,
        GeometryStreams = 54,
        StorageImageReadWithoutFormat = 55,
        StorageImageWriteWithoutFormat = 56,
        MultiViewport = 57,
        SubgroupDispatch = 58,
        NamedBarrier = 59,
        PipeStorage = 60,
        SubgroupBallotKHR = 4423,
        DrawParameters = 4427,
        SubgroupVoteKHR = 4431,
        StorageBuffer16BitAccess = 4433,
        StorageUniformBufferBlock16 = 4433,
        StorageUniform16 = 4434,
        UniformAndStorageBuffer16BitAccess = 4434,
        StoragePushConstant16 = 4435,
        StorageInputOutput16 = 4436,
        DeviceGroup = 4437,
        MultiView = 4439,
        VariablePointersStorageBuffer = 4441,
        VariablePointers = 4442,
        AtomicStorageOps = 4445,
        SampleMaskPostDepthCoverage = 4447,
        ImageGatherBiasLodAMD = 5009,
        FragmentMaskAMD = 5010,
        StencilExportEXT = 5013,
        ImageReadWriteLodAMD = 5015,
        SampleMaskOverrideCoverageNV = 5249,
        GeometryShaderPassthroughNV = 5251,
        ShaderViewportIndexLayerEXT = 5254,
        ShaderViewportIndexLayerNV = 5254,
        ShaderViewportMaskNV = 5255,
        ShaderStereoViewNV = 5259,
        PerViewAttributesNV = 5260,
        SubgroupShuffleINTEL = 5568,
        SubgroupBufferBlockIOINTEL = 5569,
        SubgroupImageBlockIOINTEL = 5570,
    }
}
