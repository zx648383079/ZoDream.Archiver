using System;

namespace UnityEngine.Document
{
    [Flags]
    public enum TransferMetaFlags : uint
    {
        NoTransferFlags = 0x0,
        /// <summary>
        /// Putting this mask in a transfer will make the variable be hidden in the property editor.
        /// </summary>
        HideInEditor = 0x1,
        Unknown1 = 0x2,
        Unknown2 = 0x4,
        Unknown3 = 0x8,
        /// <summary>
        /// Makes a variable not editable in the property editor.
        /// </summary>
        NotEditable = 0x10,
        Unknown5 = 0x20,
        /// <summary>
        /// There are 3 types of PPtrs: <see cref="StrongPPtr"/>, default (weak pointer)
        /// a Strong PPtr forces the referenced object to be cloned.
        /// A Weak PPtr doesnt clone the referenced object, but if the referenced object is being cloned anyway (eg. If another (strong) pptr references this object)
        /// this PPtr will be remapped to the cloned object.
        /// If an object referenced by a WeakPPtr is not cloned, it will stay the same when duplicating and cloning, but be NULLed when templating.
        /// </summary>
        StrongPPtr = 0x40,
        Unknown7 = 0x80,
        /// <summary>
        /// Makes an integer variable appear as a checkbox in the editor.
        /// </summary>
        TreatIntegerValueAsBoolean = 0x100,
        Unknown9 = 0x200,
        Unknown10 = 0x400,
        /// <summary>
        /// Show in simplified editor
        /// </summary>
        SimpleEditor = 0x800,
        /// <summary>
        /// For when the options of a serializer tell you to serialize debug properties (<see cref="TransferInstructionFlags.SerializeDebugProperties"/>).
        /// All debug properties have to be marked <see cref="DebugProperty"/>.
        /// Debug properties are shown in expert mode in the inspector but are not serialized normally.
        /// </summary>
        DebugProperty = 0x1000,
        Unknown13 = 0x2000,
        AlignBytes = 0x4000,
        AnyChildUsesAlignBytes = 0x8000,
        IgnoreWithInspectorUndo = 0x10000,
        Unknown17 = 0x20000,
        EditorDisplaysCharacterMap = 0x40000,
        /// <summary>
        /// Ignore this property when reading or writing .meta files
        /// </summary>
        IgnoreInMetaFiles = 0x80000,
        /// <summary>
        /// When reading meta files and this property is not present, read array entry name instead (for backwards compatibility).
        /// </summary>
        TransferAsArrayEntryNameInMetaFiles = 0x100000,
        /// <summary>
        /// When writing YAML Files, uses the flow mapping style (all properties in one line, with "{}").
        /// </summary>
        TransferUsingFlowMappingStyle = 0x200000,
        /// <summary>
        /// Tells SerializedProperty to generate bitwise difference information for this field.
        /// </summary>
        GenerateBitwiseDifferences = 0x400000,
        DontAnimate = 0x800000,
        TransferHex64 = 0x1000000,
        CharPropertyMask = 0x2000000,
        DontValidateUTF8 = 0x4000000,
        FixedBuffer = 0x8000000,
        DisallowSerializedPropertyModification = 0x10000000,
        Unknown29 = 0x20000000,
        Unknown30 = 0x40000000,
        Unknown31 = 0x80000000,
    }

}
