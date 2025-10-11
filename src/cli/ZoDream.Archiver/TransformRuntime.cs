using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.Archiver
{
    public class TransformRuntime(string rootFolder) : IConsoleRuntime
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
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]Info:{item[rootFolder.Length..]}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]Error:{ex.Message}");
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
    }
}
