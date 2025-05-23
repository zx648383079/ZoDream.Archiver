﻿using System.Collections.Generic;
using UnityEngine.Document;

namespace ZoDream.BundleExtractor.Unity.Document
{
    
    public static class TransferMetaFlagsExtensions
    {
        public static bool IsHideInEditor(this TransferMetaFlags _this) => (_this & TransferMetaFlags.HideInEditor) != 0;
        public static bool IsNotEditable(this TransferMetaFlags _this) => (_this & TransferMetaFlags.NotEditable) != 0;
        public static bool IsStrongPPtr(this TransferMetaFlags _this) => (_this & TransferMetaFlags.StrongPPtr) != 0;
        public static bool IsTreatIntegerValueAsBoolean(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TreatIntegerValueAsBoolean) != 0;
        public static bool IsSimpleEditor(this TransferMetaFlags _this) => (_this & TransferMetaFlags.SimpleEditor) != 0;
        public static bool IsDebugProperty(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DebugProperty) != 0;
        public static bool IsAlignBytes(this TransferMetaFlags _this) => (_this & TransferMetaFlags.AlignBytes) != 0;
        public static bool IsAnyChildUsesAlignBytes(this TransferMetaFlags _this) => (_this & TransferMetaFlags.AnyChildUsesAlignBytes) != 0;
        public static bool IsIgnoreWithInspectorUndo(this TransferMetaFlags _this) => (_this & TransferMetaFlags.IgnoreWithInspectorUndo) != 0;
        public static bool IsEditorDisplaysCharacterMap(this TransferMetaFlags _this) => (_this & TransferMetaFlags.EditorDisplaysCharacterMap) != 0;
        public static bool IsIgnoreInMetaFiles(this TransferMetaFlags _this) => (_this & TransferMetaFlags.IgnoreInMetaFiles) != 0;
        public static bool IsTransferAsArrayEntryNameInMetaFiles(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles) != 0;
        public static bool IsTransferUsingFlowMappingStyle(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferUsingFlowMappingStyle) != 0;
        public static bool IsGenerateBitwiseDifferences(this TransferMetaFlags _this) => (_this & TransferMetaFlags.GenerateBitwiseDifferences) != 0;
        public static bool IsDontAnimate(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DontAnimate) != 0;
        public static bool IsTransferHex64(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferHex64) != 0;
        public static bool IsCharPropertyMask(this TransferMetaFlags _this) => (_this & TransferMetaFlags.CharPropertyMask) != 0;
        public static bool IsDontValidateUTF8(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DontValidateUTF8) != 0;
        public static bool IsFixedBuffer(this TransferMetaFlags _this) => (_this & TransferMetaFlags.FixedBuffer) != 0;
        public static bool IsDisallowSerializedPropertyModification(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DisallowSerializedPropertyModification) != 0;
        public static IEnumerable<string> Split(this TransferMetaFlags flags)
        {
            if (flags == TransferMetaFlags.NoTransferFlags)
            {
                yield return nameof(TransferMetaFlags.NoTransferFlags);
            }
            else
            {
                if (flags.HasFlag(TransferMetaFlags.HideInEditor))
                {
                    yield return nameof(TransferMetaFlags.HideInEditor);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown1))
                {
                    yield return nameof(TransferMetaFlags.Unknown1);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown2))
                {
                    yield return nameof(TransferMetaFlags.Unknown2);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown3))
                {
                    yield return nameof(TransferMetaFlags.Unknown3);
                }

                if (flags.HasFlag(TransferMetaFlags.NotEditable))
                {
                    yield return nameof(TransferMetaFlags.NotEditable);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown5))
                {
                    yield return nameof(TransferMetaFlags.Unknown5);
                }

                if (flags.HasFlag(TransferMetaFlags.StrongPPtr))
                {
                    yield return nameof(TransferMetaFlags.StrongPPtr);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown7))
                {
                    yield return nameof(TransferMetaFlags.Unknown7);
                }

                if (flags.HasFlag(TransferMetaFlags.TreatIntegerValueAsBoolean))
                {
                    yield return nameof(TransferMetaFlags.TreatIntegerValueAsBoolean);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown9))
                {
                    yield return nameof(TransferMetaFlags.Unknown9);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown10))
                {
                    yield return nameof(TransferMetaFlags.Unknown10);
                }

                if (flags.HasFlag(TransferMetaFlags.SimpleEditor))
                {
                    yield return nameof(TransferMetaFlags.SimpleEditor);
                }

                if (flags.HasFlag(TransferMetaFlags.DebugProperty))
                {
                    yield return nameof(TransferMetaFlags.DebugProperty);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown13))
                {
                    yield return nameof(TransferMetaFlags.Unknown13);
                }

                if (flags.HasFlag(TransferMetaFlags.AlignBytes))
                {
                    yield return nameof(TransferMetaFlags.AlignBytes);
                }

                if (flags.HasFlag(TransferMetaFlags.AnyChildUsesAlignBytes))
                {
                    yield return nameof(TransferMetaFlags.AnyChildUsesAlignBytes);
                }

                if (flags.HasFlag(TransferMetaFlags.IgnoreWithInspectorUndo))
                {
                    yield return nameof(TransferMetaFlags.IgnoreWithInspectorUndo);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown17))
                {
                    yield return nameof(TransferMetaFlags.Unknown17);
                }

                if (flags.HasFlag(TransferMetaFlags.EditorDisplaysCharacterMap))
                {
                    yield return nameof(TransferMetaFlags.EditorDisplaysCharacterMap);
                }

                if (flags.HasFlag(TransferMetaFlags.IgnoreInMetaFiles))
                {
                    yield return nameof(TransferMetaFlags.IgnoreInMetaFiles);
                }

                if (flags.HasFlag(TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles))
                {
                    yield return nameof(TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles);
                }

                if (flags.HasFlag(TransferMetaFlags.TransferUsingFlowMappingStyle))
                {
                    yield return nameof(TransferMetaFlags.TransferUsingFlowMappingStyle);
                }

                if (flags.HasFlag(TransferMetaFlags.GenerateBitwiseDifferences))
                {
                    yield return nameof(TransferMetaFlags.GenerateBitwiseDifferences);
                }

                if (flags.HasFlag(TransferMetaFlags.DontAnimate))
                {
                    yield return nameof(TransferMetaFlags.DontAnimate);
                }

                if (flags.HasFlag(TransferMetaFlags.TransferHex64))
                {
                    yield return nameof(TransferMetaFlags.TransferHex64);
                }

                if (flags.HasFlag(TransferMetaFlags.CharPropertyMask))
                {
                    yield return nameof(TransferMetaFlags.CharPropertyMask);
                }

                if (flags.HasFlag(TransferMetaFlags.DontValidateUTF8))
                {
                    yield return nameof(TransferMetaFlags.DontValidateUTF8);
                }

                if (flags.HasFlag(TransferMetaFlags.FixedBuffer))
                {
                    yield return nameof(TransferMetaFlags.FixedBuffer);
                }

                if (flags.HasFlag(TransferMetaFlags.DisallowSerializedPropertyModification))
                {
                    yield return nameof(TransferMetaFlags.DisallowSerializedPropertyModification);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown29))
                {
                    yield return nameof(TransferMetaFlags.Unknown29);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown30))
                {
                    yield return nameof(TransferMetaFlags.Unknown30);
                }

                if (flags.HasFlag(TransferMetaFlags.Unknown31))
                {
                    yield return nameof(TransferMetaFlags.Unknown31);
                }
            }
        }
    }
}
