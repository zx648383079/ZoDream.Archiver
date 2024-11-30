using System;
using System.Text;
using System.Text.RegularExpressions;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal enum MiHoYoBinDataType
    {
        None,
        Bytes,
        JSON
    }
    internal sealed class MiHoYoBinData : UIObject, IFileWriter
    {
        private static Regex ASCII = new Regex("[^\u0020-\u007E]", RegexOptions.Compiled);

        public static bool Exportable;
        public static bool Encrypted;
        public static byte Key;

        public byte[] RawData;

        public MiHoYoBinData(UIReader reader) : base(reader)
        {
            RawData = reader.ReadArray(r => r.ReadByte());
        }

        //public string AsString => Type switch
        //{
        //    MiHoYoBinDataType.JSON => JToken.Parse(DataStr).ToString(Formatting.Indented),
        //    MiHoYoBinDataType.Bytes => ASCII.Replace(DataStr, string.Empty),
        //    _ => "",
        //};
        //public new object Dump() => Type switch
        //{
        //    MiHoYoBinDataType.JSON => AsString,
        //    MiHoYoBinDataType.Bytes => Data,
        //    _ => null,
        //};

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            // TODO
        }

        private string DataStr => Encoding.UTF8.GetString(Data);

        //public MiHoYoBinDataType Type
        //{
        //    get
        //    {
        //        try
        //        {
        //            var asToken = JToken.Parse(DataStr);
        //            if (asToken.Type == JTokenType.Object || asToken.Type == JTokenType.Array)
        //                return MiHoYoBinDataType.JSON;
        //        }
        //        catch (Exception)
        //        {
        //            return MiHoYoBinDataType.Bytes;
        //        }
        //        return MiHoYoBinDataType.None;
        //    }
        //}

        private byte[] Data
        {
            get
            {
                if (Encrypted)
                {
                    byte[] bytes = new byte[RawData.Length];
                    for (int i = 0; i < RawData.Length; i++)
                    {
                        bytes[i] = (byte)(RawData[i] ^ Key);
                    }
                    return bytes;
                }
                else return RawData;
            }
        }
    }
}
