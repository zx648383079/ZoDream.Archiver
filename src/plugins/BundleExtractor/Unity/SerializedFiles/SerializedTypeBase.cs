﻿using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public abstract class SerializedTypeBase
    {
        public int TypeID
        {
            get
            {
                return RawTypeID;
            }
            set
            {
                RawTypeID = value;
            }
        }

        /// <summary>
        /// For versions less than 17, it specifies <see cref="TypeID"/> or -<see cref="ScriptTypeIndex"/> -1 for MonoBehaviour
        /// </summary>
        public int OriginalTypeID
        {
            get
            {
                return RawTypeID;
            }
            set
            {
                RawTypeID = value;
            }
        }
        public int RawTypeID { get; set; }
        public bool IsStrippedType { get; set; }
        /// <summary>
        /// For <see cref="ElementIDType.MonoBehaviour"/> specifies script type
        /// </summary>
        public short ScriptTypeIndex { get; set; }
        /// <summary>
        /// The type of the class.
        /// </summary>
        public VirtualDocument OldType { get; } = new();
        /// <summary>
        /// Hash128
        /// </summary>
        public byte[] ScriptID { get; set; } = [];
        public byte[] OldTypeHash { get; set; } = [];

        public void Read(IBundleBinaryReader reader,
            bool hasTypeTree)
        {
            var version = reader.Get<FormatVersion>();
            RawTypeID = reader.ReadInt32();
            int typeIdLocal;
            if (version < FormatVersion.RefactoredClassId)
            {
                typeIdLocal = RawTypeID < 0 ? -1 : RawTypeID;
                IsStrippedType = false;
                ScriptTypeIndex = -1;
            }
            else
            {
                typeIdLocal = RawTypeID;
                IsStrippedType = reader.ReadBoolean();
            }

            if (version >= FormatVersion.RefactorTypeData)
            {
                ScriptTypeIndex = reader.ReadInt16();
            }

            if (version >= FormatVersion.HasTypeTreeHashes)
            {
                bool readScriptID = typeIdLocal == -1
                    || typeIdLocal == 114
                    || !IgnoreScriptTypeForHash(version, reader.Get<Version>()) && ScriptTypeIndex >= 0;
                if (readScriptID)
                {
                    ScriptID = reader.ReadBytes(16);//actually read as 4 uint
                }
                OldTypeHash = reader.ReadBytes(16);//actually read as 4 uint
            }

            if (hasTypeTree)
            {
                DocumentConverter.Read(OldType, reader);
                if (version < FormatVersion.HasTypeTreeHashes)
                {
                    //OldTypeHash gets recalculated here in a complicated way on 2023.
                }
                else if (version >= FormatVersion.StoresTypeDependencies)
                {
                    ReadTypeDependencies(reader);
                }
            }
        }

        protected abstract void ReadTypeDependencies(IBundleBinaryReader reader);

        protected abstract bool IgnoreScriptTypeForHash(FormatVersion formatVersion, Version unityVersion);

        public override string ToString()
        {
            return TypeID.ToString();
        }

        /// <summary>
        /// 5.5.0a and greater, ie format version 16+
        /// </summary>
        public static bool HasIsStrippedType(FormatVersion generation) => generation >= FormatVersion.RefactoredClassId;
        /// <summary>
        /// 5.5.0 and greater, ie format version 17+
        /// </summary>
        public static bool HasScriptTypeIndex(FormatVersion generation) => generation >= FormatVersion.RefactorTypeData;
        /// <summary>
        /// 5.0.0unk2 and greater, ie format version 13+
        /// </summary>
        public static bool HasHash(FormatVersion generation) => generation >= FormatVersion.HasTypeTreeHashes;
        /// <summary>
        /// 2019.3 and greater, ie format version 21+
        /// </summary>
        public static bool HasTypeDependencies(FormatVersion generation) => generation >= FormatVersion.StoresTypeDependencies;
    }
}
