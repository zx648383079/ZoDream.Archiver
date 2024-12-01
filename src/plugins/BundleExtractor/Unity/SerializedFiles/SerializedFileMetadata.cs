using ZoDream.BundleExtractor.Models;
using System.IO;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class SerializedFileMetadata
    {
        public UnityVersion UnityVersion { get; set; }
        public BuildTarget TargetPlatform { get; set; }
        public bool EnableTypeTree { get; set; }
        public SerializedType[] Types { get; set; } = [];
        /// <summary>
        /// Indicate that <see cref="ObjectInfo.FileID"/> is 8 bytes size<br/>
        /// Serialized files with this field enabled supposedly don't exist
        /// </summary>
        public uint LongFileID { get; set; }
        public bool SwapEndian { get; set; }
        public ObjectInfo[] Object { get; set; } = [];
        public LocalSerializedObjectIdentifier[] ScriptTypes { get; set; } = [];
        public FileIdentifier[] Externals { get; set; } = [];
        public string UserInformation { get; set; } = string.Empty;
        public SerializedTypeReference[] RefTypes { get; set; } = [];


        /// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasEndian(FormatVersion generation) => generation < FormatVersion.Unknown_9;
        /// <summary>
        /// Less than 3.5.0
        /// </summary>
        public static bool IsMetadataAtTheEnd(FormatVersion generation) => generation < FormatVersion.Unknown_9;

        /// <summary>
        /// 3.0.0b and greater
        /// </summary>
        public static bool HasSignature(FormatVersion generation) => generation >= FormatVersion.Unknown_7;
        /// <summary>
        /// 3.0.0 and greater
        /// </summary>
        public static bool HasPlatform(FormatVersion generation) => generation >= FormatVersion.Unknown_8;
        /// <summary>
        /// 5.0.0Unk2 and greater
        /// </summary>
        public static bool HasEnableTypeTree(FormatVersion generation) => generation >= FormatVersion.HasTypeTreeHashes;
        /// <summary>
        /// 3.0.0b to 4.x.x
        /// </summary>
        public static bool HasLongFileID(FormatVersion generation) => generation >= FormatVersion.Unknown_7 && generation < FormatVersion.Unknown_14;
        /// <summary>
        /// 5.0.0Unk0 and greater
        /// </summary>
        public static bool HasScriptTypes(FormatVersion generation) => generation >= FormatVersion.HasScriptTypeIndex;
        /// <summary>
        /// 1.2.0 and greater
        /// </summary>
        public static bool HasUserInformation(FormatVersion generation) => generation >= FormatVersion.Unknown_5;
        /// <summary>
        /// 2019.2 and greater
        /// </summary>
        public static bool HasRefTypes(FormatVersion generation) => generation >= FormatVersion.SupportsRefObject;

        public void Read(Stream stream, SerializedFileHeader header)
        {
            bool swapEndian = ReadSwapEndian(stream, header);
            EndianType endian = swapEndian ? EndianType.BigEndian : EndianType.LittleEndian;
            using var reader = new BundleBinaryReader(stream, endian);
            reader.Add(header.Version);
            Read(reader);
        }

        private bool ReadSwapEndian(Stream stream, SerializedFileHeader header)
        {
            if (HasEndian(header.Version))
            {
                int num = stream.ReadByte();
                //This is not and should not be aligned.
                //Alignment only happens for the endian boolean on version 9 and greater.
                //This coincides with endian being stored in the header on version 9 and greater.
                return num switch
                {
                    < 0 => throw new EndOfStreamException(),
                    _ => SwapEndian = num != 0,
                };
            }
            else
            {
                return header.Endian;
            }
        }

        private void Read(IBundleBinaryReader reader)
        {
            var generation = reader.Get<FormatVersion>();
            if (HasSignature(generation))
            {
                string signature = reader.ReadStringZeroTerm();
                UnityVersion = UnityVersion.Parse(signature);
                reader.Add(UnityVersion);
            }
            if (HasPlatform(generation))
            {
                TargetPlatform = (BuildTarget)reader.ReadUInt32();
            }

            EnableTypeTree = ReadEnableTypeTree(reader);

            Types = reader.ReadArray(() =>
            {
                var o = new SerializedType();
                o.Read(reader, UnityVersion, EnableTypeTree);
                return o;
            });

            if (HasLongFileID(generation))
            {
                LongFileID = reader.ReadUInt32();
            }

            //TODO: pass LongFileID to ObjectInfo
            Object = reader.ReadArray(() =>
            {
                var o = new ObjectInfo();
                o.Read(reader);
                return o;
            });

            if (HasScriptTypes(generation))
            {
                ScriptTypes = reader.ReadArray(() =>
                {
                    var o = new LocalSerializedObjectIdentifier();
                    o.Read(reader);
                    return o;
                });
            }

            Externals = reader.ReadArray(() =>
            {
                var o = new FileIdentifier();
                o.Read(reader);
                return o;
            });

            if (HasRefTypes(generation))
            {
                RefTypes = reader.ReadArray(() =>
                {
                    var o = new SerializedTypeReference();
                    o.Read(reader, UnityVersion, EnableTypeTree);
                    return o;
                });
            }
            if (HasUserInformation(generation))
            {
                UserInformation = reader.ReadStringZeroTerm();
            }
        }

        private static bool ReadEnableTypeTree(IBundleBinaryReader reader)
        {
            if (HasEnableTypeTree(reader.Get<FormatVersion>()))
            {
                return reader.ReadBoolean();
            }
            else
            {
                return true;
            }
        }
    }
}
