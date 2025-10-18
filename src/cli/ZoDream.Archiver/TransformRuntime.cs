using System.Buffers;
using System.Text.RegularExpressions;
using ZoDream.BundleExtractor;
using ZoDream.BundleExtractor.Unity.PlayerAsset;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver
{
    public class TransformRuntime(string rootFolder, BundleOptions options) : IConsoleRuntime
    {
        
        public Task RunAsync(CancellationToken token = default)
        {
            Task.Factory.StartNew(() => WriteSpinner(token), token);
            return Task.Factory.StartNew(() => {
                Run(token);
            }, token);
        }

        public void Run(CancellationToken token = default)
        {
            var package = PackageArguments.Create(options.Package);
            if (package.Contains("delete"))
            {
                DeleteFile(token);
            }
            if (package.Contains("fake"))
            {
                FakeFile(token);
            }
            if (package.Contains("player"))
            {
                ExtractFile(token);
            }
            if (package.Contains("qoo"))
            {
                Decrypt(token);
            }
        }
        private void ExtractFile(CancellationToken token = default)
        {
            var source = new BundleSource([rootFolder]);
            using var hander = new PlayerAssetScheme(source);
            hander.ExtractTo(options.OutputFolder, ArchiveExtractMode.Overwrite, token);
            WriteLine("Extract Finish!");
        }

        private void FakeFile(CancellationToken token = default)
        {
            var count = 0;
            foreach (var item in Directory.GetFiles(rootFolder, "*.bundle", SearchOption.AllDirectories))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var fs = File.Open(item, FileMode.Open, FileAccess.ReadWrite);
                var res = OtherBundleElementScanner.ParseFakeHeader(fs);
                if (res == fs)
                {
                    continue;
                }
                CopyTo(res, fs);
                WriteLine("Repair", item);
                count++;
            }
            WriteLine($"Repair {count} Finish!");
        }

        private static void CopyTo(Stream input, Stream output)
        {
            const int bufferSize = 4096;
            if (input.Length == 0)
            {
                return;
            }
            var chunkSize = (int)Math.Min(bufferSize, input.Length);
            var buffer = ArrayPool<byte>.Shared.Rent(chunkSize);
            try
            {
                var i = input.Position;
                var o = 0;
                while (i < input.Length)
                {
                    input.Position = i;
                    var readLen = input.Read(buffer, 0, chunkSize);
                    if (readLen == 0)
                    {
                        break;
                    }
                    output.Position = o;
                    output.Write(buffer, 0, readLen);
                    i += readLen;
                    o += readLen;
                    if (readLen <  chunkSize)
                    {
                        break;
                    }
                }
                output.Flush();
                output.SetLength(o);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void DeleteFile(CancellationToken token = default)
        {
            var regex = new Regex(@"^-?\d{16,}\.json$");
            var count = 0;
            foreach (var item in Directory.GetFiles(rootFolder, "*.json", SearchOption.AllDirectories))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (regex.IsMatch(Path.GetFileName(item)))
                {
                    File.Delete(item);
                    WriteLine("Delete", item);
                    count++;
                }
            }
            WriteLine($"Delete {count} Finish!");
        }

        private void Decrypt(CancellationToken token = default)
        {
            var buffer = new byte[3];
            var signature = new byte[] { 0xEF, 0xBB, 0xBF };
            foreach (var item in Directory.GetFiles(rootFolder, "*.txt", SearchOption.AllDirectories))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var fs = File.Open(item, FileMode.Open, FileAccess.ReadWrite);
                if (fs.Length <= buffer.Length)
                {
                    continue;
                }
                fs.ReadExactly(buffer);
                if (!buffer.Equal(signature))
                {
                    continue;
                }
                fs.Position = 0;
                try
                {
                    using var os = QooElementScanner.Decrypt(fs);
                    fs.Position = 0;
                    os.CopyTo(fs);
                    fs.SetLength(fs.Position);
                    WriteLine("Info", item[rootFolder.Length..]);
                }
                catch (Exception ex)
                {
                    WriteLine("Error", ex.Message);
                }
            }
        }

        private static void WriteSpinner(CancellationToken token)
        {
            var counter = 0;
            while (!token.IsCancellationRequested)
            {
                WriteTopLine($"[{ConsoleLogger.GetSpinner(counter ++)}] 处理中...");
                Thread.Sleep(100);
            }
        }

        private static void WriteTopLine(string message, int lineIndex = 0)
        {
            lock (Console.Out)
            {
                int originalLeft = Console.CursorLeft;
                int originalTop = Console.CursorTop;
                Console.SetCursorPosition(0, lineIndex + 1);
                Console.Write(message);
                if (message.Length < Console.BufferWidth)
                {
                    Console.Write(new string(' ', Console.BufferWidth - message.Length));
                }
                Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        private static void WriteLine(string status, string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{status}: {message}");
        }
        private static void WriteLine(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{message}");
        }
    }
}
