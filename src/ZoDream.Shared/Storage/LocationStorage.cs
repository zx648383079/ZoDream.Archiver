using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Storage
{
    public static class LocationStorage
    {
        /// <summary>
        /// 根据文件路径创建缺少的文件夹
        /// </summary>
        /// <param name="fileName"></param>
        public static void CreateDirectory(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// 根据模式是否能创建文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static bool TryCreate(string fileName, ArchiveExtractMode mode, out string fullPath)
        {
            return TryCreate(fileName, Path.GetExtension(fileName), mode, out fullPath);
        }
        /// <summary>
        /// 根据模式是否能创建文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <param name="mode"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static bool TryCreate(string fileName, string extension, ArchiveExtractMode mode, out string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(extension) && fileName.EndsWith(extension))
            {
                fileName = fileName[..^extension.Length];
            }
            fullPath = fileName + extension;
            if (mode == ArchiveExtractMode.Overwrite || !File.Exists(fullPath))
            {
                CreateDirectory(fullPath);
                return true;
            }
            if (mode == ArchiveExtractMode.Skip)
            {
                return false;
            }
            if (mode == ArchiveExtractMode.Rename)
            {
                for (var i = 1;; i++)
                {
                    fullPath = $"{fileName}({i}){extension}";
                    if (!File.Exists(fullPath))
                    {
                        CreateDirectory(fullPath);
                        return true;
                    }
                }
            }
            // TODO 询问是否覆盖
            return false;
        }

        /// <summary>
        /// 读文本文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<string> ReadAsync(string file)
        {
            if (!File.Exists(file))
            {
                return string.Empty;
            }
            var fs = new FileStream(file, FileMode.Open);
            using var reader = new StreamReader(fs, TxtEncoder.GetEncoding(fs));
            var content = await reader.ReadToEndAsync();
            return content;
        }

        public static StreamReader Reader(string file)
        {
            var fs = new FileStream(file, FileMode.Open);
            return new StreamReader(fs, TxtEncoder.GetEncoding(fs));
        }

        /// <summary>
        /// 写文本文件 默认使用无 bom 的UTF8编码
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        public static async Task WriteAsync(string file, string content)
        {
            await WriteAsync(file, content, new UTF8Encoding(false));
        }

        public static async Task WriteAsync(string file, string content, bool append)
        {
            await WriteAsync(file, content, new UTF8Encoding(false), append);
        }

        public static async Task WriteAsync(string file, string content, string encoding)
        {
            await WriteAsync(file, content, Encoding.GetEncoding(encoding));
        }

        public static async Task WriteAsync(string file, string content, Encoding encoding)
        {
            await WriteAsync(file, content, encoding, false);
        }

        public static async Task WriteAsync(string file, string content, Encoding encoding, bool append)
        {
            using var writer = new StreamWriter(file, append, encoding);
            await writer.WriteAsync(content);
        }

        public static StreamWriter Writer(string file)
        {
            return Writer(file, false);
        }

        public static StreamWriter Writer(string file, bool append)
        {
            FileStream fs;
            if (!append)
            {
                fs = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                return new StreamWriter(fs, Encoding.UTF8);
            }
            fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var encoding = TxtEncoder.GetEncoding(fs);
            fs.Seek(0, SeekOrigin.End);
            return new StreamWriter(fs, encoding);
        }


        public static string GetMD5(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return string.Empty;
            }
            using var fs = new FileStream(fileName, FileMode.Open);
            return GetMD5(fs);
        }

        public static string GetMD5(Stream fs)
        {
            var md5 = MD5.Create();
            var res = md5.ComputeHash(fs);
            var sb = new StringBuilder();
            foreach (var b in res)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
